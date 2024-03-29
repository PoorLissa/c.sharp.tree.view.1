﻿using myControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
    Front-facing class that wraps myTree and myDataGrid objects.
    Provides all the necessary infrastructure for them to work together.
    
    To use it, put the components declared in [myTree_DataGrid_Manager_Initializer] structure on a form,
    fill in the structure and pass it as a parameter to the [myTree_DataGrid_Manager] constructor:

            var mtdgmi = new myTree_DataGrid_Manager_Initializer();

            mtdgmi.form = this;
            mtdgmi.tv = treeView1;
            mtdgmi.dg = dataGridView1;
            mtdgmi.cb_Recursive = cb_Recursive;
            mtdgmi.cb_ShowDirs  = cb_ShowDirs;
            mtdgmi.cb_ShowFiles = cb_ShowFiles;
            mtdgmi.tb_Filter = textBox1;

            myTDGManager = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);

    The ultimate goal here is to obtain the list of selected files.
    To do this, call either of 'getSelectedFiles' methods.

    In case the files in the list above have changed, use 'update' method to reflect those changes in the widgets.
*/



public struct myTree_DataGrid_Manager_Initializer
{
    public Form             form;
    public TreeView         tv;
    public myDataGridView   dg;
    public CheckBox         cb_ShowFiles;
    public CheckBox         cb_ShowDirs;
    public CheckBox         cb_Recursive;
    public CheckBox         cb_FilterPath;
    public myTextBox        tb_Filter;
    public myTextBox        tb_FilterOut;
    public RichTextBox      richTextBox;
}



interface ImyTree_DataGrid_Manager
{
    void getSelectedFiles(List<myTreeListDataItem> list);           // Obtain the list of selected files
    List<myTreeListDataItem> getSelectedFiles(bool asCopy);         // Obtain the list of selected files
    void update(List<myTreeListDataItem> list, bool u1, bool u2);   // Update all the widgets' state using the data from [updatedList]
    void allowBackup(bool mode);                                    // Allow or disallow the use of backup
};



public class myTree_DataGrid_Manager : ImyTree_DataGrid_Manager
{
    // --------------------------------------------------------------------------------

    private Form                _form          = null;
    private myTree_Wrapper      _tree          = null;
    private myDataGrid_Wrapper  _dataGrid      = null;
    private CheckBox            _cb_ShowFiles  = null;
    private CheckBox            _cb_ShowDirs   = null;
    private CheckBox            _cb_Recursive  = null;
    private CheckBox            _cb_FilterPath = null;
    private myTextBox           _tb_Filter     = null;
    private myTextBox           _tb_FilterOut  = null;
    private RichTextBox         _richTextBox   = null;

    private List<myTreeListDataItem> _visibleList = null;
    private List<myTreeListDataItem> _globalFileListExt = null;     // Stores all the folders/files found in the last [nodeSelected] call
    private List<int>                _grid_SelectedRows = null;     // Stores selected rows between calls to 'tree_CellMouseDown' and 'tree_grid_onMouseClick'

    private myBackup _backup = null;                                // Keeps the history of all changes made to the file names

    private bool _doShowDirs   = true;
    private bool _doShowFiles  = true;
    private bool _useRecursion = false;
    private bool _useBackup    = true;                              // Allow the use of backups
    private bool _useTasks     = true;                              // Defines if async Tasks should be used for building file tree. If [true], build process can be cancelled
    private bool _visibleListNeedsRefreshing = true;

    private int  _nDirs;                                            // Stores the number of folders found in the last [nodeSelected] call
    private int  _nFiles;                                           // Stores the number of files found in the last [nodeSelected] call

    private string _filterStr = "";                                 // To use with filtering event
    private string _filterOutStr = "";                              // To use with filtering event
    private int    _filterDelayCnt = 0;                             // To use with filtering event (to delay filtering)

    private CancellationTokenSource _tokenSource = null;
    private Task        _tree_onAfterSelect_Task = null;

    // --------------------------------------------------------------------------------

    public myTree_DataGrid_Manager(ref myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        _nDirs  = 0;
        _nFiles = 0;

        _globalFileListExt = new List<myTreeListDataItem>();

        _form          = mtdgmi.form;
        _cb_ShowFiles  = mtdgmi.cb_ShowFiles;
        _cb_ShowDirs   = mtdgmi.cb_ShowDirs;
        _cb_Recursive  = mtdgmi.cb_Recursive;
        _cb_FilterPath = mtdgmi.cb_FilterPath;
        _tb_Filter     = mtdgmi.tb_Filter;
        _tb_FilterOut  = mtdgmi.tb_FilterOut;
        _richTextBox   = mtdgmi.richTextBox;

        _cb_ShowFiles.Checked = true;
        _cb_ShowDirs.Checked  = true;
        _cb_Recursive.Checked = false;

        _tb_Filter.PlaceholderText = "Filter";
        _tb_FilterOut.PlaceholderText = "Filter Out";

        _tree = new myTree_Wrapper(mtdgmi.tv, _globalFileListExt, path, expandEmpty, _richTextBox);
        _dataGrid = new myDataGrid_Wrapper(mtdgmi.dg, _globalFileListExt);

        // Set up events for the components:
        _tree.Obj().AfterSelect    += new TreeViewEventHandler        (tree_onAfterSelect);
        _tree.Obj().BeforeExpand   += new TreeViewCancelEventHandler  (tree_onBeforeExpand);
        _tree.Obj().AfterExpand    += new TreeViewEventHandler        (tree_onAfterExpand);
        _tree.Obj().PreviewKeyDown += new PreviewKeyDownEventHandler  (tree_grid_onPreviewKeyDown);
        _tree.Obj().MouseClick     += new MouseEventHandler           (tree_grid_onMouseClick);

        _dataGrid.Obj().CellMouseEnter   += new DataGridViewCellEventHandler      (dataGrid_CellMouseEnter);
        _dataGrid.Obj().RowsAdded        += new DataGridViewRowsAddedEventHandler (dataGrid_RowsAdded);
        _dataGrid.Obj().Scroll           += new ScrollEventHandler                (dataGrid_Scroll);
        _dataGrid.Obj().CellValueChanged += new DataGridViewCellEventHandler      (dataGrid_CellValueChanged);
        _dataGrid.Obj().PreviewKeyDown   += new PreviewKeyDownEventHandler        (tree_grid_onPreviewKeyDown);
        _dataGrid.Obj().MouseClick       += new MouseEventHandler                 (tree_grid_onMouseClick);
        _dataGrid.Obj().CellMouseDown    += new DataGridViewCellMouseEventHandler (tree_CellMouseDown);

        var cb_CommonHandler = new EventHandler(cb_onCheckedChanged_Commmon_Handler);

        _cb_ShowDirs.CheckedChanged   += cb_CommonHandler;
        _cb_ShowFiles.CheckedChanged  += cb_CommonHandler;
        _cb_Recursive.CheckedChanged  += cb_CommonHandler;
        _cb_FilterPath.CheckedChanged += cb_CommonHandler;

        _tb_Filter.Obj().TextChanged    += new EventHandler(tb_Filter_onTextChanged);
        _tb_FilterOut.Obj().TextChanged += new EventHandler(tb_Filter_onTextChanged);

        _form.FormClosing += new FormClosingEventHandler(_form_onFormClosing);

        _tree.Obj().TabIndex = 0;
        _dataGrid.Obj().TabIndex = 1;
    }

    // --------------------------------------------------------------------------------

    public ref myTree_Wrapper getTree()
    {
        return ref _tree;
    }

    // --------------------------------------------------------------------------------

    public ref myDataGrid_Wrapper getGrid()
    {
        return ref _dataGrid;
    }

    // --------------------------------------------------------------------------------

    // Public interface method: populate list with currently selected files
    public void getSelectedFiles(List<myTreeListDataItem> filesList)
    {
        _dataGrid.getSelectedFiles(filesList);
    }

    // --------------------------------------------------------------------------------

    // Public interface method: get list of currently selected files
    public List<myTreeListDataItem> getSelectedFiles(bool asCopy)
    {
        var list = new List<myTreeListDataItem>();

        _dataGrid.getSelectedFiles(list);

        if (asCopy)
        {
            var copy = new List<myTreeListDataItem>(list.Count);

            for (int i = 0; i < list.Count; i++)
                copy.Add(list[i].Clone());

            list = copy;
        }

        return list;
    }

    // --------------------------------------------------------------------------------

    public void refresh()
    {
        // To restore focus later on
        if (_dataGrid.Obj().Focused)
        {
            _tree.Obj().SelectedNode.ImageIndex = -3;
        }

        tree_onAfterSelect(this, null);
    }

    // --------------------------------------------------------------------------------

    // Process keyboad events captured outside of this object
    public bool processExternalKeyDown(Keys key)
    {
        return _dataGrid.processExternalKeyDown(key);
    }

    // --------------------------------------------------------------------------------

    // Get list of currently visible files
    private List<myTreeListDataItem> getVisibleFiles(bool asCopy)
    {
        var list = new List<myTreeListDataItem>();

        _dataGrid.getVisibleFiles(list);

        if (asCopy)
        {
            var copy = new List<myTreeListDataItem>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    copy.Add(list[i].Clone());
                }
                else
                {
                    copy.Add(null);
                }
            }

            list = copy;
        }

        return list;
    }

    // --------------------------------------------------------------------------------

    public void allowBackup(bool mode)
    {
        _useBackup = mode;
    }

    // --------------------------------------------------------------------------------

    // Finalizing method. Allows to save state when the app is about to close
    private void finalize()
    {
        if (_backup != null)
        {
            _backup.saveHistoryToFile();
        }
    }

    // --------------------------------------------------------------------------------

    // Selecting a tree node (using mouse or keyboard)
    private void tree_onAfterSelect(object sender, TreeViewEventArgs e)
    {
        _dataGrid.Enable(false);


        // Decide if the current directory in the tree has changed or not
        // (if not, the checked files in the grid will be restored later, when the grid is repopulated)
        myDataGrid_Wrapper.PopulateReason reason = myDataGrid_Wrapper.PopulateReason.viewDirChanged;


        if (_useTasks)
        {
            if (sender != null)
            {
                // sender == _cb_Recursive -- We're changing the state of [cb_Recursive] checkbox
                // sender != _cb_Recursive -- We're actually clicking the node in the tree
                //                         -- Or F5 key has been pressed

                // Hope this local variables will be treated properly by the task...
                TreeNode selectedNode = (sender == _cb_Recursive || sender == _cb_FilterPath || sender == this)
                    ? _tree.Obj().SelectedNode
                    : e.Node;

                reason = (sender == _cb_Recursive)
                    ? myDataGrid_Wrapper.PopulateReason.recursionChanged_After
                    : myDataGrid_Wrapper.PopulateReason.dirChanged;


                // Set Form's header text
                _form.Text = "pmvRenamer : " + selectedNode.FullPath;


                // This means, we will have to execute _tree.nodeSelected, which may take a LONG time
                if (_tree_onAfterSelect_Task == null || _tree_onAfterSelect_Task.IsCompleted)
                {
                    _tokenSource?.Dispose();
                }
                else
                {
                    // Cancel current task
                    _tokenSource.Cancel();

                    try
                    {
                        // Wait for current task to actually finish:
                        // Waiting on a cancelled task results in exception thrown
                        _tree_onAfterSelect_Task.Wait();
                    }
                    catch (AggregateException)
                    {
                        _globalFileListExt.Clear();
                    }
                    finally
                    {
                        _tokenSource.Dispose();
                    }
                }


                // Start new task
                _tokenSource = new System.Threading.CancellationTokenSource();


                // Create a task
                _tree_onAfterSelect_Task = new System.Threading.Tasks.Task(() =>
                {
                    bool doShowNextMsg = false;

                    _tokenSource.Token.ThrowIfCancellationRequested();

                    // Create child task and Start it immediately
                    new System.Threading.Tasks.Task(() =>
                    {
                        // Wait a bit and then signal _dataGrid to draw some text over the disabled DataGridView widget
                        for (int i = 0; _tree_onAfterSelect_Task.Status == TaskStatus.Running && i < 6; i++)
                        {
                            // Check for cancellation token
                            _tokenSource.Token.ThrowIfCancellationRequested();

                            Task.Delay(100).Wait();
                        }

                        if (_tree_onAfterSelect_Task.Status == TaskStatus.Running)
                        {
                            // Go ahead and signal the _dataGrid
                            _dataGrid.Obj().Invoke(new MethodInvoker(delegate { _dataGrid.displayRecursionMsg("Recursive Search in Progress..."); }));
                            doShowNextMsg = true;
                        }

                    }, _tokenSource.Token, TaskCreationOptions.AttachedToParent).Start();


                    // Get all the directory and file names
                    _tree.nodeSelected_Cancellable(selectedNode, ref _nDirs, ref _nFiles, _tokenSource.Token, _useRecursion);


                    _tokenSource.Token.ThrowIfCancellationRequested();

                    if (doShowNextMsg)
                    {
                        _dataGrid.Obj().Invoke(new MethodInvoker(delegate
                        {
                            _dataGrid.displayRecursionMsg(
                                $"Populating the List:\n Total {_nDirs + _nFiles} items found");
                        }));
                    }

                    _dataGrid.Obj().Invoke(new MethodInvoker(delegate
                    {
                        _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _tree.Obj().SelectedNode, _filterStr, _filterOutStr);
                        _dataGrid.Enable(true);

                        // selectedNode.ImageIndex == -2 means, the caller is myTree.setPath()
                        //   In this case, we know the user launched the app with a non-empty path param, and we want to make DataGrid selected immediately
                        // selectedNode.ImageIndex == -3 means, the caller is myTree_DataGrid_Manager.refresh()
                        //   In this case, we know F5 was hit when DataGrid was focused, and we want to make DataGrid selected again
                        if (selectedNode.ImageIndex < -1)
                        {
                            selectedNode.ImageIndex = -1;

                            _dataGrid.Obj().Focus();
                            _dataGrid.setTabFocus(true);

                            if(_dataGrid.Obj().Rows.Count > 0)
                                _dataGrid.Obj().Rows[0].Selected = true;
                        }

                    }));

                }, _tokenSource.Token);


                // Execute the task
                _tree_onAfterSelect_Task.Start();
            }
            else
            {
                // This part was initially just a direct call without a task;
                // But now it is wrapped in a task because when it is waiting for a previous task to complete, it becomes completely unresponsive;
                // This way, we're able to change the directory while waiting, and the tasks are cancelled just fine

                new System.Threading.Tasks.Task(() =>
                {
                    // In case the previous task is still running, we need to wait for it to finish;
                    // Without this, the following scenario fails:
                    // - select a folder with large amount of files
                    // - check 'recursive' checkbox
                    // - immediately uncheck 'show directories' checkbox
                    if (_tree_onAfterSelect_Task != null)
                        _tree_onAfterSelect_Task.Wait(_tokenSource.Token);

                    if (_tokenSource.Token.IsCancellationRequested == false)
                    {
                        // This call is going to work on a already populated list
                        _dataGrid.Obj().Invoke(new MethodInvoker(delegate {
                            _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _tree.Obj().SelectedNode, _filterStr, _filterOutStr);
                            _dataGrid.Enable(true);
                        }));
                    }

                }).Start();
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    // Expanding tree node -- Before
    private void tree_onBeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "[?]")
        {
            // To remove/reduce flickering when the node is opened for the first time
            _tree.Obj().BeginUpdate();
        }

        _tree.AllowRedrawing(false);
        _tree.nodeExpanded_Before(e.Node);
    }

    // --------------------------------------------------------------------------------

    // Expanding tree node -- After
    private void tree_onAfterExpand(object sender, TreeViewEventArgs e)
    {
        _tree.Obj().EndUpdate();
        _tree.AllowRedrawing(true);
    }

    // --------------------------------------------------------------------------------

    // Display tooltip on mouse hover: this row's file name with all the selected options applied
    private void dataGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    {
        var row = _dataGrid.Obj().Rows[e.RowIndex];
        bool isChecked = (bool)(row.Cells[(int)myDataGrid_Wrapper.Columns.colChBox].Value);

        if (isChecked)
        {
            // Careful, this is NOT A COPY:
            if (_visibleListNeedsRefreshing)
            {
                _visibleList = getVisibleFiles(asCopy: false);
                _visibleListNeedsRefreshing = false;
            }

            int firstDisplayedRowIndex = _dataGrid.Obj().FirstDisplayedCell.RowIndex;
            var id = e.RowIndex - firstDisplayedRowIndex;

            if (_visibleList[id] != null)
            {
                // Get simulated name and display it
                _dataGrid.setSimulatedName(myRenamer.getInstance().getSimulatedName(_visibleList[id]));
                _dataGrid.Obj().InvalidateRow(e.RowIndex);
            }
        }
        else
        {
            _dataGrid.setSimulatedName(null);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    public void dataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        _visibleListNeedsRefreshing = true;
    }

    // --------------------------------------------------------------------------------

    public void dataGrid_Scroll(object sender, ScrollEventArgs e)
    {
        _visibleListNeedsRefreshing = true;
    }

    // --------------------------------------------------------------------------------

    public void dataGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        _visibleListNeedsRefreshing = true;
    }

    // --------------------------------------------------------------------------------

    // Common handler for every checkbox
    private void cb_onCheckedChanged_Commmon_Handler(object sender, EventArgs e)
    {
        CheckBox cb = sender as CheckBox;

        if (cb != null)
        {
            // Show or hide directories
            if (cb == _cb_ShowDirs)
            {
                _doShowDirs = _cb_ShowDirs.Checked;
                tree_onAfterSelect(null, null);
            }

            // Show or hide files
            if (cb == _cb_ShowFiles)
            {
                _doShowFiles = _cb_ShowFiles.Checked;
                tree_onAfterSelect(null, null);
            }

            // Recursive search enabled/disabled
            if (cb == _cb_Recursive)
            {
                _useRecursion = _cb_Recursive.Checked;
                _dataGrid.setRecursiveMode(_useRecursion);
                tree_onAfterSelect(sender, null);
            }

            if (cb == _cb_FilterPath)
            {
                _dataGrid.setFilterMode(cb.Checked);
                tree_onAfterSelect(_useRecursion ? sender : null, null);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    // Show or hide directories -- not used
    private void cb_ShowDirs_onCheckedChanged(object sender, EventArgs e)
    {
        _doShowDirs = _cb_ShowDirs.Checked;
        tree_onAfterSelect(null, null);
    }

    // --------------------------------------------------------------------------------

    // Show or hide files -- not used
    private void cb_ShowFiles_onCheckedChanged(object sender, EventArgs e)
    {
        _doShowFiles = _cb_ShowFiles.Checked;
        tree_onAfterSelect(null, null);
    }

    // --------------------------------------------------------------------------------

    // Recursive search enabled/disabled -- not used
    private void cb_Recursive_onCheckedChanged(object sender, EventArgs e)
    {
        _useRecursion = _cb_Recursive.Checked;

        _dataGrid.setRecursiveMode(_useRecursion);

        tree_onAfterSelect(sender, null);

        return;
    }

    // --------------------------------------------------------------------------------

    private void tree_grid_onPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        switch (e.KeyCode)
        {
            // Tab between TreeView and DataGrid
            case Keys.Tab: {

                    if (sender is TreeView)
                    {
                        // Leave the TreeView focused in case the Grid is empty
                        if (_dataGrid.Obj().Rows.Count == 0)
                        {
                            e.IsInputKey = true;
                        }
                        else
                        {
                            if (_dataGrid.Obj().SelectedRows.Count == 0)
                                _dataGrid.Obj().Rows[0].Selected = true;

                            // Solution to the issue with thin scrollbars:
                            // Set the focus manually and set 'IsInputKey' to true.
                            // This will raise KeyDown event and eventually we will reach _myRenamerApp.on_KeyDown -- which will suppress the Tab key
                            if (_dataGrid.Obj().Parent is myWrappingPanel.customPanel)
                            {
                                _dataGrid.Obj().Focus();
                                e.IsInputKey = true;
                            }

                            _dataGrid.setTabFocus(true);
                        }
                    }

                    if (sender is myDataGridView)
                    {
                        _tree.Obj().Focus();
                        _dataGrid.setTabFocus(false);
                    }

                } break;

            // Toggle every selected DataGrid row
            case Keys.Space: {

                    if (sender is TreeView)
                    {
                        _dataGrid.toggleSelectedCheckboxes();
                    }

                } break;
        }

        return;
    }

    // --------------------------------------------------------------------------------

    // Store current selection, to be able to restore it later on
    private void tree_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (_dataGrid.getTabFocus() == false && _dataGrid.Obj().SelectedRows.Count > 1)
        {
            // Store the selection only when currently clicked row is already selected
            if (_dataGrid.Obj().Rows[e.RowIndex].Selected)
            {
                if (_grid_SelectedRows == null)
                {
                    _grid_SelectedRows = new List<int>();
                }

                for (int i = 0; i < _dataGrid.Obj().Rows.Count; i++)
                    if (_dataGrid.Obj().Rows[i].Selected)
                        _grid_SelectedRows.Add(i);
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    private void tree_grid_onMouseClick(object sender, MouseEventArgs e)
    {
        if (sender is TreeView)
        {
            _dataGrid.setTabFocus(false);
        }

        if (sender is myDataGridView)
        {
            // Restore selection, if needed
            if (_dataGrid.getTabFocus() == false && _dataGrid.Obj().Rows.Count > 1 && _grid_SelectedRows != null)
            {
                for (int i = 0; i < _grid_SelectedRows.Count; i++)
                {
                    _dataGrid.Obj().Rows[_grid_SelectedRows[i]].Selected = true;
                }

                _grid_SelectedRows.Clear();
            }

            _dataGrid.setTabFocus(true);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    private void _form_onFormClosing(object sender, FormClosingEventArgs e)
    {
        finalize();
    }

    // --------------------------------------------------------------------------------

    // Filter string changed
    private void tb_Filter_onTextChanged(object sender, EventArgs e)
    {
        TextBox tb  = (TextBox)(sender);
        var reason  = myDataGrid_Wrapper.PopulateReason.filterChanged;
        var selNode = _tree.Obj().SelectedNode;

        if (tb == _tb_Filter.Obj())
            _filterStr = tb.Text;

        if (tb == _tb_FilterOut.Obj())
            _filterOutStr = tb.Text;

        if (_dataGrid.Obj().RowCount < 10000)
        {
            // Populate dataGrid immediately, as this won't take much time anyway
            _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, selNode, _filterStr, _filterOutStr);
        }
        else
        {
            // Too much rows in the grid.
            // Don't want to repopulate the grid each time the user enters new char in a filter box.
            // Delay the populating task until the user stops typing
            if (_filterDelayCnt == 0)
            {
                tb.BackColor = Color.LightGoldenrodYellow;

                new Task(

                    delegate
                    {
                        _filterDelayCnt = 2;

                        while (_filterDelayCnt > 1)
                        {
                            _filterDelayCnt = 1;

                            // If the user has not typed anything while this delay's in progress,
                            // assume it's time to finally populate the grid
                            Task.Delay(500).Wait();
                        }

                        _form.Invoke(new MethodInvoker(
                            delegate
                            {
                                _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, selNode, _filterStr, _filterOutStr);
                                tb.BackColor = Color.White;
                            })
                        );

                        _filterDelayCnt = 0;

                        return;
                    }

                ).Start();

            }
            else
            {
                // Keep increasing the counter while the user is typing
                _filterDelayCnt++;
            }
        }

        return;
    }

    // --------------------------------------------------------------------------------

    // Update all the widgets' state using the data from [updatedList]
    public void update(List<myTreeListDataItem> updatedList, bool updGlobal, bool updDependent)
    {
        // We're only going to update the global list here;
        // Every dependent widged must implement its own method to update itself from the global list

        // Before updating the global list, we need to make a backup
        // to be able to restore all the files to their original names later
        if (_useBackup)
        {
            if (_backup == null)
            {
                _backup = new myBackup();
            }

            _backup.saveState(_globalFileListExt, updatedList);
        }

/*
        BEFORE RENAME:

            REAL:
                c:\dirA\folderA\fileA.txt

            GLOBAL:
                c:\dirA
                c:\dirA\folderA
                c:\dirA\folderA\fileA.txt

        AFTER RENAME (which should be done in a backwards fashion):
                c:\dirA\folderA\fileA.txt   ==> c:\dirA\folderA\fileB.txt
                c:\dirA\folderA             ==> c:\dirA\folderB
                c:\dirA                     ==> c:\dirB

            REAL:
                c:\dirB\folderB\fileB.txt

            GLOBAL:
                c:\dirA
                c:\dirA\folderA
                c:\dirA\folderA\fileA.txt

            UPDATED:
                c:\dirB
                c:\dirA\folderB
                c:\dirA\folderA\fileB.txt
*/

        // Update Global List
        if (updGlobal)
        {
            // For every changed directory:
            for (int i = 0; i < updatedList.Count; i++)
            {
                int id = updatedList[i].Id;

                if (_globalFileListExt[id].Name != updatedList[i].Name)
                {
                    // In case the recursion is enabled,
                    // we need to update all the global list's items that have been affected by the latest change
                    if (_useRecursion && updatedList[i].isDir)
                    {
                        int j = id + 1;
                        string oldPath = _globalFileListExt[id].Name;
                        string name;

                        while (j < _globalFileListExt.Count && _globalFileListExt[j].Name.Contains(oldPath))
                        {
                            name = updatedList[i].Name + _globalFileListExt[j].Name[oldPath.Length..];

                            _globalFileListExt[j].Name = name;
                            j++;
                        }

                        j = i + 1;

                        while (j < updatedList.Count && updatedList[j].Name.Contains(oldPath))
                        {
                            name = updatedList[i].Name + updatedList[j].Name[oldPath.Length..];
                            updatedList[j].Name = name;
                            j++;
                        }
                    }

                    // Update current 
                    _globalFileListExt[id].Name = updatedList[i].Name;
                }
            }
        }

        // Update dependent widgets
        if (updDependent)
        {
            _dataGrid.update();
            _tree.update(_backup, updatedList);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    public myBackup getBackup()
    {
        if (_backup == null)
        {
            _backup = new myBackup();
        }

        return _backup;
    }

    // --------------------------------------------------------------------------------
};

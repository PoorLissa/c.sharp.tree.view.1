using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
    Front-facing class that wraps myTree and myDataGrid objects.
    Provides all the necessary infrastructure fo them to work together.
    
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
    public DataGridView     dg;
    public CheckBox         cb_ShowFiles;
    public CheckBox         cb_ShowDirs;
    public CheckBox         cb_Recursive;
    public TextBox          tb_Filter;
    public RichTextBox      richTextBox;
}



public class myTree_DataGrid_Manager
{
    // --------------------------------------------------------------------------------

    private Form        _form         = null;
    private myTree      _tree         = null;
    private myDataGrid  _dataGrid     = null;
    private CheckBox    _cb_ShowFiles = null;
    private CheckBox    _cb_ShowDirs  = null;
    private CheckBox    _cb_Recursive = null;
    private TextBox     _tb_Filter    = null;
    private RichTextBox _richTextBox  = null;

    private List<myTreeListDataItem> _globalFileListExt = null;     // Stores all the folders/files found in the last [nodeSelected] call

    private myBackup _backup = null;                                // Keeps the history of all changes made to the file names

    private bool _doShowDirs   = true;
    private bool _doShowFiles  = true;
    private bool _useRecursion = false;
    private bool _useTasks     = true;                              // Defines if async tasks should be used for building file tree. If [true], build process can be cancelled

    private int  _nDirs;                                            // Stores the number of folders found in the last [nodeSelected] call
    private int  _nFiles;                                           // Stores the number of files found in the last [nodeSelected] call

    private string _filterStr = "";                                 // To use with filtering event
    private int    _filterDelayCnt = 0;                             // To use with filtering event (to delay filtering)

    private CancellationTokenSource _tokenSource = null;
    private Task        _tree_onAfterSelect_Task = null;

    // --------------------------------------------------------------------------------

    public myTree_DataGrid_Manager(ref myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        _nDirs  = 0;
        _nFiles = 0;

        _globalFileListExt = new List<myTreeListDataItem>();

        _tree     = new myTree     (mtdgmi.tv, path, expandEmpty);
        _dataGrid = new myDataGrid (mtdgmi.dg, _globalFileListExt);

        _form         = mtdgmi.form;
        _cb_ShowFiles = mtdgmi.cb_ShowFiles;
        _cb_ShowDirs  = mtdgmi.cb_ShowDirs;
        _cb_Recursive = mtdgmi.cb_Recursive;
        _tb_Filter    = mtdgmi.tb_Filter;
        _richTextBox  = mtdgmi.richTextBox;

        _cb_ShowFiles.Checked = true;
        _cb_ShowDirs.Checked  = true;
        _cb_Recursive.Checked = false;

        _tb_Filter.PlaceholderText = "Filter text";

        // Set up events for the components:
        _tree.Obj().AfterSelect  += new TreeViewEventHandler        (tree_onAfterSelect);
        _tree.Obj().BeforeExpand += new TreeViewCancelEventHandler  (tree_onBeforeExpand);
        _tree.Obj().AfterExpand  += new TreeViewEventHandler        (tree_onAfterExpand);

        _cb_ShowDirs .CheckedChanged += new EventHandler(cb_ShowDirs_onCheckedChanged);
        _cb_ShowFiles.CheckedChanged += new EventHandler(cb_ShowFiles_onCheckedChanged);
        _cb_Recursive.CheckedChanged += new EventHandler(cb_Recursive_onCheckedChanged);

        _tb_Filter.TextChanged += new EventHandler(tb_Filter_onTextChanged);
    }

    // --------------------------------------------------------------------------------

    // Public interface method: obtain the list of selected files
    public void getSelectedFiles(List<myTreeListDataItem> filesList)
    {
        _dataGrid.getSelectedFiles(filesList);
    }

    // --------------------------------------------------------------------------------

    // Public interface method: obtain the list of selected files
    public List<myTreeListDataItem> getSelectedFiles()
    {
        var list = new List<myTreeListDataItem>();

        _dataGrid.getSelectedFiles(list);

        return list;
    }

    // --------------------------------------------------------------------------------

    // Selecting a tree node (using mouse or keyboard)
    private void tree_onAfterSelect(object sender, TreeViewEventArgs e)
    {
        _dataGrid.Enable(false);


        // Decide if the current directory in the tree has changed or not
        // (if not, the checked files in the grid will be restored later, when the grid is repopulated)
        myDataGrid.PopulateReason reason = myDataGrid.PopulateReason.viewDirChanged;


        if (_useTasks)
        {
            if (sender != null)
            {
                // sender == _cb_Recursive -- We're changing the state of [cb_Recursive] checkbox
                // sender != _cb_Recursive -- We're actually clicking the node in the tree

                // Hope this local variables will be treated properly by the task...
                TreeNode selectedNode = (sender == _cb_Recursive)
                    ? _tree.Obj().SelectedNode
                    : e.Node;

                reason = (sender == _cb_Recursive)
                    ? myDataGrid.PopulateReason.recursionChanged_After
                    : myDataGrid.PopulateReason.dirChanged;

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
                    catch (AggregateException ex)
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
                    _tree.nodeSelected_Cancellable(selectedNode, _globalFileListExt, ref _nDirs, ref _nFiles, _tokenSource.Token, _useRecursion);


                    _tokenSource.Token.ThrowIfCancellationRequested();

                    if (doShowNextMsg)
                        _dataGrid.Obj().Invoke(new MethodInvoker(delegate { _dataGrid.displayRecursionMsg($"Populating the List:\n Total {_nDirs + _nFiles} items found"); }));

                    _dataGrid.Obj().Invoke(new MethodInvoker(delegate
                    {
                        _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);
                        _dataGrid.Enable(true);
                    }));

                }, _tokenSource.Token);


                // Execute the task
                _tree_onAfterSelect_Task.Start();
            }
            else
            {
                // No task needed, as the list is already populated
                _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);
                _dataGrid.Enable(true);
            }
        }
        else
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            if (sender != null)
            {
                if (sender == _cb_Recursive)
                {
                    // This happens when we're changing the state of [cb_Recursive] checkbox
                    reason = myDataGrid.PopulateReason.recursionChanged_After;
                    _tree.nodeSelected(_tree.Obj().SelectedNode, _globalFileListExt, ref _nDirs, ref _nFiles, _useRecursion);
                }
                else
                {
                    // This happens when we're actually clicking the node in the tree
                    reason = myDataGrid.PopulateReason.dirChanged;

                    _tree.nodeSelected(e.Node, _globalFileListExt, ref _nDirs, ref _nFiles, _useRecursion);

                    // Set Form's header text
                    _form.Text = e.Node.FullPath;
                }
            }

            _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
            _dataGrid.Enable(true);
        }

        return;
    }

    // --------------------------------------------------------------------------------

    // Expanding tree node -- Before
    private void tree_onBeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        _tree.AllowRedrawing(false);
        _tree.nodeExpanded_Before(e.Node);
    }

    // --------------------------------------------------------------------------------

    // Expanding tree node -- After
    private void tree_onAfterExpand(object sender, TreeViewEventArgs e)
    {
        _tree.AllowRedrawing(true);
    }

    // --------------------------------------------------------------------------------

    // Show or hide directories
    private void cb_ShowDirs_onCheckedChanged(object sender, EventArgs e)
    {
        _doShowDirs = _cb_ShowDirs.Checked;
        tree_onAfterSelect(null, null);
    }

    // --------------------------------------------------------------------------------

    // Show or hide files
    private void cb_ShowFiles_onCheckedChanged(object sender, EventArgs e)
    {
        _doShowFiles = _cb_ShowFiles.Checked;
        tree_onAfterSelect(null, null);
    }

    // --------------------------------------------------------------------------------

    // Recursive search enabled/disabled
    private void cb_Recursive_onCheckedChanged(object sender, EventArgs e)
    {
        _useRecursion = _cb_Recursive.Checked;

        _dataGrid.setRecursiveMode(_useRecursion);

        tree_onAfterSelect(sender, null);

        return;
    }

    // --------------------------------------------------------------------------------

    // Filter string changed
    private void tb_Filter_onTextChanged(object sender, EventArgs e)
    {
        _filterStr = (sender as TextBox).Text;

        var reason = myDataGrid.PopulateReason.filterChanged;

        if (_dataGrid.Obj().RowCount < 10000 || _filterStr.Length == 0)
        {
            // Populate dataGrid immediately, as this won't take much time anyway
            _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);
        }
        else
        {
            // Too much rows in the grid.
            // Don't want to repopulate the grid each time the user enters new char in a filter box.
            // Delay the populating task until the user stops typing
            if (_filterDelayCnt == 0)
            {
                _tb_Filter.BackColor = Color.LightGoldenrodYellow;

                var delayedTask = new Task(
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
                                _tb_Filter.BackColor = Color.White;
                                _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);
                            })
                        );

                        _filterDelayCnt = 0;

                        return;
                    }
                );

                delayedTask.Start();
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
    public void update(List<myTreeListDataItem> updatedList)
    {
        // We're only going to update the global list here;
        // Every dependent widged must implement its own method to update itself from the global list

        // Before updating the global list, we need to make a backup
        // to be able to restore all the files to their original names later
        if (_backup == null)
        {
            _backup = new myBackup();
        }


        _backup.saveState(_globalFileListExt, updatedList);


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


        // Update widgets
        _dataGrid.update();
        _tree.update();


        // Check our history:
        if (false)
        {
            string s = _backup.getHistory();

            _richTextBox.Text += " --- history so far ---\n";
            _richTextBox.Text += s + "\n";
            _richTextBox.Text += " ----------------------\n";
        }

        if (false)
        {
            _richTextBox.Text += " --- global list so far ---\n";

            foreach (var item in _globalFileListExt)
                _richTextBox.Text += item.Name + "\n";

            _richTextBox.Text += " ----------------------\n";
        }

        return;
    }
};

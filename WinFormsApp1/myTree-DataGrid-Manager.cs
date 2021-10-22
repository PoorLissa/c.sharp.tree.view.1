using System;
using System.Collections.Generic;
using System.Drawing;
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

    private bool _doShowDirs   = true;
    private bool _doShowFiles  = true;
    private bool _useRecursion = false;

    private int  _nDirs;                                            // Stores the number of folders found in the last [nodeSelected] call
    private int  _nFiles;                                           // Stores the number of files found in the last [nodeSelected] call

    private string _filterStr = "";                                 // To use with filtering event
    private int    _filterDelayCnt = 0;                             // To use with filtering event (to delay filtering)

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
        _tree.Obj().AfterSelect  += new TreeViewEventHandler(tree_onAfterSelect);
        _tree.Obj().BeforeExpand += new TreeViewCancelEventHandler(tree_onBeforeExpand);
        _tree.Obj().AfterExpand  += new TreeViewEventHandler(tree_onAfterExpand);

        _cb_ShowDirs.CheckedChanged  += new EventHandler(cb_ShowDirs_onCheckedChanged);
        _cb_ShowFiles.CheckedChanged += new EventHandler(cb_ShowFiles_onCheckedChanged);
        _cb_Recursive.CheckedChanged += new EventHandler(cb_Recursive_onCheckedChanged);

        _tb_Filter.TextChanged += new EventHandler(tb_Filter_onTextChanged);
    }

    // --------------------------------------------------------------------------------

    public void getSelectedFiles(List<myTreeListDataItem> filesList)
    {
        _dataGrid.getSelectedFiles(filesList);
    }

    // --------------------------------------------------------------------------------

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
        // Decide if the current directory in the tree has changed or not
        // (if not, the selected files will be restored later, when the grid is repopulated)
        myDataGrid.PopulateReason reason = myDataGrid.PopulateReason.viewDirChanged;

        if (sender != null)
        {
            if (sender == _cb_Recursive)
            {
                reason = myDataGrid.PopulateReason.recursionChanged_After;

                // This happens when we're changing the state of [cb_Recursive] checkbox
                _tree.nodeSelected(_tree.Obj().SelectedNode, _globalFileListExt, ref _nDirs, ref _nFiles, _useRecursion);
            }
            else
            {
                reason = myDataGrid.PopulateReason.dirChanged;

                // This happens when we're actually clicking the node in the tree
                _tree.nodeSelected(e.Node, _globalFileListExt, ref _nDirs, ref _nFiles, _useRecursion);

                // Set Form's header text
                _form.Text = e.Node.FullPath;
            }
        }

        _dataGrid.Populate(_nDirs, _nFiles, _doShowDirs, _doShowFiles, reason, _filterStr);

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

    // Update widget's state using the data from [updatedList]
    public void update(List<myTreeListDataItem> updatedList)
    {
        // now we can update global list, and them make every dependent widged be updated from it

        // this class must only be able to update its state.
        // all the restore functionality must be implemented by other parties

        foreach (var item in updatedList)
        {
            int id = item.Id;

            _richTextBox.Text += " check original: " + _globalFileListExt[id].Name + "\n";
            _richTextBox.Text += " check changed : " + item.Name + "\n\n";
        }

        //_dataGrid.update(updatedList);

        return;
    }
};

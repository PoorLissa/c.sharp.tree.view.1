using System;
using System.Collections.Generic;
using System.Windows.Forms;



public struct myTree_DataGrid_Manager_Initializer
{
    public Form             form;
    public TreeView         tv;
    public DataGridView     dg;
    public CheckBox         cb_ShowFiles;
    public CheckBox         cb_ShowDirs;
    public CheckBox         cb_Recursive;
    public TextBox          tb_Filter;
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

    private List<myTreeListDataItem> _globalFileListExt = null;

    private bool _doShowDirs   = true;
    private bool _doShowFiles  = true;
    private bool _useRecursion = false;

    private int  _nodeSelected_Dirs;
    private int  _nodeSelected_Files;

    private string _filterStr = "";

    // --------------------------------------------------------------------------------

    public myTree_DataGrid_Manager(ref myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        _nodeSelected_Dirs  = 0;
        _nodeSelected_Files = 0;

        _globalFileListExt = new List<myTreeListDataItem>();

        _tree     = new myTree     (mtdgmi.tv, path, expandEmpty);
        _dataGrid = new myDataGrid (mtdgmi.dg, _globalFileListExt);

        _form = mtdgmi.form;

        _cb_ShowFiles = mtdgmi.cb_ShowFiles;
        _cb_ShowDirs  = mtdgmi.cb_ShowDirs;
        _cb_Recursive = mtdgmi.cb_Recursive;

        _tb_Filter = mtdgmi.tb_Filter;

        _cb_ShowFiles.Checked = true;
        _cb_ShowDirs.Checked  = true;
        _cb_Recursive.Checked = false;

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

    // Selecting tree node
    private void tree_onAfterSelect(object sender, TreeViewEventArgs e)
    {
        // [nodeSelected_Dirs/nodeSelected_Files] will contain the number of items we need to display

        if (sender == _cb_Recursive)
        {
            // This happens when we're changing the state of [cb_Recursive] checkbox
            _tree.nodeSelected(_tree.Obj().SelectedNode, _globalFileListExt, ref _nodeSelected_Dirs, ref _nodeSelected_Files, _useRecursion);
        }
        else
        {
            if (sender != null)
            {
                // This happens when we're actually clicking the node in the tree
                _tree.nodeSelected(e.Node, _globalFileListExt, ref _nodeSelected_Dirs, ref _nodeSelected_Files, _useRecursion);

                // Set Form's header text
                _form.Text = e.Node.FullPath;
            }
        }

        _dataGrid.Populate(_nodeSelected_Dirs, _nodeSelected_Files, _doShowDirs, _doShowFiles, _filterStr);

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

        _dataGrid.Populate(_nodeSelected_Dirs, _nodeSelected_Files, _doShowDirs, _doShowFiles, _filterStr);
    }

    // --------------------------------------------------------------------------------
};
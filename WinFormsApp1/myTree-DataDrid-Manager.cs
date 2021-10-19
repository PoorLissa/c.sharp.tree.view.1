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
}

public class myTree_DataGrid_Manager
{
    // --------------------------------------------------------------------------------

    private Form        form         = null;
    private myTree      tree         = null;
    private myDataGrid  dataGrid     = null;
    private CheckBox    cb_ShowFiles = null;
    private CheckBox    cb_ShowDirs  = null;
    private CheckBox    cb_Recursive = null;


    private List<myTreeListDataItem> globalFileListExt = null;

    private bool doShowDirs   = false;
    private bool doShowFiles  = false;
    private bool useRecursion = false;

    private int  nodeSelected_Dirs;
    private int  nodeSelected_Files;

    private string filterStr = "";

    // --------------------------------------------------------------------------------

    public myTree_DataGrid_Manager(ref myTree_DataGrid_Manager_Initializer mtdgmi, string path, bool expandEmpty)
    {
        nodeSelected_Dirs  = 0;
        nodeSelected_Files = 0;

        globalFileListExt = new List<myTreeListDataItem>();

        tree     = new myTree    (mtdgmi.tv, path, expandEmpty);
        dataGrid = new myDataGrid(mtdgmi.dg, globalFileListExt);

        form = mtdgmi.form;

        cb_ShowFiles = mtdgmi.cb_ShowFiles;
        cb_ShowDirs  = mtdgmi.cb_ShowDirs;
        cb_Recursive = mtdgmi.cb_Recursive;

        cb_ShowFiles.Checked = true;
        cb_ShowDirs.Checked  = true;
        cb_Recursive.Checked = false;
    }

    // --------------------------------------------------------------------------------

    // Selecting tree node
    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
        // [nodeSelected_Dirs/nodeSelected_Files] will contain the number of items we need to display
/*
        if (sender == cb_Recursive)
        {
            // This happens when we're changing the state of [cb_Recursive] checkbox
            tree.nodeSelected(treeView1.SelectedNode, globalFileListExt, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);
        }
        else
        {
            if (sender != null)
            {
                // This happens when we're actually clicking the node in the tree
                tree.nodeSelected(e.Node, globalFileListExt, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);

                // Set Form's header text
                this.Text = e.Node.FullPath;
            }
        }

        dataGrid.Populate(nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles, filterStr);
*/
    }

    // --------------------------------------------------------------------------------

};
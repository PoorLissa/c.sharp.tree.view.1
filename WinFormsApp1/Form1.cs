﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myTree      tree     = null;
        private myDataGrid  dataGrid = null;
        private List<myTreeListDataItem> globalFileListExt = null;

        private bool doShowDirs   = false;
        private bool doShowFiles  = false;
        private bool useRecursion = false;

        private int  nodeSelected_Dirs;
        private int  nodeSelected_Files;

        private string filterStr = "";

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

/*
            myTree_DataGrid_Manager_Initializer mtdgmi = new myTree_DataGrid_Manager_Initializer();

            mtdgmi.form = this;
            mtdgmi.tv = treeView1;
            mtdgmi.dg = dataGridView1;
            mtdgmi.cb_Recursive = cb_Recursive;
            mtdgmi.cb_ShowDirs  = cb_ShowDirs;
            mtdgmi.cb_ShowFiles = cb_ShowFiles;

            myTree_DataGrid_Manager m = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);

            return;
*/
            globalFileListExt = new List<myTreeListDataItem>();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options";
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options\\__far.user.menu.1.png";
                path = "c:\\_maxx\\002 - music";
                path = "d:\\Games\\Dishonored-2\\Uninstall";
            }

            tree     = new myTree       (this.treeView1, path, expandEmpty);
            dataGrid = new myDataGrid   (this.dataGridView1, globalFileListExt);

            this.cb_ShowFiles.Checked = true;
            this.cb_ShowDirs. Checked = true;
            this.cb_Recursive.Checked = false;

            nodeSelected_Dirs  = 0;
            nodeSelected_Files = 0;

            richTextBox1.Text += path + "\n\n\n";
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new List<myTreeListDataItem>();

            dataGrid.getSelectedFiles(list);

            richTextBox1.Clear();

            foreach (var item in list)
            {
                richTextBox1.Text += item.Name + "\n";
            }
        }

        // --------------------------------------------------------------------------------

        // Selecting tree node
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // [nodeSelected_Dirs/nodeSelected_Files] will contain the number of items we need to display

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
        }

        // --------------------------------------------------------------------------------

        // Expanding tree node
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            tree.AllowRedrawing(false);
            tree.nodeExpanded_Before(e.Node);
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            tree.AllowRedrawing(true);
        }

        // --------------------------------------------------------------------------------

        // Show or hide directories
        private void cb_ShowDirs_CheckedChanged(object sender, EventArgs e)
        {
            doShowDirs = this.cb_ShowDirs.Checked;
            treeView1_AfterSelect(null, null);
        }

        // --------------------------------------------------------------------------------

        // Show or hide files
        private void cb_ShowFiles_CheckedChanged(object sender, EventArgs e)
        {
            doShowFiles = this.cb_ShowFiles.Checked;
            treeView1_AfterSelect(null, null);
        }

        // --------------------------------------------------------------------------------

        private void cb_Recursive_CheckedChanged(object sender, EventArgs e)
        {
            useRecursion = this.cb_Recursive.Checked;

            dataGrid.setRecursiveMode(useRecursion);

            treeView1_AfterSelect(sender, null);
        }

        // --------------------------------------------------------------------------------

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filterStr = (sender as TextBox).Text;

            dataGrid.Populate(nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles, filterStr);
        }

        // --------------------------------------------------------------------------------

    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myTree      tree     = null;
        private myDataGrid  dataGrid = null;
        private System.Collections.Generic.List<string> globalFileList = null;
        private System.Collections.Generic.List<string> globalFldrList = null;

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

            globalFileList = new List<string>();
            globalFldrList = new List<string>();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = "d:\\Games\\Dishonored-2\\Uninstall";
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options";
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options\\__far.user.menu.1.png";
                path = "c:\\_maxx\\music";
            }

            tree = new myTree(this.treeView1, path, expandEmpty);
            dataGrid = new myDataGrid(this.dataGridView1, globalFileList);

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
            var list = new System.Collections.Generic.List<string>();

            dataGrid.getSelectedFiles(list, doShowDirs, doShowFiles);

            richTextBox1.Clear();

            foreach (var item in list)
            {
                richTextBox1.Text += item[2..] + "\n";
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
                tree.nodeSelected(treeView1.SelectedNode, globalFileList, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);
            }
            else
            {
                if (sender != null)
                {
                    // This happens when we're actually clicking the node in the tree
                    tree.nodeSelected(e.Node, globalFileList, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);
                }
            }

            dataGrid.Populate(nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles, filterStr);
        }

        // --------------------------------------------------------------------------------

        // Expanding tree node
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            tree.AllowRedrawing(false);
            tree.nodeExpanded_Before(e.Node, globalFldrList);
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

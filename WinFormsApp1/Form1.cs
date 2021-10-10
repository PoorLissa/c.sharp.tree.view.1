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
        private System.Collections.Generic.List<string> listFiles = null;
        private System.Collections.Generic.List<string> listDirs  = null;

        private bool doShowDirs   = false;
        private bool doShowFiles  = false;
        private bool useRecursion = false;

        private int  nodeSelected_Dirs;
        private int  nodeSelected_Files;

        private string searchStr = "";

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            expandEmpty = true;
            path = "D:\\Games\\Dishonored\\Uninstall";

            tree = new myTree(this.treeView1, path, expandEmpty);
            dataGrid = new myDataGrid(this.dataGridView1);

            listFiles = new List<string>();
            listDirs  = new List<string>();

            this.cb_ShowFiles.Checked = true;
            this.cb_ShowDirs. Checked = true;
            this.cb_Recursive.Checked = false;

            nodeSelected_Dirs  = 0;
            nodeSelected_Files = 0;
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new System.Collections.Generic.List<string>();

            dataGrid.getSelectedFiles(listFiles, list, doShowDirs, doShowFiles);

            richTextBox1.Clear();

            foreach (var item in list)
            {
                richTextBox1.Text += item + "\n";
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
                tree.nodeSelected(treeView1.SelectedNode, listFiles, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);
            }
            else
            {
                if (sender != null)
                {
                    // This happens when we're actually clicking the node in the tree
                    tree.nodeSelected(e.Node, listFiles, ref nodeSelected_Dirs, ref nodeSelected_Files, useRecursion);
                }
            }

            dataGrid.Populate(listFiles, nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles, searchStr);
        }

        // --------------------------------------------------------------------------------

        // Expanding tree node
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            tree.AllowRedrawing(false);
            tree.nodeExpanded_Before(e.Node, listDirs);
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

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            dataGrid.OnKeyDown(sender, e);
        }

        // --------------------------------------------------------------------------------

        private void cb_Recursive_CheckedChanged(object sender, EventArgs e)
        {
            useRecursion = this.cb_Recursive.Checked;
            treeView1_AfterSelect(sender, null);
        }

        // --------------------------------------------------------------------------------

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            searchStr = (sender as TextBox).Text;

            dataGrid.Populate(listFiles, nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles, searchStr);
        }

        // --------------------------------------------------------------------------------
    }
}

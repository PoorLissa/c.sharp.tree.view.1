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

        private bool doShowDirs  = false;
        private bool doShowFiles = false;
        private int  nodeSelected_Dirs;
        private int  nodeSelected_Files;

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            tree = new myTree(this.treeView1, path, expandEmpty);
            dataGrid = new myDataGrid(this.dataGridView1);

            listFiles = new List<string>();
            listDirs  = new List<string>();

            this.cb_ShowFiles.Checked = true;
            this.cb_ShowDirs. Checked = true;

            nodeSelected_Dirs  = 0;
            nodeSelected_Files = 0;
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
        }

        // --------------------------------------------------------------------------------

        // Selecting tree node
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (sender != null)
            {
                // [nodeSelected_Dirs/nodeSelected_Files] will contain the number of items we need to display
                tree.nodeSelected(e.Node, listFiles, ref nodeSelected_Dirs, ref nodeSelected_Files);
            }

            dataGrid.Populate(listFiles, nodeSelected_Dirs, nodeSelected_Files, doShowDirs, doShowFiles);
        }

        // --------------------------------------------------------------------------------

        // Expanding tree node
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //richTextBox1.Text += $" --> before expand({e.Node.Text})\n";
            tree.AllowRedrawing(false);
            tree.nodeExpanded_Before(e.Node, listDirs);
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            //richTextBox1.Text += " --> after expand()\n";
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
    }
}

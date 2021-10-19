using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myTree_DataGrid_Manager manager = null;

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options";
                path = "E:\\_work\\_projects\\Visual Studio\\2021\\c.sharp.tree.view.1\\WinFormsApp1\\_far.options\\__far.user.menu.1.png";
                path = "c:\\_maxx\\002 - music";
                path = "d:\\Games\\Dishonored-2\\Uninstall";
            }

            var mtdgmi = new myTree_DataGrid_Manager_Initializer();

            mtdgmi.form = this;
            mtdgmi.tv = treeView1;
            mtdgmi.dg = dataGridView1;
            mtdgmi.cb_Recursive = cb_Recursive;
            mtdgmi.cb_ShowDirs  = cb_ShowDirs;
            mtdgmi.cb_ShowFiles = cb_ShowFiles;
            mtdgmi.tb_Filter = textBox1;

            manager = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new List<myTreeListDataItem>();

            manager.getSelectedFiles(list);

            richTextBox1.Clear();

            foreach (var item in list)
                  richTextBox1.Text += item.Name + "\n";
        }

        // --------------------------------------------------------------------------------
    }
}

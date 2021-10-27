using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using myControls;


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myTree_DataGrid_Manager myTDGManager = null;

        private myControls.myComboBox myCb = null;
        private myControls.myTextBox  myTb = null;

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = @"E:\_work\_projects\Visual Studio\2021\c.sharp.tree.view.1\WinFormsApp1\_far.options";
                path = @"E:\_work\_projects\Visual Studio\2021\c.sharp.tree.view.1\WinFormsApp1\_far.options\__far.user.menu.1.png";
                path = @"c:\_maxx\002 - music\Techno\Microsoft PFE Remediation for Configuration Man\Microsoft Visual Studio\Shared\Entity Framework Tools\NuGet Packages\EntityFramework.5.0.0";
                path = @"d:\Games\-= Games =-\Uninstall";
                path = @"c:\_maxx\002 - music";
            }

            var mtdgmi = new myTree_DataGrid_Manager_Initializer();

            mtdgmi.form         = this;
            mtdgmi.tv           = treeView1;
            mtdgmi.dg           = dataGridView1;
            mtdgmi.cb_Recursive = cb_Recursive;
            mtdgmi.cb_ShowDirs  = cb_ShowDirs;
            mtdgmi.cb_ShowFiles = cb_ShowFiles;
            mtdgmi.tb_Filter    = textBox1;
            mtdgmi.richTextBox  = richTextBox1;

            myTDGManager = new myTree_DataGrid_Manager(ref mtdgmi, path, expandEmpty);

            myCb = new myControls.myComboBox(this.comboBox1, "Select option");
            myTb = new myControls.myTextBox (this.textBox2, "Filter text");
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            var list = myTDGManager.getSelectedFiles();

            var list_copy = myTreeListDataItem.copyList(list);

            for (int i = 0; i < list.Count; i++)
            {
                list_copy[i].Name += "_z";
            }

            richTextBox1.Clear();

#if false
            for (int i = 0; i < list.Count; i++)
            {
                richTextBox1.Text += list[i].Name + "\n";
                richTextBox1.Text += list_copy[i].Name + "\n";
            }
#endif

            myTDGManager.update(list_copy);
        }

        // --------------------------------------------------------------------------------
    }
}

using System;
using System.Windows.Forms;



namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myRenamerApp app = null;

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
                path = @"d:\test\-= Games =-\Uninstall";
                path = @"c:\_maxx\test\asdasdad";
            }

            init(path, expandEmpty);
        }

        // --------------------------------------------------------------------------------

        private void init(string path, bool expandEmpty)
        {
            // myTree_DataGrid's controls:
            var mtdgmi = new myTree_DataGrid_Manager_Initializer();
                mtdgmi.form         = this;
                mtdgmi.tv           = treeView1;
                mtdgmi.dg           = dataGridView1;
                mtdgmi.cb_Recursive = cb_Recursive;
                mtdgmi.cb_ShowDirs  = cb_ShowDirs;
                mtdgmi.cb_ShowFiles = cb_ShowFiles;
                mtdgmi.tb_Filter    = textBox1;
                mtdgmi.tb_FilterOut = textBox2;
                mtdgmi.richTextBox  = richTextBox1;

            // Every control that myRenamerApp must be aware of:
            var mraControls = new myRenamerApp_Controls();
                mraControls.option_001_ch_01 = this.checkBox1;
                mraControls.option_001_ch_02 = this.checkBox2;
                mraControls.option_001_ch_03 = this.checkBox3;
                mraControls.option_001_cb_01 = new myControls.myComboBox(this.comboBox2, "Delimiter");

                mraControls.option_002_ch_01 = this.checkBox5;
                mraControls.option_002_ch_02 = this.checkBox4;
                mraControls.option_002_num_1 = this.numericUpDown1;
                mraControls.option_002_num_2 = this.numericUpDown2;

                mraControls.option_003_ch_01 = this.checkBox7;
                mraControls.option_003_num_1 = this.numericUpDown3;
                mraControls.option_003_rb_01 = this.radioButton1;
                mraControls.option_003_rb_02 = this.radioButton2;
                mraControls.option_003_tb_01 = this.textBox4;

            // myRenamerApp
            app = new myRenamerApp(mraControls, mtdgmi, path, expandEmpty);

            // Test controls
            myCb = new myControls.myComboBox(this.comboBox1, "Select option");
            myTb = new myControls.myTextBox(this.textBox3, "Filter text");
        }

        // --------------------------------------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
        }

        // --------------------------------------------------------------------------------

        private void btn_Rename_Click(object sender, EventArgs e)
        {
            app.Rename();
        }

        // --------------------------------------------------------------------------------

        private void btn_Undo_Click(object sender, EventArgs e)
        {
            app.Undo_Rename();
        }

        // --------------------------------------------------------------------------------
    }
}

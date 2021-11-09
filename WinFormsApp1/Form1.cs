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
            {
                mraControls.richTextBox      = this.richTextBox1;

                mraControls.option_001_ch_01 = this.checkBox_Option_001;
                mraControls.option_001_ch_02 = this.checkBox2;
                mraControls.option_001_ch_03 = this.checkBox3;
                mraControls.option_001_cb_01 = new myControls.myComboBox(this.comboBox2, "Delimiter");

                mraControls.option_002_ch_01 = this.checkBox_Option_002;
                mraControls.option_002_ch_02 = this.checkBox4;
                mraControls.option_002_num_1 = this.numericUpDown1;
                mraControls.option_002_num_2 = this.numericUpDown2;

                mraControls.option_003_ch_01 = this.checkBox_Option_003;
                mraControls.option_003_num_1 = this.numericUpDown3;
                mraControls.option_003_rb_01 = this.radioButton1;
                mraControls.option_003_rb_02 = this.radioButton2;
                mraControls.option_003_tb_01 = this.textBox4;

                mraControls.option_004_ch_01 = this.checkBox_Option_004;
                mraControls.option_004_tb_01 = this.textBox6;
                mraControls.option_004_tb_02 = this.textBox7;
                mraControls.option_004_num_1 = this.numericUpDown5;

                mraControls.option_005_ch_01 = this.checkBox_Option_005;
                mraControls.option_005_tb_01 = this.textBox5;
                mraControls.option_005_num_1 = this.numericUpDown4;

                mraControls.option_006_ch_01 = this.checkBox_Option_006;
                mraControls.option_006_ch_02 = this.checkBox1;
                mraControls.option_006_ch_03 = this.checkBox5;
                mraControls.option_006_ch_04 = this.checkBox6;
                mraControls.option_006_rb_01 = this.radioButton3;
                mraControls.option_006_rb_02 = this.radioButton4;
                mraControls.option_006_rb_03 = this.radioButton5;
                mraControls.option_006_rb_04 = this.radioButton6;
                mraControls.option_006_rb_05 = this.radioButton7;

                // unused yet
                mraControls.option_007_ch_01 = this.checkBox_Option_007;
                mraControls.option_008_ch_01 = this.checkBox_Option_008;

                // Each new option panel we add must have main checkbox called "checkBox_Option_xxx"
            }

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

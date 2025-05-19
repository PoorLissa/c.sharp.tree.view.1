using myControls;
using System;
using System.Windows.Forms;



namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myRenamerApp app = null;
        private mySplitButton sb_Undo = null;
        private System.Drawing.Font _btn_rename_font = null;

        //private myControls.myComboBox myCb = null;
        //private myControls.myTextBox myTb = null;
        //private Panel p = new Panel();

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = @"E:\_work\_projects\Visual Studio\2021\c.sharp.tree.view.1\WinFormsApp1\_far.options";
                path = @"d:\Games\-= Games =-\Uninstall";
                path = @"d:\test\-= Games =-\Uninstall";
                path = @"c:\_maxx\test\aaa";
                path = @"C:\Windows\System32\";
                path = @"c:\_maxx\002 - music";
            }

            init(path, expandEmpty);

            /*
                        p.Width = 666;
                        p.Height = 333;

                        p.Left = 300;
                        p.Top = 300;

                        p.Visible = false;

                        Controls.Add(p);
                        p.BringToFront();

                        this.btn_Rename.MouseHover += new EventHandler((object sender, EventArgs e) => 
                        {
                            p.Visible = true;
                        });
            */

            //myCb = new myControls.myComboBox(this.comboBox1, "Select option");
            //myTb = new myControls.myTextBox(this.textBox3, "Filter text");

            this.btn_Rename.MouseEnter += new EventHandler(btn_Rename_onMouseEnter);
            this.btn_Rename.MouseLeave += new EventHandler(btn_Rename_onMouseLeave);
        }

        // --------------------------------------------------------------------------------

        private void init(string path, bool expandEmpty)
        {
            // myTree_DataGrid's controls:
            var mtdgmi = new myTree_DataGrid_Manager_Initializer();
            mtdgmi.form = this;
            mtdgmi.tv = treeView1;
            mtdgmi.dg = myDataGridView1;
            mtdgmi.cb_Recursive = cb_Recursive;
            mtdgmi.cb_ShowDirs = cb_ShowDirs;
            mtdgmi.cb_ShowFiles = cb_ShowFiles;
            mtdgmi.cb_FilterPath = cb_FilterPath;
            mtdgmi.tb_Filter = new myTextBox(textBox1);
            mtdgmi.tb_FilterOut = new myTextBox(textBox2);
            mtdgmi.richTextBox = richTextBox1;

            // Every control that myRenamerApp must be aware of:
            var mraControls = new myRenamerApp_Controls();
            {
                mraControls.mainForm = this;

                mraControls.richTextBox = this.richTextBox1;

                mraControls.option_001_ch_01 = this.checkBox_Option_001;
                mraControls.option_001_ch_02 = this.checkBox2;
                mraControls.option_001_ch_03 = this.checkBox3;
                mraControls.option_001_cb_01 = new myComboBox(this.comboBox2, SortMode.LastOnTop, true, "Delimiter (use : to add multiple delimiters)");
                mraControls.option_001_rb_01 = this.radioButton21;
                mraControls.option_001_rb_02 = this.radioButton18;

                mraControls.option_002_ch_01 = this.checkBox_Option_002;
                mraControls.option_002_ch_02 = this.checkBox4;
                mraControls.option_002_num_1 = this.numericUpDown1;
                mraControls.option_002_num_2 = this.numericUpDown2;

                mraControls.option_003_ch_01 = this.checkBox_Option_003;
                mraControls.option_003_ch_02 = this.checkBox11;
                mraControls.option_003_num_1 = this.numericUpDown3;
                mraControls.option_003_num_2 = this.numericUpDown15;
                mraControls.option_003_rb_01 = this.radioButton1;
                mraControls.option_003_rb_02 = this.radioButton2;
                mraControls.option_003_tb_01 = this.textBox4;

                mraControls.option_004_ch_01 = this.checkBox_Option_004;
                mraControls.option_004_ch_02 = this.checkBox7;
                mraControls.option_004_tb_01 = this.textBox6;
                mraControls.option_004_tb_02 = this.textBox7;
                mraControls.option_004_num_1 = this.numericUpDown5;

                mraControls.option_005_ch_01 = this.checkBox_Option_005;
                mraControls.option_005_cb_01 = new myComboBox(this.comboBox4, SortMode.LastOnTop, false, "Template: ### - *");
                mraControls.option_005_tb_01 = this.textBox12;
                mraControls.option_005_num_1 = this.numericUpDown4;
                mraControls.option_005_num_2 = this.numericUpDown16;
                mraControls.option_005_cb_02 = this.opt_005_predefined_templates;
                mraControls.option_005_btn_1 = this.button2;

                mraControls.option_006_ch_01 = this.checkBox_Option_006;
                mraControls.option_006_ch_02 = this.checkBox1;
                mraControls.option_006_ch_03 = this.checkBox5;
                mraControls.option_006_ch_04 = this.checkBox6;
                mraControls.option_006_rb_01 = this.radioButton3;
                mraControls.option_006_rb_02 = this.radioButton4;
                mraControls.option_006_rb_03 = this.radioButton5;
                mraControls.option_006_rb_04 = this.radioButton6;
                mraControls.option_006_rb_05 = this.radioButton7;
                mraControls.option_006_cb_01 = new myComboBox(this.comboBox3, SortMode.Sorted, false, "Short Words");

                mraControls.option_007_ch_01 = this.checkBox_Option_007;
                mraControls.option_007_num_1 = this.numericUpDown6;
                mraControls.option_007_num_2 = this.numericUpDown7;

                mraControls.option_008_ch_01 = this.checkBox_Option_008;
                mraControls.option_008_num_1 = this.numericUpDown8;
                mraControls.option_008_num_2 = this.numericUpDown11;

                mraControls.option_009_ch_01 = this.checkBox_Option_009;
                mraControls.option_009_cb_01 = this.comboBox5;
                mraControls.option_009_ch_02 = this.checkBox8;
                mraControls.option_009_rb_01 = this.radioButton8;
                mraControls.option_009_rb_02 = this.radioButton9;
                mraControls.option_009_rb_03 = this.radioButton10;
                mraControls.option_009_rb_04 = this.radioButton11;
                mraControls.option_009_rb_05 = this.radioButton13;
                mraControls.option_009_rb_06 = this.radioButton12;
                mraControls.option_009_rb_07 = this.radioButton14;
                mraControls.option_009_tb_01 = this.textBox8;

                mraControls.option_010_ch_01 = this.checkBox_Option_010;
                mraControls.option_010_tb_01 = this.textBox9;
                mraControls.option_010_rb_01 = this.radioButton23;
                mraControls.option_010_rb_02 = this.radioButton22;
                mraControls.option_010_num_1 = this.numericUpDown10;
                mraControls.option_010_num_2 = this.numericUpDown12;
                mraControls.option_010_num_3 = this.numericUpDown13;
                mraControls.option_010_num_4 = this.numericUpDown14;
                mraControls.option_010_ch_02 = this.checkBox10;

                mraControls.option_011_ch_01 = this.checkBox_Option_011;
                mraControls.option_011_rb_01 = this.radioButton17;
                mraControls.option_011_rb_02 = this.radioButton16;
                mraControls.option_011_rb_03 = this.radioButton15;
                mraControls.option_011_rb_04 = this.radioButton24;
                mraControls.option_011_tb_01 = this.textBox10;

                mraControls.option_012_ch_01 = this.checkBox_Option_012;

                mraControls.option_013_ch_01 = this.checkBox_Option_013;
                mraControls.option_013_tb_01 = this.textBox5;
                mraControls.option_013_tb_02 = this.textBox11;
                mraControls.option_013_rb_01 = this.radioButton20;
                mraControls.option_013_rb_02 = this.radioButton19;
                mraControls.option_013_ch_02 = this.checkBox9;
                mraControls.option_013_num_1 = this.numericUpDown9;

                mraControls.option_014_ch_01 = this.checkBox_Option_014;
                mraControls.option_014_tb_01 = this.textBox13;
                mraControls.option_014_tb_02 = this.textBox14;
                mraControls.option_014_ch_02 = this.checkBox12;

                mraControls.option_015_ch_01 = this.checkBox_Option_015;
                mraControls.option_015_gb_01 = this.groupBox3;
                mraControls.option_015_gb_02 = this.groupBox4;
                mraControls.option_015_tb_01 = this.textBox15;

                // Each new option panel we add must have main checkbox called "checkBox_Option_xxx"
            }

             // myRenamerApp
            app = new myRenamerApp(mraControls, mtdgmi, path, expandEmpty);

            // Adjust scroll bars of the widgets:
            if (true)
            {
                myDataGridView1.BorderStyle = BorderStyle.None;
                treeView1.BorderStyle = BorderStyle.None;
                myControls.myWrappingPanel.Wrap(this.myDataGridView1, true, false);
                myControls.myWrappingPanel.Wrap(this.treeView1, true, true);
            }

            // Test controls
            //myCb = new myControls.myComboBox(this.comboBox1, "Select option");
            //myTb = new myControls.myTextBox(this.textBox3, "Filter text");

            sb_Undo = new mySplitButton(this.btn_Undo);

            sb_Undo.addAction(undo1, "Undo");
            sb_Undo.addAction(undo2, "Undo using History file");

            _btn_rename_font = btn_Rename.Font;
        }

        // --------------------------------------------------------------------------------

        private void btn_Rename_Click(object sender, EventArgs e)
        {
            app.Rename();
        }

        // --------------------------------------------------------------------------------

        private void undo1(object sender, EventArgs e)
        {
            app.Undo(useHistoryFile: false);
        }

        // --------------------------------------------------------------------------------

        private void undo2(object sender, EventArgs e)
        {
            app.Undo(useHistoryFile: true);
        }

        // --------------------------------------------------------------------------------

        private void tabPage1_MouseEnter(object sender, EventArgs e)
        {
            this.panel_base.Focus();
        }

        // --------------------------------------------------------------------------------

        private void btn_Rename_onMouseEnter(object sender, EventArgs e)
        {
            btn_Rename.ImageIndex = 1;

            btn_Rename.Font = new System.Drawing.Font(_btn_rename_font.Name, 11, _btn_rename_font.Style, _btn_rename_font.Unit, _btn_rename_font.GdiCharSet);

            new System.Threading.Tasks.Task(() =>
            {
                // Wait about 3 sec
                for (int i = 0; i < 20; i++)
                {
                    if (btn_Rename.ImageIndex != 1)
                        return;

                    System.Threading.Tasks.Task.Delay(150).Wait();
                }

                int t = 333;
                bool dir = false;
                var sb = new System.Text.StringBuilder();

                while (btn_Rename.ImageIndex == 1)
                {
                    btn_Rename.Invoke(new MethodInvoker(delegate
                    {
                        string s = btn_Rename.Text;

                        for (int i = 0; i < s.Length; i++)
                        {
                            if (myUtils.charIsCapitalLetter(s[i]))
                            {
                                if (i == 0 || i == s.Length - 1)
                                {
                                    dir = !dir;
                                    System.Threading.Tasks.Task.Delay(i == 0 ? t : 333).Wait();
                                    t += 333;
                                }

                                int j = dir ? i + 1 : i - 1;

                                char ch = s[j];
                                myUtils.charToUpperCase(ref ch);

                                sb.Clear();
                                sb.Append(s.ToLower());
                                sb[j] = ch;
                                btn_Rename.Text = sb.ToString();
                                break;
                            }
                        }


                    }));

                    System.Threading.Tasks.Task.Delay(100).Wait();
                }

            }).Start();

            return;
        }

        // --------------------------------------------------------------------------------

        private void btn_Rename_onMouseLeave(object sender, EventArgs e)
        {
            btn_Rename.ImageIndex = -1;
            btn_Rename.Font = _btn_rename_font;
            btn_Rename.Text = "Rename";
        }

        // --------------------------------------------------------------------------------

        private void Form1_Shown(object sender, EventArgs e)
        {
            var tickDiff = System.DateTime.Now.Ticks - Program.ticks;

            System.TimeSpan elapsedSpan = new System.TimeSpan(tickDiff);

            this.Text += $";   startupTime = {elapsedSpan.Milliseconds} ms";
        }

        // --------------------------------------------------------------------------------
    };

};

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

public class AAA
{
    public AAA()
    {
    }

    public void doSomeWork(RichTextBox tb, System.Threading.CancellationTokenSource src)
    {
        int total = 0;
        string res = "-done-";

        for (int i = 0; i < 500000000; i++)
        {
            if (src.Token.IsCancellationRequested)
            {
                res = "-cancelled-";
                break;
            }

            double z = Math.Sqrt((double)(i * i * i * i * i));
            double zz = z * z * z * z * z / 3.14;
            double zzz = (zz + zz) * zz;
            double zzzz = Math.Sqrt(zzz);

            total += (int)(10 * zzzz);
        }

        tb.Invoke(new MethodInvoker(delegate { tb.AppendText(res); }));

        return;
    }
};


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --------------------------------------------------------------------------------

        private myTree_DataGrid_Manager myTDGManager = null;

        private myComboBox myCb = null;

        private AAA _aaa = new AAA();

        // --------------------------------------------------------------------------------

        public Form1(string path, bool expandEmpty)
        {
            InitializeComponent();

            if (path.Length == 0)
            {
                expandEmpty = true;
                path = @"E:\_work\_projects\Visual Studio\2021\c.sharp.tree.view.1\WinFormsApp1\_far.options";
                path = @"E:\_work\_projects\Visual Studio\2021\c.sharp.tree.view.1\WinFormsApp1\_far.options\__far.user.menu.1.png";
                path = @"c:\_maxx\002 - music";
                path = @"c:\_maxx\002 - music\Techno\Microsoft PFE Remediation for Configuration Man\Microsoft Visual Studio\Shared\Entity Framework Tools\NuGet Packages\EntityFramework.5.0.0";
                path = @"d:\Games\-= Games =-\Uninstall";
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

            myCb = new myComboBox(this.comboBox1, "Select option");
        }

        // --------------------------------------------------------------------------------

        System.Threading.CancellationTokenSource source = null;
        Task t = null;

        private void button1_Click(object sender, EventArgs e)
        {
            if (t == null || t.IsCompleted)
            {
                source = new System.Threading.CancellationTokenSource();

                t = new System.Threading.Tasks.Task(() =>
                {
                    _aaa.doSomeWork(richTextBox1, source);

                }, source.Token);

                t.Start();
            }
            else
            {
                source.Cancel();
            }

            return;

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


namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.cb_ShowDirs = new System.Windows.Forms.CheckBox();
            this.cb_ShowFiles = new System.Windows.Forms.CheckBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.cb_Recursive = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeView1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.treeView1.ItemHeight = 33;
            this.treeView1.Location = new System.Drawing.Point(12, 12);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(633, 1061);
            this.treeView1.TabIndex = 0;
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1431, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(274, 163);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(1420, 308);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(611, 764);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // cb_ShowDirs
            // 
            this.cb_ShowDirs.AutoSize = true;
            this.cb_ShowDirs.Location = new System.Drawing.Point(1431, 191);
            this.cb_ShowDirs.Name = "cb_ShowDirs";
            this.cb_ShowDirs.Size = new System.Drawing.Size(171, 29);
            this.cb_ShowDirs.TabIndex = 3;
            this.cb_ShowDirs.Text = "Show Directories";
            this.cb_ShowDirs.UseVisualStyleBackColor = true;
            this.cb_ShowDirs.CheckedChanged += new System.EventHandler(this.cb_ShowDirs_CheckedChanged);
            // 
            // cb_ShowFiles
            // 
            this.cb_ShowFiles.AutoSize = true;
            this.cb_ShowFiles.Location = new System.Drawing.Point(1431, 226);
            this.cb_ShowFiles.Name = "cb_ShowFiles";
            this.cb_ShowFiles.Size = new System.Drawing.Size(121, 29);
            this.cb_ShowFiles.TabIndex = 5;
            this.cb_ShowFiles.Text = "Show Files";
            this.cb_ShowFiles.UseVisualStyleBackColor = true;
            this.cb_ShowFiles.CheckedChanged += new System.EventHandler(this.cb_ShowFiles_CheckedChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(651, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.Size = new System.Drawing.Size(763, 1060);
            this.dataGridView1.TabIndex = 6;
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
            // 
            // cb_Recursive
            // 
            this.cb_Recursive.AutoSize = true;
            this.cb_Recursive.Location = new System.Drawing.Point(1431, 261);
            this.cb_Recursive.Name = "cb_Recursive";
            this.cb_Recursive.Size = new System.Drawing.Size(111, 29);
            this.cb_Recursive.TabIndex = 7;
            this.cb_Recursive.Text = "Recursive";
            this.cb_Recursive.UseVisualStyleBackColor = true;
            this.cb_Recursive.CheckedChanged += new System.EventHandler(this.cb_Recursive_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1634, 191);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(201, 31);
            this.textBox1.TabIndex = 8;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2043, 1084);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cb_Recursive);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.cb_ShowFiles);
            this.Controls.Add(this.cb_ShowDirs);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.treeView1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckBox cb_ShowDirs;
        private System.Windows.Forms.CheckBox cb_ShowFiles;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox cb_Recursive;
        private System.Windows.Forms.TextBox textBox1;
    }
}


﻿
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeView1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.treeView1.ItemHeight = 33;
            this.treeView1.Location = new System.Drawing.Point(11, 12);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(633, 1061);
            this.treeView1.TabIndex = 0;
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
            this.cb_ShowDirs.Location = new System.Drawing.Point(1431, 192);
            this.cb_ShowDirs.Name = "cb_ShowDirs";
            this.cb_ShowDirs.Size = new System.Drawing.Size(171, 29);
            this.cb_ShowDirs.TabIndex = 3;
            this.cb_ShowDirs.Text = "Show Directories";
            this.cb_ShowDirs.UseVisualStyleBackColor = true;
            // 
            // cb_ShowFiles
            // 
            this.cb_ShowFiles.AutoSize = true;
            this.cb_ShowFiles.Location = new System.Drawing.Point(1431, 227);
            this.cb_ShowFiles.Name = "cb_ShowFiles";
            this.cb_ShowFiles.Size = new System.Drawing.Size(121, 29);
            this.cb_ShowFiles.TabIndex = 5;
            this.cb_ShowFiles.Text = "Show Files";
            this.cb_ShowFiles.UseVisualStyleBackColor = true;
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
            // 
            // cb_Recursive
            // 
            this.cb_Recursive.AutoSize = true;
            this.cb_Recursive.Location = new System.Drawing.Point(1431, 262);
            this.cb_Recursive.Name = "cb_Recursive";
            this.cb_Recursive.Size = new System.Drawing.Size(111, 29);
            this.cb_Recursive.TabIndex = 7;
            this.cb_Recursive.Text = "Recursive";
            this.cb_Recursive.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1630, 188);
            this.textBox1.Name = "textBox1";
            this.textBox1.PlaceholderText = "filter";
            this.textBox1.Size = new System.Drawing.Size(201, 31);
            this.textBox1.TabIndex = 8;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "111",
            "222",
            "333",
            "444",
            "555"});
            this.comboBox1.Location = new System.Drawing.Point(1751, 20);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(212, 33);
            this.comboBox1.TabIndex = 9;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(1751, 61);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(212, 31);
            this.textBox2.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2043, 1083);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cb_Recursive);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.cb_ShowFiles);
            this.Controls.Add(this.cb_ShowDirs);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.treeView1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
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
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox2;
    }
}


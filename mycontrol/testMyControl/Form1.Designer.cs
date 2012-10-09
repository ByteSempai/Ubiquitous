namespace testMyControl
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Test");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("General", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Test");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Advanced", new System.Windows.Forms.TreeNode[] {
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Test");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Other", new System.Windows.Forms.TreeNode[] {
            treeNode5});
            this.settingsTree1 = new mycontrol.SettingsTree();
            this.settingsPage1 = new mycontrol.SettingsPage();
            this.settingsPage2 = new mycontrol.SettingsPage();
            this.settingsPage3 = new mycontrol.SettingsPage();
            this.settingsPage4 = new mycontrol.SettingsPage();
            this.settingsPage5 = new mycontrol.SettingsPage();
            this.settingsPage6 = new mycontrol.SettingsPage();
            this.button1 = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.settingsTree1.SplitContainer.Panel2.SuspendLayout();
            this.settingsPage1.SuspendLayout();
            this.settingsPage2.SuspendLayout();
            this.settingsPage3.SuspendLayout();
            this.settingsPage4.SuspendLayout();
            this.settingsPage5.SuspendLayout();
            this.settingsPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsTree1
            // 
            treeNode1.Name = "";
            treeNode1.Text = "Test";
            treeNode2.Name = "";
            treeNode2.Text = "General";
            this.settingsTree1.CurrentNode = treeNode2;
            this.settingsTree1.CurrentPage = this.settingsPage1;
            this.settingsTree1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.settingsTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTree1.LabelEdit = false;
            this.settingsTree1.Location = new System.Drawing.Point(0, 0);
            this.settingsTree1.Name = "settingsTree1";
            this.settingsTree1.Size = new System.Drawing.Size(519, 445);
            // 
            // 
            // 
            this.settingsTree1.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTree1.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.settingsTree1.SplitContainer.Name = "splitContainer";
            // 
            // 
            // 
            this.settingsTree1.SplitContainer.Panel2.AllowDrop = true;
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage1);
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage2);
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage3);
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage4);
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage5);
            this.settingsTree1.SplitContainer.Panel2.Controls.Add(this.settingsPage6);
            this.settingsTree1.SplitContainer.Size = new System.Drawing.Size(519, 445);
            this.settingsTree1.SplitContainer.SplitterDistance = 98;
            this.settingsTree1.SplitContainer.TabIndex = 5;
            this.settingsTree1.TabIndex = 0;
            // 
            // 
            // 
            this.settingsTree1.TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTree1.TreeView.HideSelection = false;
            this.settingsTree1.TreeView.Location = new System.Drawing.Point(0, 0);
            this.settingsTree1.TreeView.Name = "treeSettings";
            this.settingsTree1.TreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode4,
            treeNode6});
            this.settingsTree1.TreeView.Size = new System.Drawing.Size(98, 445);
            this.settingsTree1.TreeView.TabIndex = 0;
            this.settingsTree1.Load += new System.EventHandler(this.settingsTree1_Load);
            // 
            // settingsPage1
            // 
            this.settingsPage1.Controls.Add(this.button1);
            this.settingsPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage1.isActive = true;
            this.settingsPage1.Location = new System.Drawing.Point(0, 0);
            this.settingsPage1.Name = "settingsPage1";
            this.settingsPage1.ParentNode = treeNode2;
            this.settingsPage1.Size = new System.Drawing.Size(417, 445);
            this.settingsPage1.TabIndex = 0;
            // 
            // settingsPage2
            // 
            this.settingsPage2.Controls.Add(this.checkedListBox2);
            this.settingsPage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage2.isActive = true;
            this.settingsPage2.Location = new System.Drawing.Point(0, 0);
            this.settingsPage2.Name = "settingsPage2";
            treeNode3.Name = "";
            treeNode3.Text = "Test";
            treeNode4.Name = "";
            treeNode4.Text = "Advanced";
            this.settingsPage2.ParentNode = treeNode4;
            this.settingsPage2.Size = new System.Drawing.Size(417, 445);
            this.settingsPage2.TabIndex = 1;
            // 
            // settingsPage3
            // 
            this.settingsPage3.Controls.Add(this.listView1);
            this.settingsPage3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage3.isActive = true;
            this.settingsPage3.Location = new System.Drawing.Point(0, 0);
            this.settingsPage3.Name = "settingsPage3";
            treeNode5.Name = "";
            treeNode5.Text = "Test";
            treeNode6.Name = "";
            treeNode6.Text = "Other";
            this.settingsPage3.ParentNode = treeNode6;
            this.settingsPage3.Size = new System.Drawing.Size(417, 445);
            this.settingsPage3.TabIndex = 2;
            // 
            // settingsPage4
            // 
            this.settingsPage4.Controls.Add(this.checkedListBox1);
            this.settingsPage4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage4.isActive = true;
            this.settingsPage4.Location = new System.Drawing.Point(0, 0);
            this.settingsPage4.Name = "settingsPage4";
            this.settingsPage4.ParentNode = treeNode1;
            this.settingsPage4.Size = new System.Drawing.Size(417, 445);
            this.settingsPage4.TabIndex = 3;
            // 
            // settingsPage5
            // 
            this.settingsPage5.Controls.Add(this.label3);
            this.settingsPage5.Controls.Add(this.label2);
            this.settingsPage5.Controls.Add(this.label1);
            this.settingsPage5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage5.isActive = true;
            this.settingsPage5.Location = new System.Drawing.Point(0, 0);
            this.settingsPage5.Name = "settingsPage5";
            this.settingsPage5.ParentNode = treeNode3;
            this.settingsPage5.Size = new System.Drawing.Size(417, 445);
            this.settingsPage5.TabIndex = 4;
            // 
            // settingsPage6
            // 
            this.settingsPage6.Controls.Add(this.maskedTextBox1);
            this.settingsPage6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPage6.isActive = true;
            this.settingsPage6.Location = new System.Drawing.Point(0, 0);
            this.settingsPage6.Name = "settingsPage6";
            this.settingsPage6.ParentNode = treeNode5;
            this.settingsPage6.Size = new System.Drawing.Size(417, 445);
            this.settingsPage6.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(184, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(174, 71);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(120, 94);
            this.checkedListBox1.TabIndex = 0;
            // 
            // checkedListBox2
            // 
            this.checkedListBox2.FormattingEnabled = true;
            this.checkedListBox2.Location = new System.Drawing.Point(129, 54);
            this.checkedListBox2.Name = "checkedListBox2";
            this.checkedListBox2.Size = new System.Drawing.Size(120, 94);
            this.checkedListBox2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(165, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(165, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "label1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(165, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "label1";
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(160, 102);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(121, 97);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(150, 117);
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(100, 20);
            this.maskedTextBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 445);
            this.Controls.Add(this.settingsTree1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.settingsTree1.SplitContainer.Panel2.ResumeLayout(false);
            this.settingsPage1.ResumeLayout(false);
            this.settingsPage2.ResumeLayout(false);
            this.settingsPage3.ResumeLayout(false);
            this.settingsPage4.ResumeLayout(false);
            this.settingsPage5.ResumeLayout(false);
            this.settingsPage5.PerformLayout();
            this.settingsPage6.ResumeLayout(false);
            this.settingsPage6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private mycontrol.SettingsTree settingsTree1;
        private mycontrol.SettingsPage settingsPage1;
        private mycontrol.SettingsPage settingsPage2;
        private mycontrol.SettingsPage settingsPage3;
        private mycontrol.SettingsPage settingsPage4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckedListBox checkedListBox2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private mycontrol.SettingsPage settingsPage5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private mycontrol.SettingsPage settingsPage6;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;

















    }
}


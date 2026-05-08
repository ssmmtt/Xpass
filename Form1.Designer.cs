namespace Xpass
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            masterPasswdTextBox = new TextBox();
            showPasswdCheckBox = new CheckBox();
            selectFilesButton = new Button();
            selectDirButton = new Button();
            decryptButton = new Button();
            pathRichTextBox = new RichTextBox();
            notifyIcon1 = new NotifyIcon(components);
            openFileDialog1 = new OpenFileDialog();
            folderBrowserDialog1 = new FolderBrowserDialog();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            button1 = new Button();
            searchTextBox = new TextBox();
            toolTip1 = new ToolTip(components);
            githubLinkPictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)githubLinkPictureBox).BeginInit();
            SuspendLayout();
            // 
            // masterPasswdTextBox
            // 
            masterPasswdTextBox.Location = new Point(323, 122);
            masterPasswdTextBox.Name = "masterPasswdTextBox";
            masterPasswdTextBox.PasswordChar = '*';
            masterPasswdTextBox.PlaceholderText = "如果设置了主密码请输入";
            masterPasswdTextBox.Size = new Size(164, 23);
            masterPasswdTextBox.TabIndex = 5;
            // 
            // showPasswdCheckBox
            // 
            showPasswdCheckBox.AutoSize = true;
            showPasswdCheckBox.ForeColor = SystemColors.ControlDarkDark;
            showPasswdCheckBox.Location = new Point(493, 123);
            showPasswdCheckBox.Name = "showPasswdCheckBox";
            showPasswdCheckBox.Size = new Size(51, 21);
            showPasswdCheckBox.TabIndex = 2;
            showPasswdCheckBox.Text = "显示";
            showPasswdCheckBox.UseVisualStyleBackColor = true;
            showPasswdCheckBox.CheckedChanged += showPasswdCheckBox_CheckedChanged;
            // 
            // selectFilesButton
            // 
            selectFilesButton.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            selectFilesButton.ForeColor = Color.DodgerBlue;
            selectFilesButton.Location = new Point(5, 5);
            selectFilesButton.Name = "selectFilesButton";
            selectFilesButton.Size = new Size(75, 50);
            selectFilesButton.TabIndex = 3;
            selectFilesButton.Text = "选择文件";
            selectFilesButton.UseVisualStyleBackColor = true;
            selectFilesButton.Click += SelectFilesButton_Click;
            // 
            // selectDirButton
            // 
            selectDirButton.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            selectDirButton.ForeColor = Color.DodgerBlue;
            selectDirButton.Location = new Point(5, 61);
            selectDirButton.Name = "selectDirButton";
            selectDirButton.Size = new Size(75, 50);
            selectDirButton.TabIndex = 4;
            selectDirButton.Text = "选择目录";
            selectDirButton.UseVisualStyleBackColor = true;
            selectDirButton.Click += SelectDirButton_Click;
            // 
            // decryptButton
            // 
            decryptButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            decryptButton.BackColor = Color.DeepSkyBlue;
            decryptButton.Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Bold);
            decryptButton.Location = new Point(593, 5);
            decryptButton.Name = "decryptButton";
            decryptButton.Size = new Size(106, 106);
            decryptButton.TabIndex = 5;
            decryptButton.Text = "解密";
            decryptButton.UseVisualStyleBackColor = false;
            decryptButton.Click += DecryptButton_Click;
            // 
            // pathRichTextBox
            // 
            pathRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pathRichTextBox.Location = new Point(86, 5);
            pathRichTextBox.Name = "pathRichTextBox";
            pathRichTextBox.ReadOnly = true;
            pathRichTextBox.Size = new Size(499, 105);
            pathRichTextBox.TabIndex = 6;
            pathRichTextBox.Text = "";
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "会话文件(*.xsh)|*.xsh|所有文件(*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "选择会话文件";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.Padding = new Padding(0, 5, 0, 5);
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6, Column7 });
            dataGridView1.Location = new Point(5, 156);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 30;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.Size = new Size(694, 415);
            dataGridView1.TabIndex = 7;
            dataGridView1.RowPostPaint += dataGridView1_RowPostPaint;
            // 
            // Column1
            // 
            Column1.HeaderText = "会话名称";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            // 
            // Column2
            // 
            Column2.HeaderText = "主机地址";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            // 
            // Column3
            // 
            Column3.HeaderText = "端口";
            Column3.Name = "Column3";
            Column3.ReadOnly = true;
            // 
            // Column4
            // 
            Column4.HeaderText = "用户名";
            Column4.Name = "Column4";
            Column4.ReadOnly = true;
            // 
            // Column5
            // 
            Column5.HeaderText = "密码";
            Column5.Name = "Column5";
            Column5.ReadOnly = true;
            // 
            // Column6
            // 
            Column6.HeaderText = "说明信息";
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            // 
            // Column7
            // 
            Column7.HeaderText = "会话路径";
            Column7.Name = "Column7";
            Column7.ReadOnly = true;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            button1.Location = new Point(627, 117);
            button1.Name = "button1";
            button1.Size = new Size(72, 32);
            button1.TabIndex = 8;
            button1.Text = "导出";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Location = new Point(5, 122);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.PlaceholderText = "搜索";
            searchTextBox.Size = new Size(310, 23);
            searchTextBox.TabIndex = 9;
            searchTextBox.TextChanged += searchTextBox_TextChanged;
            // 
            // githubLinkPictureBox
            // 
            githubLinkPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            githubLinkPictureBox.Cursor = Cursors.Hand;
            githubLinkPictureBox.Image = Properties.Resources.github;
            githubLinkPictureBox.Location = new Point(597, 122);
            githubLinkPictureBox.Name = "githubLinkPictureBox";
            githubLinkPictureBox.Size = new Size(22, 22);
            githubLinkPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            githubLinkPictureBox.TabIndex = 10;
            githubLinkPictureBox.TabStop = false;
            toolTip1.SetToolTip(githubLinkPictureBox, "项目主页");
            githubLinkPictureBox.Click += githubLinkPictureBox_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 576);
            Controls.Add(searchTextBox);
            Controls.Add(githubLinkPictureBox);
            Controls.Add(button1);
            Controls.Add(dataGridView1);
            Controls.Add(pathRichTextBox);
            Controls.Add(decryptButton);
            Controls.Add(selectDirButton);
            Controls.Add(selectFilesButton);
            Controls.Add(showPasswdCheckBox);
            Controls.Add(masterPasswdTextBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(704, 576);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Xpass";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)githubLinkPictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox masterPasswdTextBox;
        private CheckBox showPasswdCheckBox;
        private Button selectFilesButton;
        private Button selectDirButton;
        private Button decryptButton;
        private RichTextBox pathRichTextBox;
        private NotifyIcon notifyIcon1;
        private OpenFileDialog openFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
        private Button button1;
        private TextBox searchTextBox;
        private ToolTip toolTip1;
        private PictureBox githubLinkPictureBox;
    }
}

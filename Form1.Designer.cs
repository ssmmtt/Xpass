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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            masterPasswdLabel = new Label();
            masterPasswdTextBox = new TextBox();
            showPasswdCheckBox = new CheckBox();
            selectFilesButton = new Button();
            selectDirButton = new Button();
            decryptButton = new Button();
            pathRichTextBox = new RichTextBox();
            resultRichTextBox = new RichTextBox();
            notifyIcon1 = new NotifyIcon(components);
            openFileDialog1 = new OpenFileDialog();
            folderBrowserDialog1 = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // masterPasswdLabel
            // 
            masterPasswdLabel.AutoSize = true;
            masterPasswdLabel.Location = new Point(12, 151);
            masterPasswdLabel.Name = "masterPasswdLabel";
            masterPasswdLabel.Size = new Size(56, 17);
            masterPasswdLabel.TabIndex = 0;
            masterPasswdLabel.Text = "主密码：";
            // 
            // masterPasswdTextBox
            // 
            masterPasswdTextBox.Location = new Point(72, 149);
            masterPasswdTextBox.Name = "masterPasswdTextBox";
            masterPasswdTextBox.PasswordChar = '*';
            masterPasswdTextBox.PlaceholderText = "没有设置就不填";
            masterPasswdTextBox.Size = new Size(458, 23);
            masterPasswdTextBox.TabIndex = 5;
            // 
            // showPasswdCheckBox
            // 
            showPasswdCheckBox.AutoSize = true;
            showPasswdCheckBox.ForeColor = SystemColors.ControlDarkDark;
            showPasswdCheckBox.Location = new Point(536, 149);
            showPasswdCheckBox.Name = "showPasswdCheckBox";
            showPasswdCheckBox.Size = new Size(75, 21);
            showPasswdCheckBox.TabIndex = 2;
            showPasswdCheckBox.Text = "显示密码";
            showPasswdCheckBox.UseVisualStyleBackColor = true;
            showPasswdCheckBox.CheckedChanged += showPasswdCheckBox_CheckedChanged;
            // 
            // selectFilesButton
            // 
            selectFilesButton.Location = new Point(12, 11);
            selectFilesButton.Name = "selectFilesButton";
            selectFilesButton.Size = new Size(225, 44);
            selectFilesButton.TabIndex = 3;
            selectFilesButton.Text = "选择文件";
            selectFilesButton.UseVisualStyleBackColor = true;
            selectFilesButton.Click += SelectFilesButton_Click;
            // 
            // selectDirButton
            // 
            selectDirButton.Location = new Point(253, 11);
            selectDirButton.Name = "selectDirButton";
            selectDirButton.Size = new Size(225, 44);
            selectDirButton.TabIndex = 4;
            selectDirButton.Text = "选择目录";
            selectDirButton.UseVisualStyleBackColor = true;
            selectDirButton.Click += SelectDirButton_Click;
            // 
            // decryptButton
            // 
            decryptButton.BackColor = Color.DeepSkyBlue;
            decryptButton.Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 134);
            decryptButton.Location = new Point(486, 12);
            decryptButton.Name = "decryptButton";
            decryptButton.Size = new Size(127, 130);
            decryptButton.TabIndex = 5;
            decryptButton.Text = "解密";
            decryptButton.UseVisualStyleBackColor = false;
            decryptButton.Click += DecryptButton_Click;
            // 
            // pathRichTextBox
            // 
            pathRichTextBox.Location = new Point(12, 61);
            pathRichTextBox.Name = "pathRichTextBox";
            pathRichTextBox.ReadOnly = true;
            pathRichTextBox.Size = new Size(466, 81);
            pathRichTextBox.TabIndex = 6;
            pathRichTextBox.Text = "";
            // 
            // resultRichTextBox
            // 
            resultRichTextBox.Location = new Point(12, 177);
            resultRichTextBox.Name = "resultRichTextBox";
            resultRichTextBox.ReadOnly = true;
            resultRichTextBox.Size = new Size(601, 381);
            resultRichTextBox.TabIndex = 7;
            resultRichTextBox.Text = "";
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(625, 570);
            Controls.Add(resultRichTextBox);
            Controls.Add(pathRichTextBox);
            Controls.Add(decryptButton);
            Controls.Add(selectDirButton);
            Controls.Add(selectFilesButton);
            Controls.Add(showPasswdCheckBox);
            Controls.Add(masterPasswdTextBox);
            Controls.Add(masterPasswdLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Xpass";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label masterPasswdLabel;
        private TextBox masterPasswdTextBox;
        private CheckBox showPasswdCheckBox;
        private Button selectFilesButton;
        private Button selectDirButton;
        private Button decryptButton;
        private RichTextBox pathRichTextBox;
        private RichTextBox resultRichTextBox;
        private NotifyIcon notifyIcon1;
        private OpenFileDialog openFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
    }
}

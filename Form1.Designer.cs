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
            label1 = new Label();
            textBox1 = new TextBox();
            checkBox1 = new CheckBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            richTextBox1 = new RichTextBox();
            richTextBox2 = new RichTextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 151);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 0;
            label1.Text = "主密码：";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(74, 148);
            textBox1.Name = "textBox1";
            textBox1.PasswordChar = '*';
            textBox1.Size = new Size(458, 23);
            textBox1.TabIndex = 1;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.ForeColor = SystemColors.ControlDarkDark;
            checkBox1.Location = new Point(538, 151);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(75, 21);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "显示密码";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(12, 11);
            button1.Name = "button1";
            button1.Size = new Size(210, 44);
            button1.TabIndex = 3;
            button1.Text = "选择文件";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(266, 11);
            button2.Name = "button2";
            button2.Size = new Size(214, 44);
            button2.TabIndex = 4;
            button2.Text = "选择目录";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.BackColor = Color.DeepSkyBlue;
            button3.Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 134);
            button3.Location = new Point(486, 12);
            button3.Name = "button3";
            button3.Size = new Size(127, 89);
            button3.TabIndex = 5;
            button3.Text = "解密";
            button3.UseVisualStyleBackColor = false;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 61);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(468, 81);
            richTextBox1.TabIndex = 6;
            richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            richTextBox2.Location = new Point(12, 177);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(601, 381);
            richTextBox2.TabIndex = 7;
            richTextBox2.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(625, 570);
            Controls.Add(richTextBox2);
            Controls.Add(richTextBox1);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(checkBox1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private CheckBox checkBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private RichTextBox richTextBox1;
        private RichTextBox richTextBox2;
    }
}

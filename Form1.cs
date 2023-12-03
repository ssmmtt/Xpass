namespace Xpass
{
    public partial class Form1 : Form
    {
        private List<string> selectedFiles = [];
        public Form1()
        {
            InitializeComponent();
        }

        private void SelectFilesButton_Click(object sender, EventArgs e)
        {
            // 如果用户点击了 "确定" 按钮
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {

                if (openFileDialog1.FileNames.Length > 0)
                {
                    pathRichTextBox.Clear();
                    selectedFiles.Clear();
                    // 获取用户选择的文件路径
                    selectedFiles.AddRange(openFileDialog1.FileNames);

                    // 将数组的每个元素写入 RichTextBox，每个元素占据一行
                    foreach (string element in selectedFiles)
                    {
                        pathRichTextBox.AppendText(element + Environment.NewLine);
                    }
                }

            }
        }

        private void SelectDirButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                {
                    // 清空 RichTextBox
                    pathRichTextBox.Clear();
                    selectedFiles.Clear();
                    pathRichTextBox.AppendText(folderBrowserDialog1.SelectedPath);
                    selectedFiles = Xclass.GetXshFiles(folderBrowserDialog1.SelectedPath);

                }
            }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Count > 0)
            {
                string sid;
                if (masterPasswdTextBox.Text.Length > 0)
                {
                    sid = masterPasswdTextBox.Text;
                }
                else
                {
                    sid = Xclass.GetSid();
                }

                resultRichTextBox.Clear();
                int index = 1;
                foreach (string element in selectedFiles)
                {
                    resultRichTextBox.AppendText(index + "：" + element + Environment.NewLine);
                    var session = Xclass.FileParser(element, sid);
                    if (!session.isok)
                    {
                        MessageBox.Show(this, "解密失败，请检查主密码是否有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    resultRichTextBox.AppendText("    Host：" + session.host + Environment.NewLine);
                    resultRichTextBox.AppendText("    Port：" + session.port + Environment.NewLine);
                    resultRichTextBox.AppendText("    UserName：" + session.userName + Environment.NewLine);
                    resultRichTextBox.AppendText("    Password：" + session.password + Environment.NewLine);


                    resultRichTextBox.ScrollToCaret();
                    index++;
                }
            }
            else
            {
                MessageBox.Show(this, "请选择文件或者目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

        private void showPasswdCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (showPasswdCheckBox.Checked)
            {
                masterPasswdTextBox.PasswordChar = '\0';
            }
            else
            {
                masterPasswdTextBox.PasswordChar = '*';

            }
        }
    }



}

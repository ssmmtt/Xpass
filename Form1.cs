using System.Windows.Forms;

namespace Xpass
{
    public partial class Form1 : Form
    {
        private List<string> selectedFiles = [];
        public Form1()
        {
            InitializeComponent();
            // 设置每一列的最低宽度
            // ColumnWidthChanging 事件处理程序
            listView1.ColumnWidthChanging += (sender, e) =>
            {
                if (e.NewWidth < 30)
                {
                    e.Cancel = true; // 取消宽度改变
                    e.NewWidth = 30; // 设置宽度为最低宽度
                }
            };


            //listView1.DrawColumnHeader += listView1_DrawColumnHeader;
            //listView1.DrawSubItem += listView1_DrawSubItem;
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // 在这里处理表头的自定义绘制逻辑
            e.DrawDefault = true; // 绘制默认表头
        }
        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item.Selected)
            {
                SolidBrush brush = new SolidBrush(Color.FromArgb(204, 213, 240));

                e.Graphics.FillRectangle(brush, e.Bounds);    //选择后的颜色
            }
            else
            {
                if (e.ItemIndex % 2 == 0)
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);     //间隔色
                else
                    e.Graphics.FillRectangle(Brushes.YellowGreen, e.Bounds);
            }
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

        //private void AppendText(string text, Color color)
        //{
        //    resultRichTextBox.SelectionStart = resultRichTextBox.TextLength;
        //    resultRichTextBox.SelectionLength = 0;

        //    resultRichTextBox.SelectionColor = color;
        //    resultRichTextBox.AppendText(text);
        //    resultRichTextBox.SelectionColor = resultRichTextBox.ForeColor;
        //}


        //private void DecryptButton_Click(object sender, EventArgs e)
        //{
        //    if (selectedFiles.Count > 0)
        //    {
        //        string sid;
        //        if (masterPasswdTextBox.Text.Length > 0)
        //        {
        //            sid = masterPasswdTextBox.Text;
        //        }
        //        else
        //        {
        //            sid = Xclass.GetSid();
        //        }

        //        resultRichTextBox.Clear();
        //        int index = 1;
        //        foreach (string element in selectedFiles)
        //        {
        //            resultRichTextBox.AppendText(index + "：" + element + Environment.NewLine);
        //            var session = Xclass.FileParser(element, sid);
        //            resultRichTextBox.AppendText("    Host：" + session.host + Environment.NewLine);
        //            resultRichTextBox.AppendText("    Port：" + session.port + Environment.NewLine);
        //            resultRichTextBox.AppendText("    UserName：" + session.userName + Environment.NewLine);
        //            if (session.isok)
        //            {
        //                resultRichTextBox.AppendText("    Password：" + session.password + Environment.NewLine);
        //            }
        //            else
        //            {
        //                AppendText("    Password：解密失败，确认主密码是否正确！" + Environment.NewLine, Color.Red);
        //            }

        //            resultRichTextBox.ScrollToCaret();
        //            index++;
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show(this, "请选择文件或者目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}


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

                listView1.Items.Clear();
                int index = 1;
                foreach (string element in selectedFiles)
                {
                    var session = Xclass.FileParser(element, sid);
                    var password = "解密失败，确认主密码是否正确！";
                    if (session.isok)
                    {
                        password = session.password;
                    }

                    ListViewItem newItem = new ListViewItem(new[] { element, session.host, session.port, session.userName, password });
                    listView1.Items.Add(newItem);
                    newItem.EnsureVisible();
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

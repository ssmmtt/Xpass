using System.Diagnostics;
using System.Windows.Forms;

namespace Xpass
{
    public partial class Form1 : Form
    {
        private string[]? selectedFiles;
        private string? selectedDir;
        private string? selectedType;
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // 如果用户点击了 "确定" 按钮
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                // 获取用户选择的文件路径
                selectedFiles = openFileDialog1.FileNames;

                if (selectedFiles != null)
                {
                    selectedType = "file";
                    // 清空 RichTextBox
                    richTextBox1.Clear();

                    // 将数组的每个元素写入 RichTextBox，每个元素占据一行
                    foreach (string element in selectedFiles)
                    {
                        richTextBox1.AppendText(element + Environment.NewLine);
                    }
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                selectedDir = folderBrowserDialog1.SelectedPath;
                if (selectedDir != null)
                {
                    selectedType = "dir";
                    // 清空 RichTextBox
                    richTextBox1.Clear();

                    selectedDir = folderBrowserDialog1.SelectedPath;
                    richTextBox1.AppendText(selectedDir);

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(selectedType);
            switch (selectedType)
            {
                case "file":
                    break;
                case "dir":
                    break;
                default:
                    MessageBox.Show(this, "请选择文件或者目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); ;
                    break;
            }
        }
    }
}

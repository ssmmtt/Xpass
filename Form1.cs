using System.Diagnostics;
using System.Windows.Forms;

namespace Xpass
{
    public partial class Form1 : Form
    {
        private List<string> selectedFiles = [];
        public Form1()
        {
            InitializeComponent();
            ImproveDataGridView();
        }

        private void ImproveDataGridView()
        {

            // 启用隔行交替颜色
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;


            var indexColumn = new DataGridViewTextBoxColumn
            {
                Name = "IndexColumn",
                HeaderText = "序号",
                Width = 40
            };
            indexColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            var sessionPathColumn = new DataGridViewTextBoxColumn
            {
                Name = "SessionPathColumn",
                HeaderText = "会话路径",
                Width = 200
            };
            sessionPathColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            var hostColumn = new DataGridViewTextBoxColumn
            {
                Name = "HostAddressColumn",
                HeaderText = "主机地址",
                Width = 110
            };
            hostColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            var portColumn = new DataGridViewTextBoxColumn
            {
                Name = "PortColumn",
                HeaderText = "端口",
                Width = 50
            };
            portColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            var usernameColumn = new DataGridViewTextBoxColumn
            {
                Name = "UsernameColumn",
                HeaderText = "用户名",
                Width = 60
            };
            usernameColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;

            var passwordColumn = new DataGridViewTextBoxColumn
            {
                Name = "PasswordColumn",
                HeaderText = "密码",
                Width = dataGridView1.Width - 2 - (dataGridView1.RowHeadersWidth + indexColumn.Width + sessionPathColumn.Width + hostColumn.Width + portColumn.Width + usernameColumn.Width)

            };
            passwordColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            dataGridView1.Columns.AddRange(new DataGridViewTextBoxColumn[] { indexColumn, sessionPathColumn, hostColumn, portColumn, usernameColumn, passwordColumn });

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

        private void AddRowToDataGridView(List<object> rowData)
        {
            // 创建新的行
            DataGridViewRow row = new DataGridViewRow();

            // 添加每一列的单元格
            for (int i = 0; i < rowData.Count; i++)
            {
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                cell.Value = rowData[i];
                if (i == 5 && rowData[i] == "确认主密码是否正确！")
                {
                    cell.Style.ForeColor = Color.Red;
                }

                row.Cells.Add(cell);
            }

            // 将行添加到DataGridView
            dataGridView1.Rows.Add(row);
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
                dataGridView1.Rows.Clear();
                int index = 1;
                foreach (string element in selectedFiles)
                {
                    var session = Xclass.FileParser(element, sid);
                    var error = "确认主密码是否正确！";
                    if (!session.isok)
                    {
                        session.password = error;
                    }

                    AddRowToDataGridView(new List<object> { index, element, session.host, session.port, session.userName, session.password });

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

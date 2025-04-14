using System.Text;

namespace Xpass
{
    public partial class Form1 : Form
    {
        readonly string appKey = "Software\\Xpass";
        private List<string> selectedFiles = [];
        public Form1()
        {
            InitializeComponent();
            ImproveDataGridView();
            LoadLastConfig();
        }

        private void LoadLastConfig()
        {
            var path = RegistryCache.ReadFromRegistry(appKey, "path");
            var passwd = RegistryCache.ReadFromRegistry(appKey, "passwd");

            if (!string.IsNullOrEmpty(path))
            {
                pathRichTextBox.Text = path;
                var paths = path.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                selectedFiles.AddRange(paths);
            }

            if (!string.IsNullOrEmpty(passwd))
            {
                masterPasswdTextBox.Text = passwd;
            }
        }


        private void ImproveDataGridView()
        {

            // 启用隔行交替颜色
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // 百分比
            double[] columnPercentages = [35, 18, 8, 12, 26];
            int totalWidth = dataGridView1.Width - dataGridView1.RowHeadersWidth;
            // 分配列宽
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                if (i == dataGridView1.Columns.Count - 1)
                {
                    dataGridView1.Columns[i].Width = totalWidth - (dataGridView1.Columns[0].Width + dataGridView1.Columns[1].Width + dataGridView1.Columns[2].Width + dataGridView1.Columns[3].Width);
                    break;
                }
                int newWidth = (int)(totalWidth * columnPercentages[i] / 100);
                dataGridView1.Columns[i].Width = newWidth;
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
                        pathRichTextBox.AppendText(element + "\n");
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
                    selectedFiles.Add(folderBrowserDialog1.SelectedPath);
                }
            }
        }

        private void AddRowToDataGridView(List<object> rowData)
        {
            // 创建新的行
            DataGridViewRow row = new();

            // 添加每一列的单元格
            for (int i = 0; i < rowData.Count; i++)
            {
                var cell = new DataGridViewTextBoxCell { Value = rowData[i] };

                if (i == 4 && cell.Value?.ToString() == "确认主密码是否正确！")
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

                // 处理文件列表
                List<string> filesToProcess = [];
                foreach (string path in selectedFiles)
                {
                    if (File.Exists(path) && path.EndsWith(".xsh", StringComparison.OrdinalIgnoreCase))
                    {
                        filesToProcess.Add(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        filesToProcess.AddRange(Xclass.GetXshFiles(path) ?? []);
                    }
                }

                if (filesToProcess.Count == 0)
                {
                    MessageBox.Show(this, "未找到会话文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (string element in filesToProcess)
                {
                    var session = Xclass.FileParser(element, sid);
                    var error = "确认主密码是否正确！";
                    if (!session.isok)
                    {
                        session.password = error;
                    }

                    AddRowToDataGridView([element, session.host, session.port, session.userName, session.password]);
                }
                // 写入配置到注册表
                RegistryCache.WriteToRegistry(appKey, "path", pathRichTextBox.Text);
                RegistryCache.WriteToRegistry(appKey, "passwd", masterPasswdTextBox.Text);
            }
            else if (pathRichTextBox.Text == "")
            {
                MessageBox.Show(this, "请选择文件或者目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "未找到会话文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using SolidBrush brush = new(dataGridView1.RowHeadersDefaultCellStyle.ForeColor);
            // 计算行号（e.RowIndex 从 0 开始，所以要 +1）
            string rowIndex = (e.RowIndex + 1).ToString();

            // 获取行头的绘制区域
            SizeF size = e.Graphics.MeasureString(rowIndex, dataGridView1.Font);

            // 计算绘制位置，使文本居中对齐
            float x = e.RowBounds.Left + (dataGridView1.RowHeadersWidth - size.Width) / 2;
            float y = e.RowBounds.Top + (e.RowBounds.Height - size.Height) / 2;

            // 绘制行号
            e.Graphics.DrawString(rowIndex, dataGridView1.Font, brush, x, y);

            // 根据行号宽度动态调整 RowHeadersWidth
            int newWidth = (int)size.Width + 10; // 加一些额外空间，防止文本贴边
            if (newWidth > dataGridView1.RowHeadersWidth)
            {
                dataGridView1.RowHeadersWidth = newWidth;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 判断表格是否为空
            if (dataGridView1.Rows.Count == 0 || (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow))
            {
                MessageBox.Show("没有会话信息，无法导出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "CSV 文件 (*.csv)|*.csv",
                Title = "保存 CSV 文件",
                FileName = "xshell会话.csv"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportDataGridViewToCSV(dataGridView1, saveFileDialog.FileName);
            }
        }

        static void ExportDataGridViewToCSV(DataGridView dgv, string filePath)
        {
            try
            {
                StringBuilder csvContent = new();

                // 添加表头
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    csvContent.Append(FormatCsvField(dgv.Columns[i].HeaderText));
                    if (i < dgv.Columns.Count - 1)
                        csvContent.Append(',');
                }
                csvContent.AppendLine();

                // 添加数据行
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < dgv.Columns.Count; i++)
                        {
                            csvContent.Append(FormatCsvField(row.Cells[i].Value?.ToString() ?? string.Empty));  // 确保不会传递 null
                            if (i < dgv.Columns.Count - 1)
                                csvContent.Append(',');
                        }
                        csvContent.AppendLine();
                    }
                }


                // 保存到文件
                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
                MessageBox.Show("数据导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 处理 CSV 格式，避免逗号、双引号、换行符导致解析错误
        static string FormatCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\""; // 空值用双引号包裹，表示为空字符串

            bool containsSpecialChars = field.Contains(',') || field.Contains('\"') || field.Contains('\n') || field.Contains('\r');
            if (containsSpecialChars)
            {
                // 替换双引号为两个双引号（CSV 规范）
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\""; // 用双引号包裹字段
            }

            return field;
        }


    }
}

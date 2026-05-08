using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Xpass
{
    public partial class Form1 : Form
    {
        readonly string appKey = "Software\\Xpass";
        private List<string> selectedFiles = [];
        private bool hasTriedAutoDecrypt;
        public Form1()
        {
            InitializeComponent();
            EnableDataGridViewDoubleBuffered(dataGridView1);
            LoadLastConfig();
            LoadWindowSize();
            this.Resize += Form1_Resize;
            this.Shown += Form1_Shown;
        }

        /// <summary>
        /// DataGridView 未公开 DoubleBuffered，开启后可减轻滚动时闪烁与撕裂。
        /// </summary>
        private static void EnableDataGridViewDoubleBuffered(DataGridView dgv)
        {
            typeof(Control).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dgv,
                [true]);
        }

        /// <summary>
        /// 根据当前行数一次性设置行头宽度，避免在 RowPostPaint 里改宽度导致滚动时反复布局。
        /// </summary>
        private void SyncDataGridRowHeadersWidth()
        {
            int count = dataGridView1.Rows.Count;
            if (count == 0)
            {
                dataGridView1.RowHeadersWidth = 30;
                return;
            }

            string sample = count.ToString();
            Size sz = TextRenderer.MeasureText(sample, dataGridView1.Font);
            dataGridView1.RowHeadersWidth = Math.Max(30, sz.Width + 14);
        }

        private void githubLinkPictureBox_Click(object? sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/ssmmtt/Xpass",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "无法打开链接: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            // 窗口完全显示后再调整列宽，确保 DataGridView 大小已确定
            ImproveDataGridView();
            if (!hasTriedAutoDecrypt)
            {
                hasTriedAutoDecrypt = true;
                if (HasValidSelectedInput())
                {
                    RunDecrypt(false);
                }
            }
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


        /// <summary>
        /// 列区域可用宽度：应用客户区宽度，减去行头与纵向滚动条（未计入时列总宽会略大于可视区，导致始终出现横向滚动条）。
        /// </summary>
        private int GetDataGridViewAvailableColumnsWidth()
        {
            int inner = dataGridView1.ClientSize.Width - dataGridView1.RowHeadersWidth;

            bool reservedVerticalScroll = false;
            foreach (Control c in dataGridView1.Controls)
            {
                if (c is VScrollBar vsb)
                {
                    if (vsb.Visible)
                    {
                        inner -= vsb.Width;
                        reservedVerticalScroll = true;
                    }
                    break;
                }
            }

            // 布局尚未完成时竖条可能尚未 Visible，但行数已超出可视行时仍应预留，避免 Improve 与真实布局不一致
            if (!reservedVerticalScroll && dataGridView1.Rows.Count > 0)
            {
                int headerH = dataGridView1.ColumnHeadersVisible ? dataGridView1.ColumnHeadersHeight : 0;
                int rowH = dataGridView1.RowTemplate.Height > 0 ? dataGridView1.RowTemplate.Height : 22;
                int bodyH = Math.Max(0, dataGridView1.ClientSize.Height - headerH);
                int approxVisibleRows = bodyH / Math.Max(1, rowH);
                if (approxVisibleRows > 0 && dataGridView1.Rows.Count > approxVisibleRows)
                    inner -= SystemInformation.VerticalScrollBarWidth;
            }

            // 高 DPI / 3D 边框 / 网格线绘制与理论客户区偶有 1～2px 偏差，略减可避免仍出现横向微滚动
            const int layoutFudgePx = 2;
            return Math.Max(1, inner - layoutFudgePx);
        }

        private void ImproveDataGridView()
        {

            // 启用隔行交替颜色
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // 百分比：会话名称、主机地址、端口、用户名、密码、说明信息、会话路径
            double[] columnPercentages = [17, 17, 7, 9, 16, 13, 21];
            int totalWidth = GetDataGridViewAvailableColumnsWidth();
            // 分配列宽
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                if (i == dataGridView1.Columns.Count - 1)
                {
                    // 最后一列使用剩余空间
                    int usedWidth = 0;
                    for (int j = 0; j < dataGridView1.Columns.Count - 1; j++)
                    {
                        usedWidth += dataGridView1.Columns[j].Width;
                    }
                    dataGridView1.Columns[i].Width = totalWidth - usedWidth;
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

        private bool HasValidSelectedInput()
        {
            return selectedFiles.Any(path =>
                (File.Exists(path) && path.EndsWith(".xsh", StringComparison.OrdinalIgnoreCase)) ||
                Directory.Exists(path));
        }

        private static List<string> BuildFilesToProcess(IEnumerable<string> paths)
        {
            List<string> filesToProcess = [];
            foreach (string path in paths)
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

            return filesToProcess;
        }

        private void RunDecrypt(bool showMessage)
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
                // 清除搜索内容，重置筛选状态
                searchTextBox.Clear();

                // 处理文件列表
                List<string> filesToProcess = BuildFilesToProcess(selectedFiles);

                if (filesToProcess.Count == 0)
                {
                    if (showMessage)
                    {
                        MessageBox.Show(this, "未找到会话文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    SyncDataGridRowHeadersWidth();
                    ImproveDataGridView();
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

                    // 获取文件名（去掉扩展名）作为会话名称
                    string sessionName = Path.GetFileNameWithoutExtension(element);
                    // 列顺序：会话名称、主机地址、端口、用户名、密码、说明信息、会话路径
                    AddRowToDataGridView([sessionName, session.host, session.port, session.userName, session.password, session.description ?? "", element]);
                }
                SyncDataGridRowHeadersWidth();
                ImproveDataGridView();
                // 写入配置到注册表
                RegistryCache.WriteToRegistry(appKey, "path", pathRichTextBox.Text);
                RegistryCache.WriteToRegistry(appKey, "passwd", masterPasswdTextBox.Text);
            }
            else if (pathRichTextBox.Text == "" && showMessage)
            {
                MessageBox.Show(this, "请选择文件或者目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (showMessage)
            {
                MessageBox.Show(this, "未找到会话文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            RunDecrypt(true);
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
            if (e.RowIndex < 0)
                return;

            // 行头宽度在 SyncDataGridRowHeadersWidth 中统一更新；此处仅用 TextRenderer 绘制，避免 MeasureString 与绘制期间改布局导致滚动卡顿。
            string rowLabel = (e.RowIndex + 1).ToString();
            var headerRect = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dataGridView1.RowHeadersWidth,
                e.RowBounds.Height);
            Color fore = dataGridView1.RowHeadersDefaultCellStyle.ForeColor;
            TextRenderer.DrawText(
                e.Graphics,
                rowLabel,
                dataGridView1.Font,
                headerRect,
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 判断表格是否为空或没有可见的行（适配搜索筛选功能）
            bool hasVisibleRows = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    hasVisibleRows = true;
                    break;
                }
            }

            if (!hasVisibleRows)
            {
                MessageBox.Show("没有可导出的会话信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // 添加数据行（只导出可见的行，适配搜索筛选功能）
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (!row.IsNewRow && row.Visible)
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

        private void LoadWindowSize()
        {
            var widthStr = RegistryCache.ReadFromRegistry(appKey, "windowWidth");
            var heightStr = RegistryCache.ReadFromRegistry(appKey, "windowHeight");
            var windowStateStr = RegistryCache.ReadFromRegistry(appKey, "windowState");

            if (!string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out int width) &&
                !string.IsNullOrEmpty(heightStr) && int.TryParse(heightStr, out int height))
            {
                // 确保窗口大小不小于最小尺寸
                if (width >= this.MinimumSize.Width && height >= this.MinimumSize.Height)
                {
                    this.Size = new Size(width, height);
                }
            }
            else
            {
                // 如果没有保存的尺寸，使用最小窗口大小
                this.Size = this.MinimumSize;
            }

            // 恢复窗口状态
            if (!string.IsNullOrEmpty(windowStateStr) && Enum.TryParse<FormWindowState>(windowStateStr, out var windowState))
            {
                if (windowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            // 只有在窗口状态为 Normal 时才保存大小
            if (this.WindowState == FormWindowState.Normal)
            {
                RegistryCache.WriteToRegistry(appKey, "windowWidth", this.Width.ToString());
                RegistryCache.WriteToRegistry(appKey, "windowHeight", this.Height.ToString());
            }
            RegistryCache.WriteToRegistry(appKey, "windowState", this.WindowState.ToString());

            // 调整 DataGridView 列宽以适应窗口大小变化
            if (dataGridView1.Columns.Count > 0)
            {
                ImproveDataGridView();
            }
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchTextBox.Text.Trim();
            
            // 如果搜索框为空，显示所有行并清除高亮
            if (string.IsNullOrEmpty(searchText))
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Visible = true;
                        // 清除所有单元格的高亮样式
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = Color.Empty;
                            cell.Style.ForeColor = Color.Empty;
                        }
                    }
                }
                return;
            }

            // 遍历所有行进行筛选
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                bool isMatch = false;
                
                // 检查需要匹配的列：会话名称(0)、主机地址(1)、说明信息(5)
                int[] searchColumns = { 0, 1, 5 };
                
                foreach (int colIndex in searchColumns)
                {
                    if (colIndex < row.Cells.Count)
                    {
                        string cellValue = row.Cells[colIndex].Value?.ToString() ?? "";
                        
                        // 不区分大小写的匹配
                        if (cellValue.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;
                            // 高亮匹配的单元格
                            row.Cells[colIndex].Style.BackColor = Color.Yellow;
                            row.Cells[colIndex].Style.ForeColor = Color.Black;
                        }
                        else
                        {
                            // 清除不匹配单元格的高亮
                            row.Cells[colIndex].Style.BackColor = Color.Empty;
                            row.Cells[colIndex].Style.ForeColor = Color.Empty;
                        }
                    }
                }
                
                // 显示或隐藏行
                row.Visible = isMatch;
                
                // 如果行不匹配，清除其他列的高亮
                if (!isMatch)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        if (!searchColumns.Contains(i))
                        {
                            row.Cells[i].Style.BackColor = Color.Empty;
                            row.Cells[i].Style.ForeColor = Color.Empty;
                        }
                    }
                }
            }
        }
    }
}

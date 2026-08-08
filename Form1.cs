using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace Xpass
{
    public partial class Form1 : Form
    {
        readonly string appKey = "Software\\Xpass";
        private List<string> selectedFiles = [];
        private bool hasTriedAutoDecrypt;
        private const int ColHost = 1;
        private const int ColPort = 2;
        private const int ColUserName = 3;
        private const int ColPassword = 4;
        private const int ColSessionPath = 6;
        public Form1()
        {
            InitializeComponent();
            Text = $"Xpass - V{Application.ProductVersion}";
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

                // 检查所有列
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string cellValue = cell.Value?.ToString() ?? "";

                    // 不区分大小写的匹配
                    if (cellValue.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = true;
                        // 高亮匹配的单元格
                        cell.Style.BackColor = Color.Yellow;
                        cell.Style.ForeColor = Color.Black;
                    }
                    else
                    {
                        // 清除不匹配单元格的高亮
                        cell.Style.BackColor = Color.Empty;
                        cell.Style.ForeColor = Color.Empty;
                    }
                }

                // 显示或隐藏行
                row.Visible = isMatch;
            }
        }

        private void dataGridView1_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            // ColumnIndex < 0 表示点在序号（行头）上
            if (e.Button != MouseButtons.Right || e.RowIndex < 0)
                return;

            var selectedRowIndices = dataGridView1.SelectedCells
                .Cast<DataGridViewCell>()
                .Select(c => c.RowIndex)
                .Where(i => i >= 0)
                .Distinct()
                .ToHashSet();

            // 行头选中时 SelectedRows 也可能有值
            foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
            {
                if (!selectedRow.IsNewRow)
                    selectedRowIndices.Add(selectedRow.Index);
            }

            if (!selectedRowIndices.Contains(e.RowIndex))
            {
                selectedRowIndices = [e.RowIndex];
            }

            // 右键菜单操作按整行处理
            dataGridView1.ClearSelection();
            foreach (int rowIndex in selectedRowIndices.OrderBy(i => i))
            {
                if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
                    dataGridView1.Rows[rowIndex].Selected = true;
            }

            bool singleRow = selectedRowIndices.Count == 1;
            openInFolderMenuItem.Visible = singleRow;
            copyPasswordMenuItem.Visible = singleRow;
            sessionContextMenuStrip.Show(dataGridView1, dataGridView1.PointToClient(Cursor.Position));
        }

        private void dataGridView1_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            dataGridView1.ClearSelection();
            dataGridView1.Rows[e.RowIndex].Selected = true;
            ConnectSelectedSessions();
        }

        private List<DataGridViewRow> GetSelectedSessionRows()
        {
            var fromRows = dataGridView1.SelectedRows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .ToList();

            if (fromRows.Count > 0)
                return fromRows.OrderBy(r => r.Index).ToList();

            // 仅选中单元格时从 SelectedCells 汇总行
            return dataGridView1.SelectedCells
                .Cast<DataGridViewCell>()
                .Select(c => c.OwningRow)
                .Where(r => r is not null && !r.IsNewRow)
                .Distinct()
                .OrderBy(r => r!.Index)
                .Cast<DataGridViewRow>()
                .ToList();
        }

        private static string GetCellText(DataGridViewRow row, int columnIndex)
        {
            return row.Cells[columnIndex].Value?.ToString() ?? string.Empty;
        }

        private void connectSessionMenuItem_Click(object? sender, EventArgs e)
        {
            ConnectSelectedSessions();
        }

        private void ConnectSelectedSessions()
        {
            var rows = GetSelectedSessionRows();
            if (rows.Count == 0)
                return;

            // 先判断是否有默认打开方式；没有则不要 ShellExecute，避免弹出「选择打开方式」
            bool hasDefaultOpen = HasXshDefaultOpenAssociation();
            string? xshellExe = null;
            int successCount = 0;
            var errors = new List<string>();

            foreach (var row in rows)
            {
                string sessionPath = GetCellText(row, ColSessionPath);
                if (string.IsNullOrWhiteSpace(sessionPath) || !File.Exists(sessionPath))
                {
                    errors.Add($"文件不存在：{sessionPath}");
                    continue;
                }

                try
                {
                    if (hasDefaultOpen)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = sessionPath,
                            UseShellExecute = true
                        });
                        successCount++;
                        continue;
                    }

                    xshellExe ??= XshellLocator.ResolveExecutable();
                    if (xshellExe is null)
                    {
                        errors.Add("未找到 Xshell 可执行文件");
                        break;
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = xshellExe,
                        Arguments = $"\"{sessionPath}\"",
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(xshellExe) ?? string.Empty
                    });
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(sessionPath)}：{ex.Message}");
                }
            }

            if (successCount == 0)
            {
                string message = errors.Count > 0
                    ? string.Join("\n", errors.Distinct())
                    : "无法打开会话。";
                if (!hasDefaultOpen && xshellExe is null)
                {
                    message = "未找到默认打开方式，也未在本机找到 Xshell。\n请安装 Xshell 或为 .xsh 设置默认程序后重试。";
                }

                MessageBox.Show(this, message, "连接会话", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (errors.Count > 0)
            {
                MessageBox.Show(
                    this,
                    $"已打开 {successCount} 个会话，部分失败：\n{string.Join("\n", errors.Distinct())}",
                    "连接会话",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 是否存在可用的 .xsh 默认打开方式（用户 UserChoice 或 HKCR 扩展名关联）。
        /// 无默认时不得 ShellExecute，否则会弹出「选择打开方式」。
        /// </summary>
        private static bool HasXshDefaultOpenAssociation()
        {
            try
            {
                using (var userChoice = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.xsh\UserChoice"))
                {
                    var userProgId = userChoice?.GetValue("ProgId")?.ToString();
                    if (!string.IsNullOrWhiteSpace(userProgId))
                        return true;
                }

                using var extKey = Registry.ClassesRoot.OpenSubKey(".xsh");
                var progId = extKey?.GetValue(null)?.ToString();
                if (string.IsNullOrWhiteSpace(progId))
                    return false;

                using var commandKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
                var command = commandKey?.GetValue(null)?.ToString();
                return !string.IsNullOrWhiteSpace(command);
            }
            catch
            {
                return false;
            }
        }

        private void openInFolderMenuItem_Click(object? sender, EventArgs e)
        {
            var rows = GetSelectedSessionRows();
            if (rows.Count != 1)
                return;

            string sessionPath = GetCellText(rows[0], ColSessionPath);
            if (string.IsNullOrWhiteSpace(sessionPath) || !File.Exists(sessionPath))
            {
                MessageBox.Show(this, "会话文件不存在。", "打开所在目录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{sessionPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "无法打开文件夹：" + ex.Message, "打开所在目录", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyPasswordMenuItem_Click(object? sender, EventArgs e)
        {
            var rows = GetSelectedSessionRows();
            if (rows.Count != 1)
                return;

            Clipboard.SetText(GetCellText(rows[0], ColPassword));
        }

        private void copyConnectionInfoMenuItem_Click(object? sender, EventArgs e)
        {
            var rows = GetSelectedSessionRows();
            if (rows.Count == 0)
                return;

            var lines = rows.Select(row =>
                string.Join('\t',
                    GetCellText(row, ColHost),
                    GetCellText(row, ColPort),
                    GetCellText(row, ColUserName),
                    GetCellText(row, ColPassword)));

            Clipboard.SetText(string.Join('\n', lines));
        }

        private void deleteSessionFileMenuItem_Click(object? sender, EventArgs e)
        {
            var rows = GetSelectedSessionRows();
            if (rows.Count == 0)
                return;

            var names = rows
                .Select(r => Path.GetFileName(GetCellText(r, ColSessionPath)))
                .Where(n => !string.IsNullOrEmpty(n))
                .Take(5)
                .ToList();

            string summary = string.Join("\n", names);
            if (rows.Count > names.Count)
                summary += $"\n…共 {rows.Count} 个文件";

            var result = MessageBox.Show(
                this,
                $"确定将以下 {rows.Count} 个会话文件删除到回收站吗？\n\n{summary}",
                "删除会话文件",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            var failed = new List<string>();
            var rowsToRemove = new List<DataGridViewRow>();

            foreach (var row in rows)
            {
                string sessionPath = GetCellText(row, ColSessionPath);
                try
                {
                    if (File.Exists(sessionPath))
                    {
                        FileSystem.DeleteFile(
                            sessionPath,
                            UIOption.OnlyErrorDialogs,
                            RecycleOption.SendToRecycleBin);
                    }

                    rowsToRemove.Add(row);
                }
                catch (Exception ex)
                {
                    failed.Add($"{Path.GetFileName(sessionPath)}：{ex.Message}");
                }
            }

            foreach (var row in rowsToRemove.OrderByDescending(r => r.Index))
            {
                dataGridView1.Rows.Remove(row);
            }

            SyncDataGridRowHeadersWidth();
            ImproveDataGridView();

            if (failed.Count > 0)
            {
                MessageBox.Show(
                    this,
                    $"部分文件删除失败：\n{string.Join("\n", failed)}",
                    "删除会话文件",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}

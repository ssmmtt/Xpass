using System.Diagnostics;
using Microsoft.Win32;

namespace Xpass
{
    /// <summary>
    /// 快速定位 Xshell.exe：优先读本应用注册表缓存，失效则按快捷方式 / 厂商注册表 / 常见路径 / 进程 / 浅层扫描重搜并回写。
    /// </summary>
    internal static class XshellLocator
    {
        private const string AppKey = "Software\\Xpass";
        private const string CacheValueName = "XshellPath";
        private const string ExeName = "Xshell.exe";
        private const int ShallowSearchMaxDepth = 5;

        private static readonly HashSet<string> ShallowSkipDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Windows",
            "WinSxS",
            "WindowsApps",
            "$Recycle.Bin",
            "System Volume Information",
            "node_modules",
            ".git",
        };

        /// <summary>返回可用的 Xshell.exe 完整路径；未找到时清空缓存并返回 null。</summary>
        public static string? ResolveExecutable()
        {
            var cached = RegistryCache.ReadFromRegistry(AppKey, CacheValueName);
            if (!string.IsNullOrWhiteSpace(cached) && File.Exists(cached))
            {
                return cached;
            }

            var found = SearchExecutable();
            if (found is not null)
            {
                RegistryCache.WriteToRegistry(AppKey, CacheValueName, found);
                return found;
            }

            RegistryCache.DeleteFromRegistry(AppKey, CacheValueName);
            return null;
        }

        private static string? SearchExecutable()
        {
            var fromDesktop = TryFindFromShortcuts(GetDesktopRoots(), recursive: false);
            if (fromDesktop is not null)
                return fromDesktop;

            var fromStartMenu = TryFindFromShortcuts(GetStartMenuRoots(), recursive: true);
            if (fromStartMenu is not null)
                return fromStartMenu;

            var fromRegistry = TryFindFromRegistry();
            if (fromRegistry is not null)
                return fromRegistry;

            foreach (var candidate in EnumerateFastPathCandidates())
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            var fromProcess = TryFindFromRunningProcess();
            if (fromProcess is not null)
                return fromProcess;

            return TryShallowFindXshell();
        }

        private static IEnumerable<string> GetDesktopRoots()
        {
            yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        }

        private static IEnumerable<string> GetStartMenuRoots()
        {
            foreach (var startMenu in new[]
                     {
                         Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                         Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                     })
            {
                if (string.IsNullOrEmpty(startMenu))
                    continue;

                var programs = Path.Combine(startMenu, "Programs");
                if (!Directory.Exists(programs))
                    continue;

                foreach (var rel in new[] { "NetSarang", "NetSarang Computer", "Xshell" })
                {
                    var preferred = Path.Combine(programs, rel);
                    if (Directory.Exists(preferred))
                        yield return preferred;
                }

                yield return programs;
            }
        }

        private static string? TryFindFromShortcuts(IEnumerable<string> roots, bool recursive)
        {
            foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(root))
                    continue;

                IEnumerable<string> links;
                try
                {
                    links = Directory.EnumerateFiles(
                        root,
                        "*.lnk",
                        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                var linkList = links.ToList();
                foreach (var passXshellFirst in new[] { true, false })
                {
                    foreach (var lnk in linkList)
                    {
                        if (ShortcutLooksLikeXshell(lnk) != passXshellFirst)
                            continue;

                        var target = TryReadShortcutTarget(lnk);
                        var resolved = TryResolveXshellPath(target);
                        if (resolved is not null)
                            return resolved;
                    }
                }
            }

            return null;
        }

        private static bool ShortcutLooksLikeXshell(string lnkPath)
        {
            var name = Path.GetFileNameWithoutExtension(lnkPath);
            return name.Contains("Xshell", StringComparison.OrdinalIgnoreCase)
                || lnkPath.Contains("NetSarang", StringComparison.OrdinalIgnoreCase)
                || lnkPath.Contains("Xshell", StringComparison.OrdinalIgnoreCase);
        }

        private static string? TryReadShortcutTarget(string lnkPath)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType is null)
                    return null;

                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(lnkPath);
                string target = shortcut.TargetPath;
                return string.IsNullOrWhiteSpace(target) ? null : target.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string? TryFindFromRegistry()
        {
            string?[] roots =
            [
                TryResolveFromRegistryKey(Registry.CurrentUser, @"Software\NetSarang\Xshell"),
                TryResolveFromRegistryKey(Registry.LocalMachine, @"SOFTWARE\NetSarang\Xshell"),
                TryResolveFromRegistryKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\NetSarang\Xshell"),
                TryResolveFromRegistryKey(Registry.CurrentUser, @"Software\NetSarang Computer\Xshell"),
                TryResolveFromRegistryKey(Registry.LocalMachine, @"SOFTWARE\NetSarang Computer\Xshell"),
                TryResolveFromRegistryKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\NetSarang Computer\Xshell"),
            ];

            foreach (var path in roots)
            {
                if (path is not null)
                    return path;
            }

            return TryFindFromUninstallKeys();
        }

        private static string? TryResolveFromRegistryKey(RegistryKey root, string subKeyPath)
        {
            try
            {
                using var key = root.OpenSubKey(subKeyPath);
                if (key is null)
                    return null;

                foreach (var valueName in new[] { "InstallPath", "install_path", "Path", "AppPath", "InstallDir" })
                {
                    var raw = key.GetValue(valueName) as string;
                    var resolved = TryResolveXshellPath(raw);
                    if (resolved is not null)
                        return resolved;
                }

                // 版本子键（如 7、8）
                foreach (var subName in key.GetSubKeyNames())
                {
                    try
                    {
                        using var sub = key.OpenSubKey(subName);
                        if (sub is null)
                            continue;

                        foreach (var valueName in new[] { "InstallPath", "Path", "InstallDir" })
                        {
                            var raw = sub.GetValue(valueName) as string;
                            var resolved = TryResolveXshellPath(raw);
                            if (resolved is not null)
                                return resolved;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static string? TryFindFromUninstallKeys()
        {
            string[] hives =
            [
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
            ];

            foreach (var hivePath in hives)
            {
                try
                {
                    using var uninstall = Registry.LocalMachine.OpenSubKey(hivePath);
                    if (uninstall is null)
                        continue;

                    foreach (var subName in uninstall.GetSubKeyNames())
                    {
                        try
                        {
                            using var sub = uninstall.OpenSubKey(subName);
                            if (sub is null)
                                continue;

                            var displayName = sub.GetValue("DisplayName") as string;
                            if (displayName is null
                                || !displayName.Contains("Xshell", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            foreach (var valueName in new[] { "InstallLocation", "DisplayIcon", "UninstallString" })
                            {
                                var raw = sub.GetValue(valueName) as string;
                                var resolved = TryResolveXshellPath(raw);
                                if (resolved is not null)
                                    return resolved;
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return null;
        }

        private static IEnumerable<string> EnumerateFastPathCandidates()
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            foreach (var baseDir in new[] { programFilesX86, programFiles }.Where(d => !string.IsNullOrEmpty(d)))
            {
                foreach (var vendor in new[] { "NetSarang", "NetSarang Computer" })
                {
                    foreach (var product in new[] { "Xshell 8", "Xshell 7", "Xshell", "Xshell 6" })
                    {
                        yield return Path.Combine(baseDir, vendor, product, ExeName);
                    }
                }
            }

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                    continue;

                var root = drive.RootDirectory.FullName;
                foreach (var pf in new[] { "Program Files (x86)", "Program Files" })
                {
                    foreach (var vendor in new[] { "NetSarang", "NetSarang Computer" })
                    {
                        foreach (var product in new[] { "Xshell 8", "Xshell 7", "Xshell" })
                        {
                            yield return Path.Combine(root, pf, vendor, product, ExeName);
                        }
                    }
                }
            }
        }

        private static string? TryFindFromRunningProcess()
        {
            foreach (var proc in Process.GetProcessesByName("Xshell"))
            {
                try
                {
                    var path = proc.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(path) && IsXshellExe(path))
                        return path;
                }
                catch
                {
                    // 权限或 32/64 位限制
                }
                finally
                {
                    proc.Dispose();
                }
            }

            return null;
        }

        private static string? TryShallowFindXshell()
        {
            var drives = DriveInfo.GetDrives()
                .Where(static d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(static d => d.Name)
                .OrderBy(static n => n.StartsWith("C", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ToArray();

            if (drives.Length == 0)
                return null;

            string? found = null;
            var gate = new object();

            Parallel.ForEach(
                drives,
                new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, drives.Length) },
                drive =>
                {
                    if (Volatile.Read(ref found) is not null)
                        return;

                    foreach (var seed in GetShallowSearchSeeds(drive))
                    {
                        var hit = ShallowFindUnderRoot(seed);
                        if (hit is null)
                            continue;

                        lock (gate)
                        {
                            found ??= hit;
                        }

                        return;
                    }
                });

            return found;
        }

        private static IEnumerable<string> GetShallowSearchSeeds(string driveRoot)
        {
            var root = driveRoot.TrimEnd('\\');
            foreach (var rel in new[]
                     {
                         @"Program Files (x86)",
                         @"Program Files",
                         @"NetSarang",
                         @"NetSarang Computer",
                     })
            {
                var seed = Path.Combine(root, rel);
                if (Directory.Exists(seed))
                    yield return seed;
            }
        }

        private static string? ShallowFindUnderRoot(string root)
        {
            var queue = new Queue<(string Dir, int Depth)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (dir, depth) = queue.Dequeue();
                if (depth > ShallowSearchMaxDepth)
                    continue;

                try
                {
                    var direct = Path.Combine(dir, ExeName);
                    if (File.Exists(direct) && IsXshellExe(direct))
                        return direct;
                }
                catch
                {
                    // ignore
                }

                IEnumerable<string> subDirs;
                try
                {
                    subDirs = Directory.EnumerateDirectories(dir);
                }
                catch
                {
                    continue;
                }

                foreach (var sub in subDirs)
                {
                    var name = Path.GetFileName(sub);
                    if (ShallowSkipDirectoryNames.Contains(name))
                        continue;

                    if (depth < ShallowSearchMaxDepth)
                        queue.Enqueue((sub, depth + 1));
                }
            }

            return null;
        }

        private static string? TryResolveXshellPath(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var path = raw.Trim().Trim('"');
            if (path.Contains(',', StringComparison.Ordinal))
                path = path.Split(',')[0].Trim().Trim('"');

            // UninstallString 可能带参数
            if (path.Contains(".exe", StringComparison.OrdinalIgnoreCase))
            {
                var exeIdx = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                path = path[..(exeIdx + 4)].Trim().Trim('"');
            }

            if (IsXshellExe(path))
                return path;

            if (Directory.Exists(path))
            {
                var direct = Path.Combine(path, ExeName);
                if (File.Exists(direct))
                    return direct;
            }

            var parent = Path.GetDirectoryName(path);
            while (!string.IsNullOrEmpty(parent))
            {
                var candidate = Path.Combine(parent, ExeName);
                if (File.Exists(candidate))
                    return candidate;
                parent = Path.GetDirectoryName(parent);
            }

            return null;
        }

        private static bool IsXshellExe(string path) =>
            path.EndsWith(ExeName, StringComparison.OrdinalIgnoreCase) && File.Exists(path);
    }
}

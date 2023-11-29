using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Xpass
{
    class Xclass
    {

        public struct Xsh
        {
            public string host;
            public string userName;
            public string password;
            public string version;
        }

    

        public static Xsh XSHParser(string xshPath)
        {
            Xsh xsh;
            xsh.host = null;
            xsh.userName = null;
            xsh.password = null;
            xsh.version = null;

            var sid = GetSid();
            using (StreamReader sr = new StreamReader(xshPath))
            {
                string rawPass;
                while ((rawPass = sr.ReadLine()) != null)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"Host=(.*?)"))
                    {
                        xsh.host = rawPass.Replace("Host=", "");
                    }
                    if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"Password=(.*?)"))
                    {
                        rawPass = rawPass.Replace("Password=", "");
                        rawPass = rawPass.Replace("\r\n", "");
                        if (rawPass.Equals(""))
                        {
                            continue;
                        }

                    }
                    if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"UserName=(.*?)"))
                    {
                        xsh.userName = rawPass.Replace("UserName=", "");
                    }
                    if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"Version=(.*?)"))
                    {
                        xsh.version = rawPass.Replace("Version=", "");
                    }
                }
            }
            return xsh;
        }

        public static string GetSid()
        {
            string userName = WindowsIdentity.GetCurrent().Name;
            string sid = new SecurityIdentifier(WindowsIdentity.GetCurrent().User.Value).ToString();

            return $"{userName}{sid}";
        }

        public static List<string> GetXshFiles(string directory)
        {
            List<string> xshFiles = new List<string>();

            try
            {
                // 获取当前目录下的所有 .xsh 文件
                string[] files = Directory.GetFiles(directory, "*.xsh");

                // 将当前目录下的 .xsh 文件添加到列表中
                xshFiles.AddRange(files);

                // 获取当前目录下的所有子目录
                string[] subdirectories = Directory.GetDirectories(directory);

                // 递归遍历子目录
                foreach (var subdirectory in subdirectories)
                {
                    // 递归获取子目录下的 .xsh 文件
                    List<string> subdirectoryXshFiles = GetXshFiles(subdirectory);

                    // 将子目录下的 .xsh 文件添加到列表中
                    xshFiles.AddRange(subdirectoryXshFiles);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
            }

            return xshFiles;
        }

        public static string Decrypt(string filePath)
        {

            return "";
        }

    }

    class ARC4
    {
        private readonly byte[] s;
        private byte i;
        private byte j;

        public ARC4(byte[] key)
        {
            s = new byte[256];
            for (int k = 0; k < 256; k++)
            {
                s[k] = (byte)k;
            }

            int j = 0;
            for (int k = 0; k < 256; k++)
            {
                j = (j + s[k] + key[k % key.Length]) % 256;
                (s[k], s[j]) = (s[j], s[k]);
            }
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];
            for (int k = 0; k < inputCount; k++)
            {
                i = (byte)((i + 1) % 256);
                j = (byte)((j + s[i]) % 256);
                (s[i], s[j]) = (s[j], s[i]);
                outputBuffer[k] = (byte)(inputBuffer[k] ^ s[(s[i] + s[j]) % 256]);
            }

            return outputBuffer;
        }
    }
    class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public IniFile(string filePath, Encoding encoding)
        {
            string[] lines = File.ReadAllLines(filePath, encoding);

            Dictionary<string, string> currentSection = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";"))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentSection = GetOrCreateSection(sectionName);
                }
                else if (currentSection != null)
                {
                    int separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex >= 0)
                    {
                        string key = trimmedLine.Substring(0, separatorIndex).Trim();
                        string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                        currentSection[key] = value;
                    }
                }
            }
        }

        public string this[string section, string key]
        {
            get
            {
                if (sections.TryGetValue(section, out var values) && values.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }
        }

        private Dictionary<string, string> GetOrCreateSection(string section)
        {
            if (!sections.TryGetValue(section, out var values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[section] = values;
            }
            return values;
        }
    }
}

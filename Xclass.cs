using System.Security.Principal;


namespace Xpass
{
    public struct Xsh
    {
        public string host;
        public string userName;
        public string password;
        public string encryptPw;
        public string port;
    }

    class Xclass
    {

        public static Xsh FileParser(string xshPath)
        {

            Xsh xsh = new();

            using (StreamReader sr = new StreamReader(xshPath))
            {
                string? rawPass;
                while ((rawPass = sr.ReadLine()) != null)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"Host=(.*?)"))
                    {
                        xsh.host = rawPass.Replace("Host=", "");
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"^Port=(.*?)"))
                    {
                        xsh.port = rawPass.Replace("Port=", "");

                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"Password=(.*?)"))
                    {
                        rawPass = rawPass.Replace("Password=", "");
                        rawPass = rawPass.Replace("\r\n", "");
                        xsh.encryptPw = rawPass;
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(rawPass, @"UserName=(.*?)"))
                    {
                        xsh.userName = rawPass.Replace("UserName=", "");
                    }
                }
            }
            return xsh;
        }

        public static string GetSid()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();

            if (currentIdentity != null)
            {
                string userName = currentIdentity.Name;

                try
                {
                    string sid = new SecurityIdentifier(currentIdentity.User?.Value ?? "").ToString();
                    return $"{userName}{sid}";
                }
                catch (Exception ex)
                {
                    return $"{userName} Error retrieving SID: {ex.Message}";
                }
            }

            return "Unable to retrieve current Windows identity.";
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
}

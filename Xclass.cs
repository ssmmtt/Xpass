using System.Collections;
using System.Text;

namespace Xpass
{
    public struct Xsh
    {
        public string host;
        public string userName;
        public string password;
        public string encryptPw;
        public string port;
        public bool isok;
    }

    class Xclass
    {
        public static Xsh FileParser(string xshPath, string sid)
        {
            Xsh xsh = new()
            {
                isok = true
            };

            using (StreamReader sr = new(xshPath))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, @"Host=(.*?)"))
                    {
                        xsh.host = line.Replace("Host=", "");
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^Port=(.*?)"))
                    {
                        xsh.port = line.Replace("Port=", "");
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"Password=(.*?)"))
                    {
                        line = line.Replace("Password=", "");
                        line = line.Replace("\r\n", "");
                        xsh.encryptPw = line;
                        if (line != null && line != "")
                        {
                            var password = Decrypt(sid, line);
                            if (password == null)
                            {
                                xsh.isok = false;
                            }
                            else
                            {
                                xsh.password = password;
                            }
                        }
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"UserName=(.*?)"))
                    {
                        xsh.userName = line.Replace("UserName=", "");
                    }
                }
            }
            return xsh;
        }

        public static string GetSid()
        {
            try
            {
                string userName = Environment.UserName;
                string domainName = Environment.UserDomainName;
                var ntAccount = new System.Security.Principal.NTAccount(domainName, userName);
                var sid = (System.Security.Principal.SecurityIdentifier)ntAccount.Translate(typeof(System.Security.Principal.SecurityIdentifier));
                string sidString = sid.ToString();
                string reversedString = ReverseString(sidString);
                return reversedString + userName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<string> GetXshFiles(string directory)
        {
            List<string> xshFiles = [];

            try
            {
                // 获取当前目录下的所有 .xsh 文件
                xshFiles.AddRange(Directory.GetFiles(directory, "*.xsh"));

                // 获取当前目录下的所有子目录
                string[] subdirectories = Directory.GetDirectories(directory);

                // 递归遍历子目录并并行化处理
                Parallel.ForEach(subdirectories, subdirectory =>
                {
                    try
                    {
                        // 获取子目录下的 .xsh 文件
                        List<string> subdirectoryXshFiles = GetXshFiles(subdirectory);

                        // 将子目录下的 .xsh 文件添加到列表中
                        lock (xshFiles) // 使用锁确保线程安全
                        {
                            xshFiles.AddRange(subdirectoryXshFiles);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理子目录的异常并继续其他目录
                        Console.WriteLine($"子目录 {subdirectory} 发生异常: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"目录 {directory} 发生异常: {ex.Message}");
            }

            return xshFiles;
        }

        public static string? Decrypt(string sid, string encryptPw)
        {
            byte[] v1 = Convert.FromBase64String(encryptPw);
            byte[] key = System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(sid));

            using var rc4 = new RC4(key);
            byte[] v3 = rc4.TransformFinalBlock(v1, 0, v1.Length - 0x20);
            byte[] expectedHash = new byte[32];
            Array.Copy(v1, v1.Length - 32, expectedHash, 0, 32);

            if (StructuralComparisons.StructuralEqualityComparer.Equals(System.Security.Cryptography.SHA256.Create().ComputeHash(v3), expectedHash))
            {
                return Encoding.ASCII.GetString(v3);
            }
            else
            {
                return null;
            }
        }

        static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    class RC4 : IDisposable
    {
        private readonly byte[] s;
        private int i, j;

        public RC4(byte[] key)
        {
            s = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + s[i]) & 255;
                Swap(i, j);
            }

            i = 0;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int offset, int count)
        {
            byte[] outputBuffer = new byte[count];
            TransformBlock(inputBuffer, offset, count, outputBuffer, 0);
            return outputBuffer;
        }

        public int TransformBlock(byte[] inputBuffer, int offset, int count, byte[] outputBuffer, int outputOffset)
        {
            for (int k = 0; k < count; k++)
            {
                i = (i + 1) & 255;
                j = (j + s[i]) & 255;
                Swap(i, j);
                outputBuffer[k + outputOffset] = (byte)(inputBuffer[k + offset] ^ s[(s[i] + s[j]) & 255]);
            }

            return count;
        }

        private void Swap(int i, int j)
        {
            (s[j], s[i]) = (s[i], s[j]);
        }

        public void Dispose()
        {
            Array.Clear(s, 0, s.Length);
            i = j = 0;
        }
    }
}

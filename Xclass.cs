using System.Collections;
using System.Runtime.InteropServices;
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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ConvertSidToStringSid(IntPtr sid, out IntPtr stringSid);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            IntPtr Sid,
            ref int cbSid,
            StringBuilder? ReferencedDomainName,
            ref int cchReferencedDomainName,
            out int peUse
        );

        public static Xsh FileParser(string xshPath, string sid)
        {

            Xsh xsh = new();
            xsh.isok = true;

            using (StreamReader sr = new StreamReader(xshPath))
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
            string userName = Environment.UserName;
            string computerName = Environment.MachineName;

            IntPtr sid = IntPtr.Zero;
            int cbSid = 0;
            int cchReferencedDomainName = 0;
            int peUse;

            LookupAccountName(computerName, userName, IntPtr.Zero, ref cbSid, null, ref cchReferencedDomainName, out peUse);

            sid = Marshal.AllocHGlobal(cbSid);
            StringBuilder referencedDomainName = new StringBuilder(cchReferencedDomainName);

            if (LookupAccountName(computerName, userName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out peUse))
            {
                IntPtr stringSid;

                if (ConvertSidToStringSid(sid, out stringSid))
                {
                    string key = ReverseString(Marshal.PtrToStringAuto(stringSid)) + userName;
                    // Free allocated memory
                    Marshal.FreeHGlobal(sid);
                    Marshal.FreeHGlobal(stringSid);
                    return key;
                }
            }
            return "";
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

        public static string? Decrypt(string sid, string encryptPw)
        {

            byte[] v1 = Convert.FromBase64String(encryptPw);
            byte[] key = System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(sid));

            using (var rc4 = new RC4(key))
            {
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

            i = j = 0;
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
            byte temp = s[i];
            s[i] = s[j];
            s[j] = temp;
        }

        public void Dispose()
        {
            Array.Clear(s, 0, s.Length);
            i = j = 0;
        }
    }
}

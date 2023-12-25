using Microsoft.Win32;

namespace Xpass
{
    internal class RegistryCache
    {
        public static void WriteToRegistry(string key, string valueName, string value)
        {
            try
            {
                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(key))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue(valueName, value);
                        Console.WriteLine("Write to registry successful.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to registry: " + ex.Message);
            }
        }

        public static string? ReadFromRegistry(string key, string valueName)
        {
            try
            {
                using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(key);
                if (registryKey != null)
                {
                    return registryKey.GetValue(valueName)?.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading from registry: " + ex.Message);
            }

            return null;
        }
    }
}

using Microsoft.Win32;
using System;

namespace Xpass
{
    internal class RegistryCache
    {
        /// <summary>
        /// 向注册表写入值
        /// </summary>
        /// <param name="key">注册表键路径</param>
        /// <param name="valueName">值名称</param>
        /// <param name="value">要存储的值</param>
        /// <returns>是否成功写入</returns>
        public static bool WriteToRegistry(string key, string valueName, string value)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(key, writable: true);
                if (registryKey == null)
                {
                    Console.WriteLine($"[Error] Failed to create or open registry key: {key}");
                    return false;
                }

                registryKey.SetValue(valueName, value);
                Console.WriteLine($"[Success] Written to registry: {key}\\{valueName} = {value}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Error writing to registry: {ex}");
                return false;
            }
        }

        /// <summary>
        /// 从注册表读取值
        /// </summary>
        /// <param name="key">注册表键路径</param>
        /// <param name="valueName">值名称</param>
        /// <returns>读取的值（如果存在），否则为 null</returns>
        public static string? ReadFromRegistry(string key, string valueName)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(key, writable: false);
                if (registryKey == null)
                {
                    Console.WriteLine($"[Warning] Registry key not found: {key}");
                    return null;
                }

                var value = registryKey.GetValue(valueName)?.ToString();
                Console.WriteLine($"[Info] Read from registry: {key}\\{valueName} = {value}");
                return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Error reading from registry: {ex}");
                return null;
            }
        }

        /// <summary>
        /// 删除注册表中的值（用于缓存失效后清空）。
        /// </summary>
        public static bool DeleteFromRegistry(string key, string valueName)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(key, writable: true);
                if (registryKey == null)
                {
                    return true;
                }

                registryKey.DeleteValue(valueName, throwOnMissingValue: false);
                Console.WriteLine($"[Success] Deleted registry value: {key}\\{valueName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Error deleting registry value: {ex}");
                return false;
            }
        }
    }
}

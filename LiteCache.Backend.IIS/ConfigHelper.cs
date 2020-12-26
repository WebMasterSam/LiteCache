using System;
using Microsoft.Extensions.Configuration;

namespace LiteCache.Backend.Helpers
{
    public static class ConfigHelper
    {
        private static IConfiguration _config;

        public static void SetConfigurationInstance(IConfiguration config)
        {
            _config = config;
        }

        public static T GetAppSetting<T>(string jsonPath)
        {
            return _config.GetValue<T>(jsonPath.Replace(".", ":"));
        }

        public static string GetAppSetting(string jsonPath)
        {
            return _config.GetValue<string>(jsonPath.Replace(".", ":"));
        }

        public static bool GetAppSettingBoolean(string jsonPath)
        {
            return _config.GetValue<bool>(jsonPath.Replace(".", ":"));
        }

        public static decimal GetAppSettingDecimal(string jsonPath)
        {
            return _config.GetValue<decimal>(jsonPath.Replace(".", ":"));
        }

        public static int GetAppSettingInt(string jsonPath)
        {
            return _config.GetValue<int>(jsonPath.Replace(".", ":"));
        }

        public static Guid GetAppSettingGuid(string jsonPath)
        {
            var guid = _config.GetValue<string>(jsonPath.Replace(".", ":"));

            return new Guid(guid);
        }
    }
}

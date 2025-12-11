using System.Configuration;

namespace DataEditorX.Common
{
    public class XMLReader
    {
        /// <summary>
        /// 保存值
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="appValue"></param>
        public static void Save(string appKey, string appValue)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = config.AppSettings.Settings;
                if (settings[appKey] == null)
                {
                    settings.Add(appKey, appValue);
                }
                else
                {
                    settings[appKey].Value = appValue;
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {
                // ignore failures to save configuration
            }
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="appKey"></param>
        /// <returns></returns>
        public static string GetAppConfig(string appKey)
        {
            try
            {
                string val = ConfigurationManager.AppSettings[appKey];
                return val ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

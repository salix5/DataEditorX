/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 2014-10-26
 * 时间: 10:26
 * 
 */
using System.Windows.Forms;

namespace System.IO
{
    /// <summary>
    /// 路径处理
    /// </summary>
    public class MyPath
    {
        /// <summary>
        /// 从相对路径获取真实路径
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string GetRealPath(string dir)
        {
            string path = Application.StartupPath;
            if (dir.StartsWith("./"))
            {
                dir = Combine(path, dir.Substring(2));
            }
            return dir;
        }
        /// <summary>
        /// 合并路径
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }
        /// <summary>
        /// 检查目录是否合法
        /// </summary>
        /// <param name="dir">目录</param>
        /// <param name="fallbackPath">不合法时，采取的目录</param>
        /// <returns></returns>
        public static string CheckDir(string dir, string fallbackPath)
        {
            try
            {
                DirectoryInfo info = Directory.CreateDirectory(GetRealPath(dir));
                return info.FullName;
            }
            catch
            {
                DirectoryInfo fallbackInfo = Directory.CreateDirectory(fallbackPath);
                return fallbackInfo.FullName;
            }
        }
        /// <summary>
        /// 根据tag获取文件名
        /// tag_lang.txt
        /// </summary>
        /// <param name="tag">前面</param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetFileName(string tag, string lang)
        {
            return $"{tag}_{lang}.txt";
        }
        /// <summary>
        /// 由tag和lang获取文件名
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFullFileName(string tag, string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (!name.StartsWith(tag + "_"))
            {
                return "";
            }
            else
            {
                return name.Replace(tag + "_", "");
            }
        }

        public static void CreateDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        public static void CreateDirByFile(string file)
        {
            string dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}

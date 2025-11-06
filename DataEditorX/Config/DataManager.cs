/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 5月18 星期日
 * 时间: 18:08
 * 
 */
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataEditorX.Config
{
    public class DataManager
    {
        /// <summary>
        /// 内容开头
        /// </summary>
        public const string TAG_START = "##";
        /// <summary>
        /// 内容结尾
        /// </summary>
        public const string TAG_END = "#";
        /// <summary>
        /// 行分隔符
        /// </summary>
        public const char LINE_SEPARATOR = '\t';

        #region 根据tag获取内容
        public static string SubString(string content, string tag)
        {
            Regex reg = new Regex(string.Format(@"{0}{1}\n([\S\s]*?)\n{2}", TAG_START, tag, TAG_END), RegexOptions.Multiline);
            Match mac = reg.Match(content);
            if (mac.Success)//把相应的内容提取出来
            {
                return mac.Groups[1].Value;
            }
            return "";
        }
        #endregion

        #region 读取
        /// <summary>
        /// 从字符串中，按tag来分割内容，并读取内容
        /// </summary>
        /// <param name="content">字符串</param>
        /// <param name="tag">开始的标志</param>
        /// <returns></returns>
        public static Dictionary<long, string> Read(string content, string tag)
        {
            return LinesToDictionary(SubString(content, tag).Split('\n'));
        }
        /// <summary>
        /// 从行读取内容
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static Dictionary<long, string> LinesToDictionary(string[] lines)
        {
            Dictionary<long, string> result = new Dictionary<long, string>();
            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] words = line.Split(LINE_SEPARATOR);
                if (words.Length < 2)
                {
                    continue;
                }
                if (words[1] == "N/A")
                {
                    continue;
                }

                long lkey;
                if (words[0].StartsWith("0x"))
                {
                    long.TryParse(words[0].Replace("0x", ""), NumberStyles.HexNumber, null, out lkey);
                }
                else
                {
                    long.TryParse(words[0], out lkey);
                }
                if (result.ContainsKey(lkey))
                {
                    continue;
                }
                result.Add(lkey, words[1]);
            }
            return result;
        }

        #endregion

        #region 查找
        public static List<long> GetKeys(Dictionary<long, string> dic)
        {
            List<long> list = new List<long>();
            foreach (long l in dic.Keys)
            {
                list.Add(l);
            }
            return list;
        }
        public static string[] GetValues(Dictionary<long, string> dic)
        {
            List<string> list = new List<string>();
            foreach (long l in dic.Keys)
            {
                list.Add(dic[l]);
            }
            return list.ToArray();
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(Dictionary<long, string> dic, long key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key].Trim();
            }

            return key.ToString("x");
        }
        #endregion
    }
}

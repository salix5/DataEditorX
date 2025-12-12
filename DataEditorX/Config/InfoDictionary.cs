using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataEditorX.Config
{
    public class InfoItem
    {
        public long Key { get; set; }
        public string Value { get; set; }
    }
    public class InfoDictionary : OrderedDictionary
    {
        /// <summary>
        /// 内容开头
        /// </summary>
        const string TAG_START = "##";
        /// <summary>
        /// 内容结尾
        /// </summary>
        const string TAG_END = "#";
        /// <summary>
        /// 行分隔符
        /// </summary>
        const char LINE_SEPARATOR = '\t';

        #region 根据tag获取内容
        static string SubString(string content, string tag)
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

        /// <summary>
        /// 从字符串中，按tag来分割内容，并读取内容
        /// </summary>
        /// <param name="content">字符串</param>
        /// <param name="tag">开始的标志</param>
        /// <returns></returns>
        public void Initialize(string content, string tag)
        {
            Initialize(SubString(content, tag).Split('\n'));
        }

        /// <summary>
        /// 从行读取内容
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public void Initialize(string[] lines)
        {
            Clear();
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

                long key;
                if (words[0].StartsWith("0x"))
                {
                    long.TryParse(words[0].Replace("0x", ""), NumberStyles.HexNumber, null, out key);
                }
                else
                {
                    long.TryParse(words[0], out key);
                }
                Add(key, words[1]);
            }
        }

        public InfoItem[] GetItems()
        {
            InfoItem[] items = new InfoItem[Count];
            int index = 0;
            foreach (DictionaryEntry entry in this)
            {
                items[index] = new InfoItem { Key = (long)entry.Key, Value = (string)entry.Value };
                index++;
            }
            return items;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(long key)
        {
            if (Contains(key))
            {
                return ((string)this[key]).Trim();
            }

            return key.ToString("x");
        }
    }
}

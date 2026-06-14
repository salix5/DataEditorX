/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 7月8 星期二
 * 时间: 9:52
 * 
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DataEditorX.Language
{
    /// <summary>
    /// Description of Language.
    /// </summary>
    public class LanguageHelper
    {
        static readonly Dictionary<string, string> _gWordsList = new();
        static readonly Dictionary<LMSG, string> _gMsgList = new();
        const char SEP_CONTROL = '.';
        const char SEP_LINE = '\t';
        const string STR_COMMENT = "#";
        readonly Dictionary<string, string> mWordslist = new();

        #region 获取消息文字
        public static string GetMsg(LMSG key)
        {
            if (!_gMsgList.TryGetValue(key, out string value))
            {
                return key.ToString();
            }
            return value;
        }
        #endregion

        #region 设置控件信息
        /// <summary>
        /// 设置控件文字
        /// </summary>
        /// <param name="fm"></param>
        public static void SetFormLabel(Form fm)
        {
            if (fm == null)
            {
                return;
            }
            fm.SuspendLayout();
            SetControlLabel(fm, "", fm.Name);
            fm.ResumeLayout();
        }

        static bool GetLabel(string key, out string value)
        {
            return _gWordsList.TryGetValue(key, out value);
        }

        static void SetControlLabel(Control c, string pName, string formName)
        {
            string fullPath = pName;
            if (!string.IsNullOrEmpty(c.Name))
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = c.Name;
                }
                else
                {
                    fullPath += $"{SEP_CONTROL}{c.Name}";
                }
            }
            if (c is ListView lv)
            {
                int i, count = lv.Columns.Count;
                for (i = 0; i < count; i++)
                {
                    ColumnHeader ch = lv.Columns[i];
                    if (GetLabel($"{fullPath}{SEP_CONTROL}{i}", out string title))
                    {
                        ch.Text = title;
                    }
                }
            }
            else if (c is ToolStrip ms)
            {
                foreach (ToolStripItem tsi in ms.Items)
                {
                    SetMenuItem($"{formName}{SEP_CONTROL}{ms.Name}", tsi);
                }
            }
            else
            {
                if (GetLabel(fullPath, out string title))
                {
                    c.Text = title;
                }
            }

            foreach (Control sc in c.Controls)
            {
                SetControlLabel(sc, fullPath, formName);
            }
            ContextMenuStrip conms = c.ContextMenuStrip;
            if (conms != null)
            {
                foreach (ToolStripItem ts in conms.Items)
                {
                    SetMenuItem($"{formName}{SEP_CONTROL}{conms.Name}", ts);
                }
            }
        }

        static void SetMenuItem(string pName, ToolStripItem tsi)
        {
            string title;

            if (tsi is ToolStripMenuItem tsmi)
            {
                if (GetLabel($"{pName}{SEP_CONTROL}{tsmi.Name}", out title))
                {
                    tsmi.Text = title;
                }

                if (tsmi.HasDropDownItems)
                {
                    foreach (ToolStripItem subtsi in tsmi.DropDownItems)
                    {
                        SetMenuItem(pName, subtsi);
                    }
                }
            }
            else if (tsi is ToolStripLabel tlbl)
            {
                if (GetLabel($"{pName}{SEP_CONTROL}{tlbl.Name}", out title))
                {
                    tlbl.Text = title;
                }
            }
        }

        #endregion

        #region 获取控件信息
        public void GetFormLabel(Form fm)
        {
            if (fm == null)
            {
                return;
            }
            GetControlLabel(fm, "", fm.Name);
        }

        void AddLabel(string key, string value)
        {
            if (!mWordslist.ContainsKey(key))
            {
                mWordslist.Add(key, value);
            }
        }

        void GetControlLabel(Control c, string pName, string formName)
        {
            string fullPath = pName;
            if (!string.IsNullOrEmpty(c.Name))
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = c.Name;
                }
                else
                {
                    fullPath += $"{SEP_CONTROL}{c.Name}";
                }
            }

            if (c is ListView lv)
            {
                int i, count = lv.Columns.Count;
                for (i = 0; i < count; i++)
                {
                    AddLabel($"{fullPath}{SEP_CONTROL}{i}", lv.Columns[i].Text);
                }
            }
            else if (c is ToolStrip ms)
            {
                foreach (ToolStripItem tsi in ms.Items)
                {
                    GetMenuItem($"{formName}{SEP_CONTROL}{ms.Name}", tsi);
                }
            }
            else if (!string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Text))
            {
                AddLabel(fullPath, c.Text);
            }

            foreach (Control sc in c.Controls)
            {
                GetControlLabel(sc, fullPath, formName);
            }
            ContextMenuStrip conms = c.ContextMenuStrip;
            if (conms != null)
            {
                foreach (ToolStripItem ts in conms.Items)
                {
                    GetMenuItem($"{formName}{SEP_CONTROL}{conms.Name}", ts);
                }
            }
        }

        void GetMenuItem(string pName, ToolStripItem tsi)
        {
            if (string.IsNullOrEmpty(tsi.Name))
            {
                return;
            }

            if (tsi is ToolStripMenuItem tsmi)
            {
                AddLabel($"{pName}{SEP_CONTROL}{tsmi.Name}", tsmi.Text);
                if (tsmi.HasDropDownItems)
                {
                    foreach (ToolStripItem subtsi in tsmi.DropDownItems)
                    {
                        GetMenuItem(pName, subtsi);
                    }
                }
            }
            else if (tsi is ToolStripLabel tlbl)
            {
                AddLabel($"{pName}{SEP_CONTROL}{tlbl.Name}", tlbl.Text);
            }
        }

        #endregion

        #region 保存语言文件
        public bool SaveLanguage(string conf)
        {
            using FileStream fs = new(conf, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new(fs, Encoding.UTF8);
            foreach (string k in mWordslist.Keys)
            {
                sw.WriteLine(k + SEP_LINE + mWordslist[k]);
            }
            sw.WriteLine("#");
            foreach (LMSG k in _gMsgList.Keys)
            {
                //记得替换换行符
                sw.WriteLine("0x" + ((uint)k).ToString("x") + SEP_LINE + _gMsgList[k].Replace("\n", "\\n"));
            }
            foreach (LMSG k in Enum.GetValues(typeof(LMSG)))
            {
                if (!_gMsgList.ContainsKey(k))
                {
                    sw.WriteLine("0x" + ((uint)k).ToString("x") + SEP_LINE + k.ToString());
                }
            }
            sw.Close();
            fs.Close();
            return true;
        }
        #endregion

        #region 加载语言文件
        public static void LoadFormLabels(string f)
        {
            if (!File.Exists(f))
            {
                return;
            }

            _gWordsList.Clear();
            _gMsgList.Clear();
            using FileStream fs = new(f, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new(fs, Encoding.UTF8);
            string line;
            LMSG ltemp;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith(STR_COMMENT))
                {
                    continue;
                }

                int sepIndex = line.IndexOf(SEP_LINE);
                if (sepIndex < 0)
                {
                    continue;
                }
                string key = line.Substring(0, sepIndex);
                string value = line.Substring(sepIndex + 1);

                if (line.StartsWith("0x"))//加载消息文字
                {
                    uint.TryParse(key.Substring(2), NumberStyles.HexNumber, null, out uint utemp);
                    ltemp = (LMSG)utemp;
                    if (!_gMsgList.ContainsKey(ltemp))
                    {
                        _gMsgList.Add(ltemp, value.Replace("\\n", "\n"));
                    }
                }
                else //加载界面语言
                {
                    if (!_gWordsList.ContainsKey(key))
                    {
                        _gWordsList.Add(key, value);
                    }
                }
            }

        }
        #endregion
    }

}

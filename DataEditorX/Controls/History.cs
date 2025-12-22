using DataEditorX.Config;
using DataEditorX.Core;
using DataEditorX.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DataEditorX.Controls
{

    public class History
    {
        readonly IMainForm mainForm;
        string historyFile = "";
        readonly List<string> cdbHistory = new();
        readonly List<string> luaHistory = new();
        public string[] GetCdbHistory()
        {
            return cdbHistory.ToArray();
        }
        public string[] GetLuaHistory()
        {
            return luaHistory.ToArray();
        }
        public History(IMainForm mainForm)
        {
            this.mainForm = mainForm;
        }
        //读取历史记录
        public void ReadHistory(string historyFile)
        {
            this.historyFile = historyFile;
            if (!File.Exists(historyFile))
            {
                return;
            }

            string[] lines = File.ReadAllLines(historyFile);
            AddHistorys(lines);
        }
        //添加历史记录
        void AddHistorys(string[] lines)
        {
            luaHistory.Clear();
            cdbHistory.Clear();
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                if (File.Exists(line))
                {
                    if (YGOUtil.IsDatabase(line))
                    {
                        if (cdbHistory.Count < MyConfig.MAX_HISTORY)
                        {
                            cdbHistory.Add(line);
                        }
                    }
                    else
                    {
                        if (luaHistory.Count < MyConfig.MAX_HISTORY)
                        {
                            luaHistory.Add(line);
                        }
                    }
                }
            }
        }
        public void AddHistory(string file)
        {
            List<string> tmplist = new()
            {
                //添加到开始
                file
            };
            //添加旧记录
            tmplist.AddRange(cdbHistory.ToArray());
            tmplist.AddRange(luaHistory.ToArray());
            //
            AddHistorys(tmplist.ToArray());
            SaveHistory();
            MenuHistory();
        }
        //保存历史
        void SaveHistory()
        {
            string texts = "# database history";
            foreach (string str in cdbHistory)
            {
                if (File.Exists(str))
                {
                    texts += Environment.NewLine + str;
                }
            }
            texts += Environment.NewLine + "# script history";
            foreach (string str in luaHistory)
            {
                if (File.Exists(str))
                {
                    texts += Environment.NewLine + str;
                }
            }
            if (File.Exists(historyFile))
            {
                File.Delete(historyFile);
            }

            File.WriteAllText(historyFile, texts);
        }
        //添加历史记录菜单
        public void MenuHistory()
        {
            //cdb历史
            mainForm.CdbMenuClear();
            foreach (string str in cdbHistory)
            {
                ToolStripMenuItem tsmi = new(str);
                tsmi.Click += MenuHistoryItem_Click;
                mainForm.AddCdbMenu(tsmi);
            }
            mainForm.AddCdbMenu(new ToolStripSeparator());
            ToolStripMenuItem tsmiclear = new(LanguageHelper.GetMsg(LMSG.ClearHistory));
            tsmiclear.Click += MenuHistoryClear_Click;
            mainForm.AddCdbMenu(tsmiclear);
            //lua历史
            mainForm.LuaMenuClear();
            foreach (string str in luaHistory)
            {
                ToolStripMenuItem tsmi = new(str);
                tsmi.Click += MenuHistoryItem_Click;
                mainForm.AddLuaMenu(tsmi);
            }
            mainForm.AddLuaMenu(new ToolStripSeparator());
            ToolStripMenuItem tsmiclear2 = new(LanguageHelper.GetMsg(LMSG.ClearHistory));
            tsmiclear2.Click += MenuHistoryClear2_Click;
            mainForm.AddLuaMenu(tsmiclear2);
        }

        void MenuHistoryClear2_Click(object sender, EventArgs e)
        {
            luaHistory.Clear();
            MenuHistory();
            SaveHistory();
        }
        void MenuHistoryClear_Click(object sender, EventArgs e)
        {
            cdbHistory.Clear();
            MenuHistory();
            SaveHistory();
        }
        void MenuHistoryItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                string file = tsmi.Text;
                if (File.Exists(file))
                {
                    mainForm.Open(file);
                }
            }
        }
    }
}

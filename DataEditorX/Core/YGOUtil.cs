using DataEditorX.Config;
using DataEditorX.Core.Info;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataEditorX.Core
{
    static class YGOUtil
    {
        static DataConfig _datacfg;
        static YGOUtil()
        {
            _datacfg = new DataConfig();
        }
        public static void SetConfig(DataConfig dcfg)
        {
            _datacfg = dcfg;
        }

        #region 判断文件类型
        public static bool IsScript(string file)
        {
            return file?.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        public static bool IsDataBase(string file)
        {
            return file?.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        #endregion

        #region 获取属性，种族
        public static string GetAttributeString(long attr)
        {
            return _datacfg.dicCardAttributes.GetValue(attr);
        }


        public static string GetRace(long race)
        {
            return _datacfg.dicCardRaces.GetValue(race);
        }
        #endregion

        #region 获取卡片类型
        public static string GetCardType(Card c)
        {
            string str = "???";
            if (c.IsType(CardType.TYPE_MONSTER))
            {//卡片类型和第1效果
                if (c.IsType(CardType.TYPE_XYZ))
                {
                    str = GetType(CardType.TYPE_XYZ);
                }
                else if (c.IsType(CardType.TYPE_TOKEN))
                {
                    str = GetType(CardType.TYPE_TOKEN);
                }
                else if (c.IsType(CardType.TYPE_RITUAL))
                {
                    str = GetType(CardType.TYPE_RITUAL);
                }
                else if (c.IsType(CardType.TYPE_FUSION))
                {
                    str = GetType(CardType.TYPE_FUSION);
                }
                else if (c.IsType(CardType.TYPE_SYNCHRO))
                {
                    str = GetType(CardType.TYPE_SYNCHRO);
                }
                else if (c.IsType(CardType.TYPE_EFFECT))
                {
                    str = GetType(CardType.TYPE_EFFECT);
                }
                else
                {
                    str = GetType(CardType.TYPE_NORMAL);
                }

                str += GetType(CardType.TYPE_MONSTER);
            }
            else if (c.IsType(CardType.TYPE_SPELL))
            {
                if (c.IsType(CardType.TYPE_EQUIP))
                {
                    str = GetType(CardType.TYPE_EQUIP);
                }
                else if (c.IsType(CardType.TYPE_QUICKPLAY))
                {
                    str = GetType(CardType.TYPE_QUICKPLAY);
                }
                else if (c.IsType(CardType.TYPE_FIELD))
                {
                    str = GetType(CardType.TYPE_FIELD);
                }
                else if (c.IsType(CardType.TYPE_CONTINUOUS))
                {
                    str = GetType(CardType.TYPE_CONTINUOUS);
                }
                else if (c.IsType(CardType.TYPE_RITUAL))
                {
                    str = GetType(CardType.TYPE_RITUAL);
                }
                else
                {
                    str = GetType(CardType.TYPE_NORMAL);
                }

                str += GetType(CardType.TYPE_SPELL);
            }
            else if (c.IsType(CardType.TYPE_TRAP))
            {
                if (c.IsType(CardType.TYPE_CONTINUOUS))
                {
                    str = GetType(CardType.TYPE_CONTINUOUS);
                }
                else if (c.IsType(CardType.TYPE_COUNTER))
                {
                    str = GetType(CardType.TYPE_COUNTER);
                }
                else
                {
                    str = GetType(CardType.TYPE_NORMAL);
                }

                str += GetType(CardType.TYPE_TRAP);
            }
            return str.Replace(" ", "");
        }

        static string GetType(CardType type)
        {
            return _datacfg.dicCardTypes.GetValue((long)type);
        }

        public static string GetTypeString(long type)
        {
            string str = "";
            foreach (long k in _datacfg.dicCardTypes.Keys)
            {
                if ((type & k) == k)
                {
                    str += GetType((CardType)k) + "|";
                }
            }
            if (str.Length > 0)
            {
                str = str.Substring(0, str.Length - 1);
            }
            else
            {
                str = "???";
            }

            return str;
        }
        #endregion

        #region 系列名
        public static string GetSetNameString(long setcode)
        {
            long sc1 = setcode & 0xffff;
            long sc2 = (setcode >> 16) & 0xffff;
            long sc3 = (setcode >> 32) & 0xffff;
            long sc4 = (setcode >> 48) & 0xffff;
            string setname = _datacfg.dicSetnames.GetValue(sc1)
                    + " " + _datacfg.dicSetnames.GetValue(sc2)
                    + " " + _datacfg.dicSetnames.GetValue(sc3)
                    + " " + _datacfg.dicSetnames.GetValue(sc4);

            return setname;
        }
        #endregion

        #region 根据文件读取数据库
        /// <summary>
        /// 读取ydk文件为密码数组
        /// </summary>
        /// <param name="file">ydk文件</param>
        /// <returns>密码数组</returns>
        public static string[] ReadYDK(string ydkfile)
        {
            HashSet<string> IDs = new HashSet<string>();
            if (File.Exists(ydkfile))
            {
                using (FileStream f = new FileStream(ydkfile, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(f, Encoding.UTF8)) 
                    {
                        string str;
                        while ((str = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(str))
                            {
                                continue;
                            }
                            if (str.StartsWith("!") || str.StartsWith("#"))
                            {
                                continue;
                            }
                            IDs.Add(str);
                        }
                    }
                }
            }
            if (IDs.Count == 0)
            {
                return null;
            }

            return IDs.ToArray();
        }
        #endregion

        #region 图像
        public static string[] ReadImage(string path)
        {
            List<string> list = new List<string>();
            string[] files = Directory.GetFiles(path, "*.*");
            int n = files.Length;
            for (int i = 0; i < n; i++)
            {
                string ex = Path.GetExtension(files[i]).ToLower();
                if (ex == ".jpg" || ex == ".png" || ex == ".bmp")
                {
                    list.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            return list.ToArray();
        }
        #endregion

        #region 删除资源
        //删除资源
        public static void CardDelete(long id, YgoPath ygopath)
        {
            string[] files = ygopath.GetCardfiles(id);
            for (int i = 0; i < files.Length; i++)
            {
                if (FileSystem.FileExists(files[i]))
                {
                    FileSystem.DeleteFile(files[i], UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
        }
        #endregion

        #region 资源改名
        //资源改名
        public static void CardRename(long newid, long oldid, YgoPath ygopath)
        {
            string[] newfiles = ygopath.GetCardfiles(newid);
            string[] oldfiles = ygopath.GetCardfiles(oldid);

            for (int i = 0; i < oldfiles.Length; i++)
            {
                if (File.Exists(oldfiles[i]))
                {
                    try
                    {
                        File.Move(oldfiles[i], newfiles[i]);
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region 复制资源
        public static void CardCopy(long newid, long oldid, YgoPath ygopath)
        {
            string[] newfiles = ygopath.GetCardfiles(newid);
            string[] oldfiles = ygopath.GetCardfiles(oldid);

            for (int i = 0; i < oldfiles.Length; i++)
            {
                if (File.Exists(oldfiles[i]))
                {
                    try
                    {
                        File.Copy(oldfiles[i], newfiles[i], false);
                    }
                    catch { }
                }
            }
        }
        #endregion
    }
}

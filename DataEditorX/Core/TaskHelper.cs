/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 2014-10-12
 * 时间: 19:43
 * 
 */
using DataEditorX.Common;
using DataEditorX.Config;
using DataEditorX.Core.Info;
using DataEditorX.Core.Mse;
using DataEditorX.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace DataEditorX.Core
{
    /// <summary>
    /// 任务
    /// </summary>
    public class TaskHelper
    {
        #region Member
        /// <summary>
        /// 当前任务
        /// </summary>
        private MyTask nowTask = MyTask.NONE;
        /// <summary>
        /// 上一次任务
        /// </summary>
        private MyTask lastTask = MyTask.NONE;

        /// <summary>
        /// 当前卡片列表
        /// </summary>
        public Card[] CardList { get; private set; } = Array.Empty<Card>();
        /// <summary>
        /// 任务参数
        /// </summary>
        private string[] mArgs = Array.Empty<string>();
        /// <summary>
        /// 图片设置
        /// </summary>
        private readonly ImageSet imgSet = new();
        /// <summary>
        /// MSE转换
        /// </summary>
        private readonly MseMaker mseHelper = new();
        /// <summary>
        /// 是否取消
        /// </summary>
        private bool isCancel = false;
        /// <summary>
        /// 是否在运行
        /// </summary>
        private bool isRun = false;
        /// <summary>
        /// 后台工作线程
        /// </summary>
        private readonly BackgroundWorker worker;

        public TaskHelper(string datapath, BackgroundWorker worker)
        {
            Datapath = datapath;
            this.worker = worker;
        }
        public MseMaker MseHelper
        {
            get { return mseHelper; }
        }
        public bool IsRuning()
        {
            return isRun;
        }
        public bool IsCancel()
        {
            return isCancel;
        }
        public void Cancel()
        {
            isRun = false;
            isCancel = true;
        }
        public MyTask GetLastTask()
        {
            return lastTask;
        }

        public void TestPendulumText(string desc)
        {
            mseHelper.TestPendulum(desc);
        }
        #endregion

        #region Other
        //设置任务
        public void SetTask(MyTask myTask, Card[] cards, params string[] args)
        {
            nowTask = myTask;
            CardList = cards;
            mArgs = args;
        }
        //转换图片
        public void ToImg(string img, string saveimg1)
        {
            if (!File.Exists(img))
            {
                return;
            }

            using Bitmap bmp = new(img);
            MyBitmap.SaveAsJPEG(MyBitmap.Zoom(bmp, imgSet.width, imgSet.height), saveimg1, imgSet.quality);
        }
        #endregion

        #region 检查更新
        public static void CheckVersion(bool showNew)
        {
            string newver = CheckUpdate.GetNewVersion(MyConfig.updateURL);
            if (newver == CheckUpdate.DEFAULT)
            {
                if (!showNew)
                {
                    return;
                }

                MyMsg.Error(LMSG.CheckUpdateFail);
                return;
            }

            if (CheckUpdate.CheckVersion(newver, Application.ProductVersion))
            {
                if (!MyMsg.Question(LMSG.HaveNewVersion))
                {
                    return;
                }

                MyUtils.OpenRepository();
            }
            else
            {
                if (!showNew)
                {
                    return;
                }

                MyMsg.Show(LMSG.NowIsNewVersion);
            }
        }
        public void OnCheckUpdate(bool showNew)
        {
            CheckVersion(showNew);
        }
        #endregion

        #region 裁剪图片
        public void CutImages(string imgpath, bool isreplace)
        {
            int count = CardList.Length;
            int i = 0;
            foreach (Card c in CardList)
            {
                if (isCancel)
                {
                    break;
                }

                i++;
                worker.ReportProgress((i / count), string.Format("{0}/{1}", i, count));
                string jpg = MyPath.Combine(imgpath, $"{c.id}.jpg");
                string savejpg = MyPath.Combine(mseHelper.ImagePath, $"{c.id}.jpg");
                if (File.Exists(jpg) && (isreplace || !File.Exists(savejpg)))
                {
                    using Bitmap bp = new(jpg);
                    Bitmap bmp;
                    if (c.IsType(CardType.TYPE_XYZ))//超量
                    {
                        bmp = MyBitmap.Cut(bp, imgSet.xyzArea);
                    }
                    else if (c.IsType(CardType.TYPE_PENDULUM))//P怪兽
                    {
                        bmp = MyBitmap.Cut(bp, imgSet.pendulumArea);
                    }
                    else//一般
                    {
                        bmp = MyBitmap.Cut(bp, imgSet.normalArea);
                    }
                    MyBitmap.SaveAsJPEG(bmp, savejpg, imgSet.quality);
                }
            }
        }
        #endregion

        #region 转换图片
        public void ConvertImages(string imgpath, string gamepath, bool isreplace)
        {
            string picspath = MyPath.Combine(gamepath, "pics");
            string[] files = Directory.GetFiles(imgpath);
            int i = 0;
            int count = files.Length;

            foreach (string file in files)
            {
                if (isCancel)
                {
                    break;
                }

                i++;
                worker.ReportProgress(i / count, string.Format("{0}/{1}", i, count));
                string ex = Path.GetExtension(file).ToLower();
                string name = Path.GetFileNameWithoutExtension(file);
                string jpg_b = MyPath.Combine(picspath, $"{name}.jpg");
                if (ex == ".jpg" || ex == ".png" || ex == ".bmp")
                {
                    using Bitmap bmp = new(file);
                    if (isreplace || !File.Exists(jpg_b))
                    {
                        MyBitmap.SaveAsJPEG(MyBitmap.Zoom(bmp, imgSet.width, imgSet.height), jpg_b, imgSet.quality);
                    }
                }
            }
        }
        #endregion

        #region MSE存档
        public string Datapath { get; }

        public void SaveMSEs(string file, Card[] cards, bool isUpdate)
        {
            if (cards is null || cards.Length == 0)
            {
                return;
            }

            string pack_db = MyPath.GetRealPath(MyConfig.ReadString("pack_db"));
            bool rarity = MyConfig.ReadBoolean("mse_auto_rarity", false);
#if DEBUG
            MessageBox.Show("db = " + pack_db + ",auto rarity=" + rarity);
#endif
            int c = cards.Length;
            //不分开，或者卡片数小于单个存档的最大值
            if (mseHelper.MaxNum == 0 || c < mseHelper.MaxNum)
            {
                SaveMSE(1, file, cards, pack_db, rarity, isUpdate);
            }
            else
            {
                int nums = c / mseHelper.MaxNum;
                if (nums * mseHelper.MaxNum < c)//计算需要分多少个存档
                {
                    nums++;
                }

                List<Card> clist = new();
                for (int i = 0; i < nums; i++)//分别生成存档
                {
                    clist.Clear();
                    for (int j = 0; j < mseHelper.MaxNum; j++)
                    {
                        int index = i * mseHelper.MaxNum + j;
                        if (index < c)
                        {
                            clist.Add(cards[index]);
                        }
                    }
                    int t = file.LastIndexOf(".mse-set");
                    string fname = (t > 0) ? file.Substring(0, t) : file;
                    fname += string.Format("_{0}.mse-set", i + 1);
                    SaveMSE(i + 1, fname, clist.ToArray(), pack_db, rarity, isUpdate);
                }
            }
        }
        public void SaveMSE(int num, string file, Card[] cards, string pack_db, bool rarity, bool isUpdate)
        {
            string setFile = file + ".txt";
            Dictionary<Card, string> images = mseHelper.WriteSet(setFile, cards, pack_db, rarity);
            if (isUpdate)//仅更新文字
            {
                return;
            }

            try
            {
                using FileStream fs = new(file, FileMode.Create, FileAccess.Write);
                using ZipArchive archive = new(fs, ZipArchiveMode.Create, false);
                // 添加文字到压缩包，内部文件名固定为 "set"
                archive.CreateEntryFromFile(setFile, "set");

                int i = 0;
                foreach (var kvp in images)
                {
                    Card c = kvp.Key;
                    string img = kvp.Value;
                    if (isCancel)
                    {
                        break;
                    }

                    i++;
                    worker.ReportProgress(i * 100 / images.Count, string.Format("{0}/{1}-{2}", i, images.Count, num));

                    // 获取需要写入的最终图片（可包含裁剪/缓存逻辑）
                    string cachePath = mseHelper.GetImageCache(img, c);
                    string entryName = Path.GetFileName(img);
                    if (File.Exists(cachePath))
                    {
                        archive.CreateEntryFromFile(cachePath, entryName);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region 导出数据
        public void ExportData(string path, string zipname, string _cdbfile, string modulescript)
        {
            Card[] cards = CardList;
            if (cards == null || cards.Length == 0)
            {
                return;
            }

            int count = cards.Length;
            YgoPath ygopath = new(path);
            string name = Path.GetFileNameWithoutExtension(zipname);
            //数据库
            string cdbfile = zipname + ".cdb";
            //说明
            string readme = MyPath.Combine(path, name + ".txt");
            //新卡ydk
            string deckydk = ygopath.GetYdk(name);
            //module scripts
            string extra_script = "";
            if (modulescript.Length > 0)
            {
                extra_script = ygopath.GetModuleScript(modulescript);
            }

            File.Delete(cdbfile);
            Database.CreateDatabase(cdbfile);
            Database.InsertCards(cdbfile, false, CardList);
            if (File.Exists(zipname))
            {
                File.Delete(zipname);
            }

            /*using (ZipStorer zips = ZipStorer.Create(zipname, ""))
            {
                zips.AddFile(cdbfile, Path.GetFileNameWithoutExtension(_cdbfile) + ".cdb", "");
                if (File.Exists(readme))
                {
                    zips.AddFile(readme, "readme_" + name + ".txt", "");
                }

                if (File.Exists(deckydk))
                {
                    zips.AddFile(deckydk, "deck/" + name + ".ydk", "");
                }

                if (modulescript.Length > 0 && File.Exists(extra_script))
                {
                    zips.AddFile(extra_script, extra_script.Replace(path, ""), "");
                }

                int i = 0;
                foreach (Card c in cards)
                {
                    i++;
                    worker.ReportProgress(i / count, string.Format("{0}/{1}", i, count));
                    string[] files = ygopath.GetCardfiles(c.id);
                    foreach (string file in files)
                    {
                        if (!string.Equals(file, extra_script) && File.Exists(file))
                        {
                            zips.AddFile(file, file.Replace(path, ""), "");
                        }
                    }
                }
            }*/
            File.Delete(cdbfile);
        }
        #endregion

        #region 运行
        public void Run()
        {
            isCancel = false;
            isRun = true;
            switch (nowTask)
            {
                case MyTask.ExportData:
                    if (mArgs.Length >= 3)
                    {
                        ExportData(mArgs[0], mArgs[1], mArgs[2], mArgs[3]);
                    }
                    break;
                case MyTask.CheckUpdate:
                    bool showNew = (mArgs.Length >= 1) ? (mArgs[0] == bool.TrueString) : false;
                    OnCheckUpdate(showNew);
                    break;
                case MyTask.CutImages:
                    if (mArgs.Length >= 2)
                    {
                        bool replace = (mArgs.Length >= 2) ? (mArgs[1] == bool.TrueString) : true;
                        CutImages(mArgs[0], replace);
                    }
                    break;
                case MyTask.SaveAsMSE:
                    if (mArgs.Length >= 2)
                    {
                        bool replace = (mArgs.Length >= 2) ? (mArgs[1] == bool.TrueString) : false;
                        SaveMSEs(mArgs[0], CardList, replace);
                    }
                    break;
                case MyTask.ConvertImages:
                    if (mArgs.Length >= 2)
                    {
                        bool replace = (mArgs.Length >= 3) ? (mArgs[2] == bool.TrueString) : true;
                        ConvertImages(mArgs[0], mArgs[1], replace);
                    }
                    break;
            }
            isRun = false;
            lastTask = nowTask;
            nowTask = MyTask.NONE;
            CardList = Array.Empty<Card>();
            mArgs = Array.Empty<string>();
        }
        #endregion
    }

}

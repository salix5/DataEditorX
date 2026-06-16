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
        public string Datapath { get; }
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
            if (string.IsNullOrEmpty(newver))
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
                worker.ReportProgress(i / count, string.Format("{0}/{1}", i, count));
                string jpg = MyPath.Combine(imgpath, $"{c.id}.jpg");
                string outputPath = MyPath.Combine(Application.StartupPath, MyConfig.PATH_IMAGES);
                string savejpg = MyPath.Combine(outputPath, $"{c.id}.jpg");
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

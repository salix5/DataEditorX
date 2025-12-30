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
            this.worker = worker;
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
            MyBitmap.SaveAsJPEG(MyBitmap.Zoom(bmp, imgSet.W, imgSet.H), saveimg1, imgSet.quilty);
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
                worker.ReportProgress(i / count, string.Format("{0}/{1}", i, count));
                string jpg = MyPath.Combine(imgpath, c.id + ".jpg");
                string savejpg = MyPath.Combine(mseHelper.ImagePath, c.id + ".jpg");
                if (File.Exists(jpg) && (isreplace || !File.Exists(savejpg)))
                {
                    Bitmap bp = new(jpg);
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
                    bp.Dispose();
                    MyBitmap.SaveAsJPEG(bmp, savejpg, imgSet.quilty);
                    //bmp.Save(savejpg, ImageFormat.Png);
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

            foreach (string f in files)
            {
                if (isCancel)
                {
                    break;
                }

                i++;
                worker.ReportProgress(i / count, string.Format("{0}/{1}", i, count));
                string ex = Path.GetExtension(f).ToLower();
                string name = Path.GetFileNameWithoutExtension(f);
                string jpg_b = MyPath.Combine(picspath, name + ".jpg");
                if (ex == ".jpg" || ex == ".png" || ex == ".bmp")
                {
                    if (File.Exists(f))
                    {
                        Bitmap bmp = new(f);
                        //大图，如果替换，或者不存在
                        if (isreplace || !File.Exists(jpg_b))
                        {

                            MyBitmap.SaveAsJPEG(MyBitmap.Zoom(bmp, imgSet.W, imgSet.H),
                                                jpg_b, imgSet.quilty);
                        }
                    }
                }
            }
        }
        #endregion

        #region MSE存档
        public string MSEImagePath
        {
            get { return mseHelper.ImagePath; }
        }
        #endregion

        #region 导出数据
        public void ExportData(string path, string zipname, string _cdbfile, string modulescript)
        {
        }
        #endregion

        #region 运行
        public void Run()
        {
            isCancel = false;
            isRun = true;
            bool replace;
            bool showNew;
            switch (nowTask)
            {
                case MyTask.ExportData:
                    if (mArgs != null && mArgs.Length >= 3)
                    {
                        ExportData(mArgs[0], mArgs[1], mArgs[2], mArgs[3]);
                    }
                    break;
                case MyTask.CheckUpdate:
                    showNew = false;
                    if (mArgs != null && mArgs.Length >= 1)
                    {
                        showNew = mArgs[0] == bool.TrueString;
                    }
                    OnCheckUpdate(showNew);
                    break;
                case MyTask.CutImages:
                    if (mArgs != null && mArgs.Length >= 2)
                    {
                        replace = true;
                        if (mArgs.Length >= 2)
                        {
                            if (mArgs[1] == bool.FalseString)
                            {
                                replace = false;
                            }
                        }
                        CutImages(mArgs[0], replace);
                    }
                    break;
                case MyTask.SaveAsMSE:
                    break;
                case MyTask.ReadMSE:
                    break;
                case MyTask.ConvertImages:
                    if (mArgs != null && mArgs.Length >= 2)
                    {
                        replace = true;
                        if (mArgs.Length >= 3)
                        {
                            if (mArgs[2] == bool.FalseString)
                            {
                                replace = false;
                            }
                        }
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

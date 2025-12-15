/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 5月12 星期一
 * 时间: 12:00
 * 
 */
using DataEditorX.Config;
using DataEditorX.Language;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace DataEditorX
{
    internal sealed class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string arg = (args.Length > 0) ? args[0] : "";
            if (arg == MyConfig.TAG_SAVE_LANG || arg == MyConfig.TAG_SAVE_LANG2)
            {
                //保存语言
                SaveLanguage();
                MessageBox.Show("Save Language OK.");
                Environment.Exit(1);
            }
            using var mutex = new Mutex(true, "DataEditorX_SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                return;
            }
            string datapath = MyPath.Combine(Application.StartupPath, MyConfig.PATH_DATA);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm mainForm = new(datapath);
            //设置将要打开的文件
            mainForm.SetOpenFile(arg);
            //数据目录
            mainForm.InitializeData();
            Application.Run(mainForm);
        }
        static void SaveLanguage()
        {
            string datapath = MyPath.Combine(Application.StartupPath, MyConfig.PATH_DATA);
            string conflang = MyConfig.GetLanguageFile(datapath);
            LanguageHelper.LoadFormLabels(conflang);
            LanguageHelper langhelper = new();
            MainForm form1 = new(datapath);
            LanguageHelper.SetFormLabel(form1);
            langhelper.GetFormLabel(form1);
            DataEditForm form2 = new(datapath);
            LanguageHelper.SetFormLabel(form2);
            langhelper.GetFormLabel(form2);
            CodeEditForm form3 = new();
            LanguageHelper.SetFormLabel(form3);
            langhelper.GetFormLabel(form3);
            // LANG.GetFormLabel(this);
            //获取窗体文字
            langhelper.SaveLanguage(conflang + ".bak");
        }

    }
}

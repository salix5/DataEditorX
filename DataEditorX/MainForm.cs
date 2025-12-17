/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 2014-10-20
 * 时间: 9:19
 * 
 */
using DataEditorX.Config;
using DataEditorX.Controls;
using DataEditorX.Core;
using DataEditorX.Language;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DataEditorX
{
    public partial class MainForm : Form, IMainForm
    {
        #region member
        //历史
        History history;
        //数据目录
        string datapath;
        //语言配置
        string conflang;
        //数据库对比
        DataEditForm compare1, compare2;
        //临时卡片
        Card[] tCards;
        //编辑器配置
        DataConfig datacfg = null;
        CodeConfig codecfg = null;
        //将要打开的文件
        string openfile;
        #endregion

        #region 设置界面，消息语言
        public MainForm(string datapath)
        {
            //初始化控件
            InitializeComponent();
            this.datapath = datapath;
            //文件路径
            conflang = MyConfig.GetLanguageFile(datapath);
            //游戏数据,MSE数据
            datacfg = new DataConfig(MyConfig.GetCardInfoFile(datapath));
        }
        public void InitializeData()
        {
            //数据目录
            if (MyConfig.ReadBoolean(MyConfig.TAG_ASYNC))
            {
                //后台加载数据
                bgWorker1.RunWorkerAsync();
            }
            else
            {
                Init();
                InitForm();
            }
        }
        void CheckUpdate()
        {
            TaskHelper.CheckVersion(false);
        }
        public void SetOpenFile(string file)
        {
            openfile = file;
        }
        void Init()
        {
            //初始化YGOUtil的数据
            YGOUtil.SetConfig(datacfg);

            //代码提示
            string funtxt = MyPath.Combine(datapath, MyConfig.FILE_FUNCTION);
            string conlua = MyPath.Combine(datapath, MyConfig.FILE_CONSTANT);
            string confstring = MyPath.Combine(datapath, MyConfig.FILE_STRINGS);
            codecfg = new CodeConfig();
            //添加函数
            codecfg.AddFunction(funtxt);
            //添加指示物
            codecfg.AddStrings(confstring);
            //添加常量
            codecfg.AddConstant(conlua);
            codecfg.SetNames(datacfg.dicSetnames);
            //生成菜单
            codecfg.InitAutoMenus();
            history = new History(this);
            //读取历史记录
            history.ReadHistory(MyPath.Combine(datapath, MyConfig.FILE_HISTORY));
            //加载多语言
            LanguageHelper.LoadFormLabels(conflang);
        }
        void InitForm()
        {
            LanguageHelper.SetFormLabel(this);

            //设置所有窗口的语言
            DockContentCollection contents = dockPanel.Contents;
            foreach (DockContent dc in contents)
            {
                if (dc is Form)
                {
                    LanguageHelper.SetFormLabel(dc);
                }
            }
            //添加历史菜单
            history.MenuHistory();

            //如果没有将要打开的文件，则打开一个空数据库标签
            if (string.IsNullOrEmpty(openfile))
            {
                OpenDatabase("");
            }
            else
            {
                Open(openfile);
            }
        }
        #endregion

        #region 打开历史
        //清除cdb历史
        public void CdbMenuClear()
        {
            menuitem_history.DropDownItems.Clear();
        }
        //清除lua历史
        public void LuaMenuClear()
        {
            menuitem_shistory.DropDownItems.Clear();
        }
        //添加cdb历史
        public void AddCdbMenu(ToolStripItem item)
        {
            menuitem_history.DropDownItems.Add(item);
        }
        //添加lua历史
        public void AddLuaMenu(ToolStripItem item)
        {
            menuitem_shistory.DropDownItems.Add(item);
        }
        #endregion

        #region 打开文件
        //打开脚本
        void OpenScript(string file)
        {
            CodeEditForm cf = new();
            //设置界面语言
            LanguageHelper.SetFormLabel(cf);
            //设置cdb列表
            cf.SetCDBList(history.GetcdbHistory());
            //初始化函数提示
            cf.InitTooltip(codecfg);
            //打开文件
            cf.Open(file);
            cf.Show(dockPanel, DockState.Document);
        }
        //打开数据库
        void OpenDatabase(string file)
        {
            DataEditForm def = new(datapath, file);
            //设置语言
            LanguageHelper.SetFormLabel(def);
            //初始化界面数据
            def.InitControl(datacfg);
            def.Show(dockPanel, DockState.Document);
        }
        //打开文件
        public void Open(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return;
            }
            //添加历史
            history.AddHistory(file);
            //检查是否已经打开
            if (FindEditForm(file, true))
            {
                return;
            }
            //检查可用的
            if (FindEditForm(file, false))
            {
                return;
            }

            if (YGOUtil.IsScript(file))
            {
                OpenScript(file);
            }
            else if (YGOUtil.IsDatabase(file))
            {
                OpenDatabase(file);
            }
        }
        //检查是否打开
        bool FindEditForm(string file, bool isOpen)
        {
            DockContentCollection contents = dockPanel.Contents;
            //遍历所有标签
            foreach (DockContent dc in contents)
            {
                IEditForm edform = (IEditForm)dc;
                if (edform == null)
                {
                    continue;
                }

                if (isOpen)//是否检查打开
                {
                    if (file != null && file.Equals(edform.GetOpenFile()))
                    {
                        edform.Activate();
                        return true;
                    }
                }
                else//检查是否空白，如果为空，则打开文件
                {
                    if (string.IsNullOrEmpty(edform.GetOpenFile()) && edform.CanOpen(file))
                    {
                        edform.Open(file);
                        edform.Activate();
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region 窗口管理
        //关闭当前
        void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dockPanel.ActiveContent.DockHandler != null)
            {
                dockPanel.ActiveContent.DockHandler.Close();
            }
        }
        //打开脚本编辑
        void Menuitem_codeeditorClick(object sender, EventArgs e)
        {
            OpenScript(null);
        }

        //新建DataEditorX
        void DataEditorToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenDatabase("");
        }
        //关闭其他或者所有
        void CloseMdi(bool isall)
        {
            DockContentCollection contents = dockPanel.Contents;
            int num = contents.Count - 1;
            try
            {
                while (num >= 0)
                {
                    if (contents[num].DockHandler.DockState == DockState.Document)
                    {
                        if (isall)
                        {
                            contents[num].DockHandler.Close();
                        }
                        else if (dockPanel.ActiveContent != contents[num])
                        {
                            contents[num].DockHandler.Close();
                        }
                    }
                    num--;
                }
            }
            catch { }
        }
        //关闭其他
        void CloseOtherToolStripMenuItemClick(object sender, EventArgs e)
        {
            CloseMdi(false);
        }
        //关闭所有
        void CloseAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            CloseMdi(true);
        }
        #endregion

        #region 文件菜单
        //得到当前的数据编辑
        DataEditForm GetActive()
        {
            DataEditForm df = dockPanel.ActiveContent as DataEditForm;
            return df;
        }
        //打开文件
        void Menuitem_openClick(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.OpenFile);
            if (GetActive() != null || dockPanel.Contents.Count == 0)//判断当前窗口是不是DataEditor
            {
                dlg.Filter = MyConfig.CDB_TYPE;
            }
            else
            {
                dlg.Filter = MyConfig.SCRIPT_TYPE;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string file = dlg.FileName;
                Open(file);
            }
        }

        //退出
        void QuitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }
        //新建文件
        void Menuitem_newClick(object sender, EventArgs e)
        {
            using SaveFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.NewFile);
            if (GetActive() != null)//判断当前窗口是不是DataEditor
            {
                dlg.Filter = MyConfig.CDB_TYPE;
            }
            else
            {
                dlg.Filter = MyConfig.SCRIPT_TYPE;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string file = dlg.FileName;
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                //是否是数据库
                if (YGOUtil.IsDatabase(file))
                {
                    if (Database.CreateDatabase(file))//是否创建成功
                    {
                        if (MyMsg.Question(LMSG.IfOpenDatabase))//是否打开新建的数据库
                        {
                            Open(file);
                        }
                    }
                }
                else
                {
                    Open(file);
                }
            }
        }
        //保存文件
        void Menuitem_saveClick(object sender, EventArgs e)
        {
            if (dockPanel.ActiveContent is IEditForm cf)
            {
                if (cf.Save())//是否保存成功
                {
                    MyMsg.Show(LMSG.SaveFileOK);
                }
            }
        }
        #endregion

        #region 卡片复制粘贴
        //复制选中
        void Menuitem_copyselecttoClick(object sender, EventArgs e)
        {
            DataEditForm df = GetActive();
            if (df is null)
            {
                return;
            }
            tCards = df.GetCardList(true);
            if (tCards.Length == 0)
            {
                return;
            }
            SetCopyNumber(tCards.Length);
            MyMsg.Show(LMSG.CopyCards);
        }
        //复制当前结果
        void Menuitem_copyallClick(object sender, EventArgs e)
        {
            DataEditForm df = GetActive();
            if (df is null)
            {
                return;
            }
            tCards = df.GetCardList(false);
            if (tCards.Length == 0)
            {
                return;
            }
            SetCopyNumber(tCards.Length);
            MyMsg.Show(LMSG.CopyCards);
        }
        //显示复制卡片的数量
        void SetCopyNumber(int c)
        {
            string tmp = menuitem_pastecards.Text;
            int t = tmp.LastIndexOf(" (");
            if (t > 0)
            {
                tmp = tmp.Substring(0, t);
            }

            tmp = tmp + " (" + c.ToString() + ")";
            menuitem_pastecards.Text = tmp;
        }
        //粘贴卡片
        void Menuitem_pastecardsClick(object sender, EventArgs e)
        {
            if (tCards == null)
            {
                return;
            }

            DataEditForm df = GetActive();
            if (df == null)
            {
                return;
            }

            df.SaveCards(tCards);//保存卡片
            MyMsg.Show(LMSG.PasteCards);
        }

        #endregion

        #region 数据对比
        //设置数据库1
        void Menuitem_comp1Click(object sender, EventArgs e)
        {
            compare1 = GetActive();
            if (compare1 != null && !string.IsNullOrEmpty(compare1.GetOpenFile()))
            {
                menuitem_comp2.Enabled = true;
                CompareDB();
            }
        }
        //设置数据库2
        void Menuitem_comp2Click(object sender, EventArgs e)
        {
            compare2 = GetActive();
            if (compare2 != null && !string.IsNullOrEmpty(compare2.GetOpenFile()))
            {
                CompareDB();
            }
        }
        //对比数据库
        void CompareDB()
        {
            if (compare1 == null || compare2 == null)
            {
                return;
            }

            string cdb1 = compare1.GetOpenFile();
            string cdb2 = compare2.GetOpenFile();
            if (string.IsNullOrEmpty(cdb1)
               || string.IsNullOrEmpty(cdb2)
               || cdb1 == cdb2)
            {
                return;
            }

            bool checktext = MyMsg.Question(LMSG.CheckText);
            //分别对比数据库
            compare1.CompareCards(cdb2, checktext);
            compare2.CompareCards(cdb1, checktext);
            MyMsg.Show(LMSG.CompareOK);
            menuitem_comp2.Enabled = false;
            compare1 = null;
            compare2 = null;
        }

        #endregion

        #region 后台加载数据
        private void bgWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Init();
        }

        private void bgWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //更新UI
            InitForm();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //检查更新
            if (MyConfig.ReadBoolean(MyConfig.TAG_AUTO_CHECK_UPDATE))
            {
                Thread th = new(CheckUpdate)
                {
                    IsBackground = true//如果exe结束，则线程终止
                };
                th.Start();
            }
        }

    }
}

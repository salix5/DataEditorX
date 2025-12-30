/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 5月18 星期日
 * 时间: 20:22
 * 
 */
using DataEditorX.Common;
using DataEditorX.Config;
using DataEditorX.Core;
using DataEditorX.Core.Mse;
using DataEditorX.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DataEditorX
{
    public partial class DataEditForm : DockContent, IDataForm
    {
        string default_script_name = MyConfig.ReadString(MyConfig.TAG_DEFAULT_SCRIPT_NAME);

        public string DefaultScriptName
        {
            get
            {
                if (!string.IsNullOrEmpty(default_script_name))
                {
                    return default_script_name;
                }
                string cdbName = Path.GetFileNameWithoutExtension(nowCdbFile);
                if (cdbName.Length > 0 && File.Exists(ygopath.GetModuleScript(cdbName)))
                {
                    return cdbName;
                }
                return "";
            }
            set
            {
                default_script_name = value;
            }
        }

        #region 成员变量/构造
        readonly TaskHelper tasker;
        string taskname = "";
        //目录
        readonly YgoPath ygopath = new(Application.StartupPath);
        /// <summary>当前卡片</summary>
        Card oldCard = new(0);
        /// <summary>搜索条件</summary>
        Card srcCard = new(0);
        //卡片编辑
        readonly CardEdit cardedit;
        readonly string[] strs = Enumerable.Repeat("", Card.STR_SIZE).ToArray();
        /// <summary>
        /// 对比的id集合
        /// </summary>
        readonly List<long> codeList = new();
        //初始标题
        string title = "";
        string nowCdbFile = "";
        int maxRow = 20;
        int page = 1;
        int pageNum = 1;

        /// <summary>
        /// 搜索结果
        /// </summary>
        readonly List<Card> cardlist = new();

        //setcode正在输入
        readonly bool[] isSetcodeEditing = new bool[5];

        Image? cover;

        readonly string datapath = "";
        readonly string confcover = "";

        public DataEditForm(string datapath, string cdbfile) : this(datapath)
        {
            nowCdbFile = cdbfile;
        }
        public DataEditForm(string datapath)
        {
            this.datapath = datapath;
            cardedit = new CardEdit(this);
            confcover = MyPath.Combine(datapath, "cover.jpg");
            InitPath();
            InitializeComponent();
            title = Text;
            tasker = new TaskHelper(datapath, bgWorker1);
        }

        #endregion

        #region 接口
        public string GetOpenFile()
        {
            return nowCdbFile;
        }
        public bool CanOpen(string file)
        {
            return YGOUtil.IsDatabase(file);
        }
        public bool Create(string file)
        {
            return Database.CreateDatabase(file);
        }
        public bool Save()
        {
            return true;
        }
        #endregion

        #region 窗体
        //窗体第一次加载
        void DataEditForm_Load(object sender, EventArgs e)
        {
            HideMenu();//是否需要隐藏菜单
            SetTitle();//设置标题
            menuitem_operacardsfile.Checked = MyConfig.ReadBoolean(MyConfig.TAG_SYNC_WITH_CARD);
            menuitem_openfileinthis.Checked = MyConfig.ReadBoolean(MyConfig.TAG_OPEN_IN_THIS);
            menuitem_autocheckupdate.Checked = MyConfig.ReadBoolean(MyConfig.TAG_AUTO_CHECK_UPDATE);
            //Add MSE language items
            //AddMenuItemFormMSE();
            GetLanguageItem();
        }
        //窗体关闭
        void DataEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //当前有任务执行，是否结束
            if (tasker.IsRuning())
            {
                if (!CancelTask())
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        //窗体激活
        void DataEditForm_Enter(object sender, EventArgs e)
        {
            SetTitle();
        }
        private void DataEditForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                tb_cardname.Focus();
                tb_cardname.SelectAll();
            }
        }
        private void DataEditForm_Shown(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                InitListRows();
                if (File.Exists(nowCdbFile))
                {
                    Open(nowCdbFile);
                }
            }));
        }
        #endregion

        #region 初始化设置
        //隐藏菜单
        void HideMenu()
        {
            if (MdiParent == null)
            {
                return;
            }

            SuspendLayout();
            mainMenu.Visible = false;
            menuitem_file.Visible = false;
            menuitem_file.Enabled = false;
            ResumeLayout(true);
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(MenuStrip))
                {
                    continue;
                }

                Point p = c.Location;
                c.Location = new Point(p.X, p.Y - 25);
            }
            ResumeLayout();
        }
        //移除Tag
        string RemoveTag(string text)
        {
            int t = text.LastIndexOf(" (");
            if (t > 0)
            {
                return text.Substring(0, t);
            }
            return text;
        }
        //设置标题
        void SetTitle()
        {
            string str = title;
            string str2 = RemoveTag(title);
            if (!string.IsNullOrEmpty(nowCdbFile))
            {
                str = nowCdbFile + "-" + str;
                str2 = Path.GetFileName(nowCdbFile);
            }
            if (MdiParent != null) //父容器不为空
            {
                Text = str2;
                if (tasker.IsRuning())
                {
                    if (DockPanel.ActiveContent == this)
                    {
                        MdiParent.Text = str;
                    }
                }
                else
                {
                    MdiParent.Text = str;
                }
            }
            else
            {
                Text = str;
            }
        }
        //按cdb路径设置目录
        void SetCDBPath(string cdb)
        {
            nowCdbFile = cdb;
            SetTitle();
            string path = Application.StartupPath;
            if (cdb.Length > 0)
            {
                path = Path.GetDirectoryName(cdb);
            }
            ygopath.SetPath(path);
        }
        //初始化文件路径
        void InitPath()
        {
            if (File.Exists(confcover))
            {
                cover = MyBitmap.ReadImage(confcover);
            }
            else
            {
                cover = null;
            }
        }
        #endregion

        #region 界面控件
        //初始化控件
        public void InitControl(DataConfig datacfg)
        {
            try
            {
                InitComboBox(cb_cardrace, datacfg.dicCardRaces);
                InitComboBox(cb_cardattribute, datacfg.dicCardAttributes);
                InitComboBox(cb_cardrule, datacfg.dicCardRules);
                InitComboBox(cb_cardlevel, datacfg.dicCardLevels);
                InitCheckPanel(pl_cardtype, datacfg.dicCardTypes);
                InitCheckPanel(pl_markers, datacfg.dicLinkMarkers);
                InitCheckPanel(pl_category, datacfg.dicCardCategorys);
                InitComboBox(cb_setname1, datacfg.dicSetnames);
                InitComboBox(cb_setname2, datacfg.dicSetnames);
                InitComboBox(cb_setname3, datacfg.dicSetnames);
                InitComboBox(cb_setname4, datacfg.dicSetnames);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "启动错误");
            }
        }
        //初始化FlowLayoutPanel
        void InitCheckPanel(FlowLayoutPanel fpanel, InfoDictionary dict)
        {
            fpanel.SuspendLayout();
            fpanel.Controls.Clear();
            foreach (DictionaryEntry entry in dict)
            {
                string value = (string)entry.Value;
                if (value != null && value.StartsWith("NULL"))
                {
                    Label lab = new();
                    string[] sizes = value.Split(',');
                    if (sizes.Length >= 3)
                    {
                        lab.Size = new Size(int.Parse(sizes[1]), int.Parse(sizes[2]));
                    }
                    lab.AutoSize = false;
                    lab.Margin = fpanel.Margin;
                    fpanel.Controls.Add(lab);
                }
                else
                {
                    CheckBox _cbox = new()
                    {
                        Tag = entry.Key,
                        Text = value,
                        AutoSize = true,
                        Margin = fpanel.Margin
                    };
                    //_cbox.Click += PanelOnCheckClick;
                    fpanel.Controls.Add(_cbox);
                }
            }
            fpanel.ResumeLayout();
        }

        //初始化ComboBox
        void InitComboBox(ComboBox cb, InfoDictionary dict)
        {
            cb.ValueMember = "Key";
            cb.DisplayMember = "Value";
            cb.DataSource = dict.GetItems();
            if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = 0;
            }
        }

        //计算list最大行数
        void InitListRows()
        {
            bool addTest = lv_cardlist.Items.Count == 0;
            if (addTest)
            {
                ListViewItem item = new()
                {
                    Text = "Test"
                };
                lv_cardlist.Items.Add(item);
            }
            int headH = lv_cardlist.Items[0].GetBounds(ItemBoundsPortion.ItemOnly).Y;
            int itemH = lv_cardlist.Items[0].GetBounds(ItemBoundsPortion.ItemOnly).Height;
            if (itemH > 0)
            {
                int n = (lv_cardlist.Height - headH) / itemH;
                if (n > 0)
                {
                    maxRow = n;
                }
                //MessageBox.Show("height="+lv_cardlist.Height+",item="+itemH+",head="+headH+",max="+MaxRow);
            }
            if (addTest)
            {
                lv_cardlist.Items.Clear();
            }
            if (maxRow < 10)
            {
                maxRow = 20;
            }
        }
        //设置checkbox
        void SetCheck(FlowLayoutPanel fpl, long number)
        {
            long temp;
            //string strType = "";
            foreach (Control c in fpl.Controls)
            {
                if (c is CheckBox cbox)
                {
                    if (cbox.Tag == null)
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = (long)cbox.Tag;
                    }

                    if ((temp & number) == temp && temp != 0)
                    {
                        cbox.Checked = true;
                        //strType += "/" + c.Text;
                    }
                    else
                    {
                        cbox.Checked = false;
                    }
                }
            }
            //return strType;
        }
        void SetEnabled(FlowLayoutPanel fpl, bool set)
        {
            foreach (Control c in fpl.Controls)
            {
                if (c is CheckBox cbox)
                {
                    cbox.Enabled = set;
                }
            }
        }
        //设置combobox
        void SetSelect(ComboBox cb, long k)
        {
            cb.SelectedIndex = -1;
            cb.SelectedValue = k;
        }
        //得到所选值
        long GetSelect(ComboBox cb)
        {
            if (cb.SelectedIndex == -1)
            {
                return 0;
            }
            return (long)cb.SelectedValue;
        }
        //得到checkbox的总值
        long GetCheck(FlowLayoutPanel fpl)
        {
            long result = 0;
            foreach (Control c in fpl.Controls)
            {
                if (c is CheckBox cbox && cbox.Checked)
                {
                    long temp = 0;
                    if (cbox.Tag != null)
                    {
                        temp = (long)cbox.Tag;
                    }
                    result |= temp;
                }
            }
            return result;
        }
        //添加列表行
        void AddListView(int p)
        {
            int i, j, istart, iend;

            if (p <= 0)
            {
                p = 1;
            }
            else if (p >= pageNum)
            {
                p = pageNum;
            }

            istart = (p - 1) * maxRow;
            iend = p * maxRow;
            if (iend > cardlist.Count)
            {
                iend = cardlist.Count;
            }

            page = p;
            lv_cardlist.BeginUpdate();
            lv_cardlist.Items.Clear();
            if ((iend - istart) > 0)
            {
                ListViewItem[] items = new ListViewItem[iend - istart];
                Card mcard;
                for (i = istart, j = 0; i < iend; i++, j++)
                {
                    mcard = cardlist[i];
                    items[j] = new ListViewItem
                    {
                        Tag = i,
                        Text = mcard.id.ToString()
                    };
                    if (mcard.id == oldCard.id)
                    {
                        items[j].Checked = true;
                    }

                    if (i % 2 == 0)
                    {
                        items[j].BackColor = Color.GhostWhite;
                    }
                    else
                    {
                        items[j].BackColor = Color.White;
                    }

                    items[j].SubItems.Add(mcard.name);
                }
                lv_cardlist.Items.AddRange(items);
            }
            lv_cardlist.EndUpdate();
            tb_page.Text = page.ToString();
        }
        #endregion

        #region 设置卡片
        public YgoPath GetPath()
        {
            return ygopath;
        }
        public Card GetOldCard()
        {
            return oldCard;
        }

        private void setLinkMarks(long mark, bool setCheck = false)
        {
            if (setCheck)
            {
                SetCheck(pl_markers, mark);
            }
            tb_link.Text = Convert.ToString(mark, 2).PadLeft(9, '0');
        }

        public void LoadCard(Card c)
        {
            oldCard = c;

            tb_cardname.Text = c.name;
            tb_cardtext.Text = c.NormalizedDesc;

            Array.Copy(c.Str, strs, c.Str.Length);
            lb_scripttext.Items.Clear();
            lb_scripttext.Items.AddRange(c.Str);
            tb_edittext.Text = "";
            //data
            SetSelect(cb_cardrule, c.ot);
            SetSelect(cb_cardattribute, c.attribute);
            SetSelect(cb_cardlevel, c.GetLevel());
            SetSelect(cb_cardrace, c.race);
            //setcode
            long[] setcodes = c.GetSetcode();
            tb_setcode1.Text = setcodes[0].ToString("x");
            tb_setcode2.Text = setcodes[1].ToString("x");
            tb_setcode3.Text = setcodes[2].ToString("x");
            tb_setcode4.Text = setcodes[3].ToString("x");
            //type,category
            SetCheck(pl_cardtype, c.type);
            if (c.IsType(Core.Info.CardType.TYPE_LINK))
            {
                setLinkMarks(c.def, true);
            }
            else
            {
                tb_link.Text = "";
                SetCheck(pl_markers, 0);
            }
            SetCheck(pl_category, c.category);
            //Pendulum
            tb_pleft.Text = c.GetLeftScale().ToString();
            tb_pright.Text = c.GetRightScale().ToString();
            //atk，def
            tb_atk.Text = c.atk.ToString();
            if (c.IsType(Core.Info.CardType.TYPE_LINK))
            {
                tb_def.Text = "0";
                tb_def.Enabled = false;
            }
            else
            {
                tb_def.Text = c.def.ToString();
                tb_def.Enabled = true;
            }

            tb_cardcode.Text = c.id.ToString();
            tb_cardalias.Text = c.alias.ToString();
            SetImage(c.id.ToString());
        }
        #endregion

        #region 获取卡片
        public Card GetCard()
        {
            Card c = new(0)
            {
                name = tb_cardname.Text,
                desc = MyUtils.ConvertNewline(tb_cardtext.Text, false)
            };

            Array.Copy(strs, c.Str, c.Str.Length);

            long.TryParse(tb_cardcode.Text, out c.id);
            long.TryParse(tb_cardalias.Text, out c.alias);
            c.ot = GetSelect(cb_cardrule);
            c.SetSetcode(tb_setcode1.Text, tb_setcode2.Text, tb_setcode3.Text, tb_setcode4.Text);

            c.type = GetCheck(pl_cardtype);
            c.race = GetSelect(cb_cardrace);
            c.attribute = GetSelect(cb_cardattribute);
            c.category = GetCheck(pl_category);

            long level = GetSelect(cb_cardlevel) & 0xffffL;
            uint.TryParse(tb_pleft.Text, out uint temp);
            level |= (temp & 0xffU) << 24;
            uint.TryParse(tb_pright.Text, out temp);
            level |= (temp & 0xffU) << 16;
            c.level = level;
            if (tb_atk.Text == "?" || tb_atk.Text == "？")
            {
                c.atk = -2;
            }
            else if (tb_atk.Text == ".")
            {
                c.atk = -1;
            }
            else
            {
                long.TryParse(tb_atk.Text, out c.atk);
            }

            if (c.IsType(Core.Info.CardType.TYPE_LINK))
            {
                c.def = GetCheck(pl_markers);
            }
            else
            {
                if (tb_def.Text == "?" || tb_def.Text == "？")
                {
                    c.def = -2;
                }
                else if (tb_def.Text == ".")
                {
                    c.def = -1;
                }
                else
                {
                    long.TryParse(tb_def.Text, out c.def);
                }
            }

            return c;
        }
        #endregion

        #region 卡片列表
        //列表选择
        void Lv_cardlistSelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_cardlist.SelectedItems.Count > 0)
            {
                int sel = lv_cardlist.SelectedItems[0].Index;
                int index = (page - 1) * maxRow + sel;
                if (index < cardlist.Count)
                {
                    Card c = cardlist[index];
                    LoadCard(c);
                }
            }
        }
        //列表按键
        void Lv_cardlistKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    cardedit.DeleteCommand(menuitem_operacardsfile.Checked);
                    break;
                case Keys.Right:
                    NextPage();
                    break;
                case Keys.Left:
                    PrevPage();
                    break;
            }
        }
        //上一页
        void PrevPage()
        {
            if (!IsOpened())
            {
                return;
            }

            page--;
            AddListView(page);
        }
        void Btn_PageUpClick(object sender, EventArgs e)
        {
            PrevPage();
        }
        //下一页
        void NextPage()
        {
            if (!IsOpened())
            {
                return;
            }

            page++;
            AddListView(page);
        }
        void Btn_PageDownClick(object sender, EventArgs e)
        {
            NextPage();
        }
        private void tb_page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(tb_page.Text, out int p);
                if (p <= 0)
                {
                    return;
                }
                AddListView(p);
                e.SuppressKeyPress = true;
            }
        }
        #endregion

        #region 卡片搜索，打开
        //检查是否打开数据库
        public bool IsOpened()
        {
            return File.Exists(nowCdbFile);
        }
        //打开数据库
        public bool Open(string file)
        {
            if (!File.Exists(file))
            {
                MyMsg.Error(LMSG.FileIsNotExists);
                return false;
            }
            SetCDBPath(file);
            //清空
            codeList.Clear();
            cardlist.Clear();
            srcCard = new Card(0);
            //检查表是否存在
            Database.CheckTable(file);
            SetCards(Database.ReadAll(file), false);

            return true;
        }
        //setcode的搜索
        public bool CardFilter(Card c, Card sc)
        {
            bool res = true;
            if (sc.setcode != 0)
            {
                res = c.IsSetcode(sc.setcode & 0xffff);
            }

            return res;
        }
        //设置卡片列表的结果
        public void SetCards(Card[] cards, bool preservePage)
        {
            cardlist.Clear();
            foreach (Card c in cards)
            {
                if (CardFilter(c, srcCard))
                {
                    cardlist.Add(c);
                }
            }
            if (cardlist.Count > 0)
            {
                pageNum = (int)Math.Ceiling((double)cardlist.Count / maxRow);
                tb_pagenum.Text = pageNum.ToString();
                if (preservePage)//是否跳到之前页数
                {
                    AddListView(page);
                }
                else
                {
                    AddListView(1);
                }
            }
            else
            {
                pageNum = 1;
                page = 1;
                tb_pagenum.Text = pageNum.ToString();
                AddListView(1);
            }
        }
        //搜索卡片
        public void Refresh(bool preservePage)
        {
            Search(srcCard, preservePage);
        }
        void Search(Card c, bool preservePage)
        {
            if (!IsOpened())
            {
                return;
            }
            //如果临时卡片不为空，则更新，这个在搜索的时候清空
            if (codeList.Count > 0)
            {
                SetCards(Database.ReadFromId(nowCdbFile, codeList.ToArray()), true);
            }
            else
            {
                srcCard = c;
                SetCards(Database.ReadByCondition(nowCdbFile, c), preservePage);
            }
        }
        //更新临时卡片
        public void Reset()
        {
            oldCard = new Card(0);
            LoadCard(oldCard);
        }
        #endregion

        #region 按钮
        //搜索卡片
        void Btn_serachClick(object sender, EventArgs e)
        {
            codeList.Clear();
            Search(GetCard(), false);
        }
        //重置卡片
        void Btn_resetClick(object sender, EventArgs e)
        {
            Reset();
        }
        //添加
        void Btn_addClick(object sender, EventArgs e)
        {
            cardedit.AddCommand();
        }
        //修改
        void Btn_modClick(object sender, EventArgs e)
        {
            cardedit.UpdateCommand(menuitem_operacardsfile.Checked);
        }
        //打开脚本
        void Btn_luaClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }
            Card c = GetCard();
            string lua;
            if (c.id > 0)
            {
                lua = ygopath.GetScript(c.id);
            }
            else if (DefaultScriptName.Length > 0)
            {
                lua = ygopath.GetModuleScript(DefaultScriptName);
            }
            else
            {
                return;
            }
            if (!File.Exists(lua))
            {
                MyPath.CreateDirByFile(lua);
                if (MyMsg.Question(LMSG.IfCreateScript))
                {
                    using FileStream fs = new(lua, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter sw = new(fs, new UTF8Encoding(false));
                    sw.WriteLine("--" + c.name);
                    sw.WriteLine("local s,id,o=GetID()");
                    sw.WriteLine("function s.initial_effect(c)");
                    sw.WriteLine("\t");
                    sw.WriteLine("end");
                    sw.Close();
                    fs.Close();
                }
            }
            if (File.Exists(lua))
            {
                if (menuitem_openfileinthis.Checked)
                {
                    if (DockPanel.Parent is not MainForm main)
                    {
                        return;
                    }
                    main.Open(lua);
                }
                else
                {
                    System.Diagnostics.Process.Start(lua);
                }
            }
        }
        //删除
        void Btn_delClick(object sender, EventArgs e)
        {
            cardedit.DeleteCommand(menuitem_operacardsfile.Checked);
        }
        //导入卡图
        void Btn_imgClick(object sender, EventArgs e)
        {
            ImportImageFromSelect();
        }
        #endregion

        #region 文本框
        private void tb_cardcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                long.TryParse(tb_cardcode.Text, out long id);
                if (id <= 0)
                {
                    return;
                }
                Card c = new(id);
                codeList.Clear();
                Search(c, false);
                e.SuppressKeyPress = true;
            }
        }
        //卡片名称搜索、编辑
        void Tb_cardnameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tb_cardname.Text.Length == 0)
                {
                    return;
                }
                Card c = new(0)
                {
                    name = tb_cardname.Text
                };
                codeList.Clear();
                Search(c, false);
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.R && e.Control)
            {
                Reset();
            }
        }

        //脚本文本
        void Lb_scripttextSelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lb_scripttext.SelectedIndex;
            if (index < 0)
            {
                tb_edittext.Text = "";
            }
            tb_edittext.Text = strs[index];
        }

        //脚本文本
        void Tb_edittextTextChanged(object sender, EventArgs e)
        {
            int index = lb_scripttext.SelectedIndex;
            if (index < 0)
            {
                return;
            }
            strs[index] = tb_edittext.Text;
            if ((string)lb_scripttext.Items[index] != tb_edittext.Text)
            {
                lb_scripttext.Items[index] = tb_edittext.Text;
            }
        }
        #endregion

        #region 帮助菜单
        void Menuitem_aboutClick(object sender, EventArgs e)
        {
            MyMsg.Show(
                LanguageHelper.GetMsg(LMSG.About) + Application.ProductName + "\n"
                + LanguageHelper.GetMsg(LMSG.Version) + Application.ProductVersion + "\n"
                + LanguageHelper.GetMsg(LMSG.Author) + "salix5");
        }

        void Menuitem_checkupdateClick(object sender, EventArgs e)
        {
            CheckUpdate(true);
        }
        public void CheckUpdate(bool showNew)
        {
            if (!isRun())
            {
                tasker.SetTask(MyTask.CheckUpdate, Array.Empty<Card>(), showNew.ToString());
                Run(LanguageHelper.GetMsg(LMSG.checkUpdate));
            }
        }
        bool CancelTask()
        {
            bool bl = false;
            if (tasker.IsRuning())
            {
                bl = MyMsg.Question(LMSG.IfCancelTask);
                if (bl)
                {
                    tasker.Cancel();

                    if (bgWorker1.IsBusy)
                    {
                        bgWorker1.CancelAsync();
                    }
                }
            }
            return bl;
        }
        void Menuitem_cancelTaskClick(object sender, EventArgs e)
        {
            CancelTask();
        }
        void Menuitem_githubClick(object sender, EventArgs e)
        {
            MyUtils.OpenRepository();
        }
        #endregion

        #region 文件菜单
        //打开文件
        void Menuitem_openClick(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectDatabasePath);
            dlg.Filter = MyConfig.CDB_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Open(dlg.FileName);
            }
        }
        //新建文件
        void Menuitem_newClick(object sender, EventArgs e)
        {
            using SaveFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectDatabasePath);
            dlg.Filter = MyConfig.CDB_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (Database.CreateDatabase(dlg.FileName))
                {
                    if (MyMsg.Question(LMSG.IfOpenDatabase))
                    {
                        Open(dlg.FileName);
                    }
                }
            }
        }
        //读取ydk
        void Menuitem_readydkClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectYdkPath);
            dlg.Filter = MyConfig.YDK_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                codeList.Clear();
                long[] ids = YGOUtil.ReadYDK(dlg.FileName);
                codeList.AddRange(ids);
                SetCards(Database.ReadFromId(nowCdbFile, ids), false);
            }
        }
        //从图片文件夹读取
        void Menuitem_readimagesClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            using FolderBrowserDialog fdlg = new();
            fdlg.Description = LanguageHelper.GetMsg(LMSG.SelectImagePath);
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                codeList.Clear();
                long[] ids = YGOUtil.ReadImage(fdlg.SelectedPath);
                codeList.AddRange(ids);
                SetCards(Database.ReadFromId(nowCdbFile, ids), false);
            }
        }
        //关闭
        void Menuitem_quitClick(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region 线程
        //是否在执行
        bool isRun()
        {
            if (tasker.IsRuning())
            {
                MyMsg.Warning(LMSG.RunError);
                return true;
            }
            return false;
        }
        //执行任务
        void Run(string name)
        {
            if (isRun())
            {
                return;
            }

            taskname = name;
            title = title + " (" + taskname + ")";
            SetTitle();
            bgWorker1.RunWorkerAsync();
        }
        //线程任务
        void BgWorker1DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            tasker.Run();
        }
        void BgWorker1ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            title = string.Format("{0} ({1}-{2})",
                                  RemoveTag(title),
                                  taskname,
                                  // e.ProgressPercentage,
                                  e.UserState);
            SetTitle();
        }
        //任务完成
        void BgWorker1RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //还原标题
            int t = title.LastIndexOf(" (");
            if (t > 0)
            {
                title = title.Substring(0, t);
                SetTitle();
            }
            if (e.Error is not null)
            {
                tasker.Cancel();

                if (bgWorker1.IsBusy)
                {
                    bgWorker1.CancelAsync();
                }

                MyMsg.Show(LanguageHelper.GetMsg(LMSG.TaskError) + "\n" + e.Error);
            }
            else if (tasker.IsCancel() || e.Cancelled)
            {//取消任务
                MyMsg.Show(LMSG.CancelTask);
            }
            else
            {
                MyTask mt = tasker.GetLastTask();
                switch (mt)
                {
                    case MyTask.CheckUpdate:
                        break;
                    case MyTask.ExportData:
                        MyMsg.Show(LMSG.ExportDataOK);
                        break;
                    case MyTask.CutImages:
                        MyMsg.Show(LMSG.CutImageOK);
                        break;
                    case MyTask.SaveAsMSE:
                        MyMsg.Show(LMSG.SaveMseOK);
                        break;
                    case MyTask.ConvertImages:
                        MyMsg.Show(LMSG.ConvertImageOK);
                        break;
                    case MyTask.ReadMSE:
                        //保存读取的卡片
                        SaveCards(tasker.CardList);
                        MyMsg.Show(LMSG.ReadMSEisOK);
                        break;
                }
            }
        }
        #endregion

        #region 复制卡片
        //得到卡片列表，是否是选中的
        public Card[] GetCardList(bool onlyselect)
        {
            if (!IsOpened())
            {
                return Array.Empty<Card>();
            }

            List<Card> cards = new();
            if (onlyselect)
            {
                foreach (ListViewItem lvitem in lv_cardlist.SelectedItems)
                {
                    int index;
                    if (lvitem.Tag != null)
                    {
                        index = (int)lvitem.Tag;
                    }
                    else
                    {
                        index = lvitem.Index + (page - 1) * maxRow;
                    }

                    if (index >= 0 && index < cardlist.Count)
                    {
                        cards.Add(cardlist[index]);
                    }
                }
            }
            else
            {
                cards.AddRange(cardlist.ToArray());
            }
            return cards.ToArray();
        }
        //保存卡片到当前数据库
        public void SaveCards(Card[] cards)
        {
            cardedit.CopyCommand(cards);
        }
        #endregion

        #region MSE存档/裁剪图片
        //裁剪图片
        void Menuitem_cutimagesClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            if (isRun())
            {
                return;
            }

            bool isreplace = MyMsg.Question(LMSG.IfReplaceExistingImage);
            tasker.SetTask(MyTask.CutImages, cardlist.ToArray(), ygopath.picpath, isreplace.ToString());
            Run(LanguageHelper.GetMsg(LMSG.CutImage));
        }
        void Menuitem_saveasmse_selectClick(object sender, EventArgs e)
        {
            //选择
            SaveAsMSE(true);
        }

        void Menuitem_saveasmseClick(object sender, EventArgs e)
        {
            //全部
            SaveAsMSE(false);
        }
        void SaveAsMSE(bool onlyselect)
        {
            if (!IsOpened())
            {
                return;
            }

            if (isRun())
            {
                return;
            }

            Card[] cards = GetCardList(onlyselect);
            if (cards.Length == 0)
            {
                return;
            }
            //select save mse-set
            using SaveFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectMseSet);
            dlg.Filter = MyConfig.MSE_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                bool isUpdate = false;
#if DEBUG
                isUpdate = MyMsg.Question(LMSG.OnlySet);
#endif
                tasker.SetTask(MyTask.SaveAsMSE, cards, dlg.FileName, isUpdate.ToString());
                Run(LanguageHelper.GetMsg(LMSG.SaveMse));
            }
        }
        #endregion

        #region 导入卡图
        void ImportImageFromSelect()
        {
            string tid = tb_cardcode.Text;
            if (tid == "0" || tid.Length == 0)
            {
                return;
            }

            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectImage) + "-" + tb_cardname.Text;
            dlg.Filter = MyConfig.IMAGE_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //dlg.FileName;
                ImportImage(dlg.FileName, tid);
            }
        }
        private void pl_image_DoubleClick(object sender, EventArgs e)
        {
            ImportImageFromSelect();
        }
        void Pl_imageDragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[] ?? Array.Empty<string>();
            if (files.Length > 0 && File.Exists(files[0]))
            {
                ImportImage(files[0], tb_cardcode.Text);
            }
        }

        void Pl_imageDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link; //重要代码：表明是链接类型的数据，比如文件路径
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void menuitem_importmseimg_Click(object sender, EventArgs e)
        {
            string tid = tb_cardcode.Text;
            menuitem_importmseimg.Checked = !menuitem_importmseimg.Checked;
            SetImage(tid);
        }
        void ImportImage(string file, string tid)
        {
            string f;
            if (pl_image.BackgroundImage != null
                && pl_image.BackgroundImage != cover)
            {//释放图片资源
                pl_image.BackgroundImage.Dispose();
                pl_image.BackgroundImage = cover;
            }
            if (menuitem_importmseimg.Checked)
            {
                if (!Directory.Exists(tasker.MSEImagePath))
                {
                    Directory.CreateDirectory(tasker.MSEImagePath);
                }

                f = MyPath.Combine(tasker.MSEImagePath, tid + ".jpg");
                File.Copy(file, f, true);
            }
            else
            {
                //	tasker.ToImg(file, ygopath.GetImage(tid),
                //				 ygopath.GetImageThum(tid));
                tasker.ToImg(file, ygopath.GetImage(tid));
            }
            SetImage(tid);
        }
        public void SetImage(string id)
        {
            long.TryParse(id, out long t);
            SetImage(t);
        }
        public void SetImage(long id)
        {
            string pic = ygopath.GetImage(id);
            if (menuitem_importmseimg.Checked)//显示MSE图片
            {
                string msepic = MseMaker.GetCardImagePath(tasker.MSEImagePath, oldCard);
                if (File.Exists(msepic))
                {
                    pl_image.BackgroundImage = MyBitmap.ReadImage(msepic);
                    return;
                }
            }
            pl_image.BackgroundImage = MyBitmap.ReadImage(pic) ?? cover;
        }
        void Menuitem_convertimageClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            if (isRun())
            {
                return;
            }

            using FolderBrowserDialog fdlg = new();
            fdlg.Description = LanguageHelper.GetMsg(LMSG.SelectImagePath);
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                bool isreplace = MyMsg.Question(LMSG.IfReplaceExistingImage);
                tasker.SetTask(MyTask.ConvertImages, Array.Empty<Card>(), fdlg.SelectedPath, ygopath.gamepath, isreplace.ToString());
                Run(LanguageHelper.GetMsg(LMSG.ConvertImage));
            }
        }
        #endregion

        #region 导出数据包
        void Menuitem_exportdataClick(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            if (isRun())
            {
                return;
            }

            using SaveFileDialog dlg = new();
            dlg.InitialDirectory = ygopath.gamepath;
            dlg.Filter = "Zip|*.zip|All Files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tasker.SetTask(MyTask.ExportData, GetCardList(false), ygopath.gamepath, dlg.FileName, GetOpenFile(), DefaultScriptName);
                Run(LanguageHelper.GetMsg(LMSG.ExportData));
            }

        }
        #endregion

        #region 对比数据
        /// <summary>
        /// 数据一致，返回true，不存在和数据不同，则返回false
        /// </summary>
        bool CheckCard(Card[] cards, Card card, bool checkinfo)
        {
            foreach (Card c in cards)
            {
                if (c.id != card.id)
                {
                    continue;
                }
                //data数据不一样
                if (checkinfo)
                {
                    return card.EqualsData(c);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public void CompareCards(string cdbfile, bool checktext)
        {
            if (!IsOpened())
            {
                return;
            }

            codeList.Clear();
            srcCard = new Card(0);
            Card[] mcards = Database.ReadAll(nowCdbFile);
            Card[] cards = Database.ReadAll(cdbfile);
            foreach (Card card in mcards)
            {
                if (!CheckCard(cards, card, checktext))
                {
                    codeList.Add(card.id);
                }
            }
            SetCards(Database.ReadFromId(nowCdbFile, codeList.ToArray()), false);
        }
        #endregion

        #region MSE配置菜单
        //把文件添加到菜单
        /*void AddMenuItemFormMSE()
        {
            if (!Directory.Exists(datapath))
            {
                return;
            }

            menuitem_mseconfig.DropDownItems.Clear();//清空
            string[] files = Directory.GetFiles(datapath);
            foreach (string file in files)
            {
                string name = MyPath.GetFullFileName(MSEConfig.TAG, file);
                //是否是MSE配置文件
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                //菜单文字是语言
                ToolStripMenuItem tsmi = new(name)
                {
                    ToolTipText = file//提示文字为真实路径
                };
                tsmi.Click += SetMseConfig_Click;
                if (msecfg.configName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    tsmi.Checked = true;//如果是当前，则打勾
                }

                menuitem_mseconfig.DropDownItems.Add(tsmi);
            }
        }
        void SetMseConfig_Click(object sender, EventArgs e)
        {
            if (isRun())//正在执行任务
            {
                return;
            }

            if (sender is ToolStripMenuItem tsmi)
            {
                //读取新的配置
                msecfg.SetConfig(tsmi.ToolTipText, datapath);
                //刷新菜单
                AddMenuItemFormMSE();
                //保存配置
                ConfigManager.Save(MyConfig.TAG_MSE_LANGUAGE, tsmi.Text);
            }
        }*/
        #endregion

        #region 查找lua函数
        private void menuitem_findluafunc_Click(object sender, EventArgs e)
        {
            string funtxt = MyPath.Combine(datapath, MyConfig.FILE_FUNCTION);
            using FolderBrowserDialog fd = new();
            fd.Description = "Folder Name: ocgcore";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                LuaFunction.Read(funtxt);//先读取旧函数列表
                LuaFunction.Find(fd.SelectedPath);//查找新函数，并保存
                MessageBox.Show("OK");
            }
        }

        #endregion

        #region 系列名textbox
        //系列名输入时
        void setCode_InputText(int index, ComboBox cb, TextBox tb)
        {
            if (index >= 0 && index < isSetcodeEditing.Length)
            {
                if (isSetcodeEditing[index])//如果正在编辑
                {
                    return;
                }

                isSetcodeEditing[index] = true;
                long.TryParse(tb.Text, NumberStyles.HexNumber, null, out long temp);
                SetSelect(cb, temp);
                isSetcodeEditing[index] = false;
            }
        }
        private void tb_setcode1_TextChanged(object sender, EventArgs e)
        {
            setCode_InputText(1, cb_setname1, tb_setcode1);
        }

        private void tb_setcode2_TextChanged(object sender, EventArgs e)
        {
            setCode_InputText(2, cb_setname2, tb_setcode2);
        }

        private void tb_setcode3_TextChanged(object sender, EventArgs e)
        {
            setCode_InputText(3, cb_setname3, tb_setcode3);
        }

        private void tb_setcode4_TextChanged(object sender, EventArgs e)
        {
            setCode_InputText(4, cb_setname4, tb_setcode4);
        }
        #endregion

        #region 系列名comobox
        //系列选择框 选择时
        void setCode_Selected(int index, ComboBox cb, TextBox tb)
        {
            if (index >= 0 && index < isSetcodeEditing.Length)
            {
                if (isSetcodeEditing[index])//如果正在编辑
                {
                    return;
                }

                isSetcodeEditing[index] = true;
                long tmp = GetSelect(cb);
                tb.Text = tmp.ToString("x");
                isSetcodeEditing[index] = false;
            }
        }
        private void cb_setname1_SelectedIndexChanged(object sender, EventArgs e)
        {
            setCode_Selected(1, cb_setname1, tb_setcode1);
        }

        private void cb_setname2_SelectedIndexChanged(object sender, EventArgs e)
        {
            setCode_Selected(2, cb_setname2, tb_setcode2);
        }

        private void cb_setname3_SelectedIndexChanged(object sender, EventArgs e)
        {
            setCode_Selected(3, cb_setname3, tb_setcode3);
        }

        private void cb_setname4_SelectedIndexChanged(object sender, EventArgs e)
        {
            setCode_Selected(4, cb_setname4, tb_setcode4);
        }
        #endregion

        #region 读取MSE存档
        private void menuitem_readmse_Click(object sender, EventArgs e)
        {
            if (!IsOpened())
            {
                return;
            }

            if (isRun())
            {
                return;
            }
            //select open mse-set
            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectMseSet);
            dlg.Filter = MyConfig.MSE_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                bool isUpdate = MyMsg.Question(LMSG.IfReplaceExistingImage);
                tasker.SetTask(MyTask.ReadMSE, Array.Empty<Card>(), dlg.FileName, isUpdate.ToString());
                Run(LanguageHelper.GetMsg(LMSG.ReadMSE));
            }
        }
        #endregion

        #region 设置
        //删除卡片的时候，是否要删除图片和脚本
        private void menuitem_deletecardsfile_Click(object sender, EventArgs e)
        {
            menuitem_operacardsfile.Checked = !menuitem_operacardsfile.Checked;
            ConfigManager.Save(MyConfig.TAG_SYNC_WITH_CARD, menuitem_operacardsfile.Checked.ToString().ToLower());
        }
        //用CodeEditor打开lua
        private void menuitem_openfileinthis_Click(object sender, EventArgs e)
        {
            menuitem_openfileinthis.Checked = !menuitem_openfileinthis.Checked;
            ConfigManager.Save(MyConfig.TAG_OPEN_IN_THIS, menuitem_openfileinthis.Checked.ToString().ToLower());
        }
        //自动检查更新
        private void menuitem_autocheckupdate_Click(object sender, EventArgs e)
        {
            menuitem_autocheckupdate.Checked = !menuitem_autocheckupdate.Checked;
            ConfigManager.Save(MyConfig.TAG_AUTO_CHECK_UPDATE, menuitem_autocheckupdate.Checked.ToString().ToLower());
        }
        //set default script name
        private void menuitem_default_script_Click(object sender, EventArgs e)
        {
            DefaultScriptName = Microsoft.VisualBasic.Interaction.InputBox("Set default script name (without extension).\n\nPress \"Cancel\" to remove default script name.", "", DefaultScriptName);
            ConfigManager.Save(MyConfig.TAG_DEFAULT_SCRIPT_NAME, DefaultScriptName);
        }
        #endregion

        #region 语言菜单
        void GetLanguageItem()
        {
            if (!Directory.Exists(datapath))
            {
                return;
            }

            menuitem_language.DropDownItems.Clear();
            string[] files = Directory.GetFiles(datapath, MyConfig.TAG_LANGUAGE);
            foreach (string file in files)
            {
                string name = MyPath.GetFullFileName(MyConfig.TAG_LANGUAGE, file);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                TextInfo txinfo = new CultureInfo(CultureInfo.InstalledUICulture.Name).TextInfo;
                ToolStripMenuItem tsmi = new(txinfo.ToTitleCase(name))
                {
                    ToolTipText = file
                };
                tsmi.Click += SetLanguage_Click;
                if (MyConfig.ReadString(MyConfig.TAG_LANGUAGE).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    tsmi.Checked = true;
                }
                menuitem_language.DropDownItems.Add(tsmi);
            }
        }
        void SetLanguage_Click(object sender, EventArgs e)
        {
            if (isRun())
            {
                return;
            }

            if (sender is ToolStripMenuItem tsmi)
            {
                ConfigManager.Save(MyConfig.TAG_LANGUAGE, tsmi.Text);
                GetLanguageItem();
                MyMsg.Show(LMSG.PlzRestart);
            }
        }
        #endregion

        //把mse存档导出为图片
        void Menuitem_exportMSEimageClick(object sender, EventArgs e)
        {
            if (isRun())
            {
                return;
            }

            string msepath = MyPath.GetRealPath(MyConfig.ReadString(MyConfig.TAG_MSE_PATH));
            if (!File.Exists(msepath))
            {
                MyMsg.Error(LMSG.ExportMseImagesErr);
                menuitem_exportMSEimage.Checked = false;
                return;
            }
            else
            {
                if (MseMaker.MseIsRunning())
                {
                    MseMaker.MseStop();
                    menuitem_exportMSEimage.Checked = false;
                    return;
                }
            }
            //select open mse-set
            using OpenFileDialog dlg = new();
            dlg.Title = LanguageHelper.GetMsg(LMSG.SelectMseSet);
            dlg.Filter = MyConfig.MSE_TYPE;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string mseset = dlg.FileName;
                string exportpath = MyPath.GetRealPath(MyConfig.ReadString(MyConfig.TAG_MSE_EXPORT));
                MseMaker.ExportSet(msepath, mseset, exportpath, delegate
                {
                    menuitem_exportMSEimage.Checked = false;
                });
                menuitem_exportMSEimage.Checked = true;
            }
            else
            {
                menuitem_exportMSEimage.Checked = false;
            }
        }
        void Menuitem_testPendulumTextClick(object sender, EventArgs e)
        {
            Card c = GetCard();
            tasker.TestPendulumText(c.desc);
        }
        void Menuitem_export_select_sqlClick(object sender, EventArgs e)
        {
            using SaveFileDialog dlg = new();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Database.ExportSQL(dlg.FileName, GetCardList(true));
                MyMsg.Show("OK");
            }
        }
        void Menuitem_export_all_sqlClick(object sender, EventArgs e)
        {
            using SaveFileDialog dlg = new();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Database.ExportSQL(dlg.FileName, GetCardList(false));
                MyMsg.Show("OK");
            }
        }

        private void text2LinkMarks(string text)
        {
            try
            {
                long mark = Convert.ToInt64(text, 2);
                setLinkMarks(mark, true);
            }
            catch
            {
                //
            }
        }

        void Tb_linkTextChanged(object sender, EventArgs e)
        {
            text2LinkMarks(tb_link.Text);
        }

        private void tb_cardtext_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.R)
            {
                Reset();
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
            {
                tb_cardname.Focus();
            }
        }

        private void menuitem_language_Click(object sender, EventArgs e)
        {

        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] drops = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (drops == null)
            {
                return;
            }
            List<string> files = new();
            foreach (string file in drops)
            {
                if (Directory.Exists(file))
                {
                    files.AddRange(Directory.EnumerateFiles(file, "*.cdb", SearchOption.AllDirectories));
                    files.AddRange(Directory.EnumerateFiles(file, "*.lua", SearchOption.AllDirectories));
                }
                files.Add(file);
            }
            if (files.Count > 5)
            {
                if (!MyMsg.Question(LMSG.ManyFilesWarning))
                {
                    return;
                }
            }
            if (DockPanel.Parent is not MainForm main)
            {
                return;
            }
            foreach (string file in files)
            {
                main.Open(file);
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        void Tb_linkKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '0' && e.KeyChar != '1' && e.KeyChar != 1 && e.KeyChar != 22 && e.KeyChar != 3 && e.KeyChar != 8)
            {
                //				MessageBox.Show("key="+(int)e.KeyChar);
                e.Handled = true;
            }
            else
            {
                text2LinkMarks(tb_link.Text);
            }
        }

    }
}

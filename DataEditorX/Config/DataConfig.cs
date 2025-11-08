/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 2014-10-23
 * 时间: 7:54
 * 
 */
using System.IO;
using System.Windows.Forms;
using DataEditorX.Common;

namespace DataEditorX.Config
{
    /// <summary>
    /// DataEditor的数据
    /// </summary>
    public class DataConfig
    {
        public DataConfig()
        {
            InitMember(MyPath.Combine(Application.StartupPath, MyConfig.TAG_CARDINFO + ".txt"));
        }
        public DataConfig(string conf)
        {
            InitMember(conf);
        }
        /// <summary>
        /// 初始化成员
        /// </summary>
        /// <param name="conf"></param>
        public void InitMember(string conf)
        {
            //conf = MyPath.Combine(datapath, MyConfig.FILE_INFO);
            if (!File.Exists(conf))
            {
                return;
            }
            //提取内容
            string text = MyUtils.ConvertNewline(File.ReadAllText(conf), false);
            dicCardRules.Initialize(text, MyConfig.TAG_RULE);
            dicSetnames.Initialize(text, MyConfig.TAG_SETNAME);
            dicCardTypes.Initialize(text, MyConfig.TAG_TYPE);
            dicLinkMarkers.Initialize(text, MyConfig.TAG_MARKER);
            dicCardCategorys.Initialize(text, MyConfig.TAG_CATEGORY);
            dicCardAttributes.Initialize(text, MyConfig.TAG_ATTRIBUTE);
            dicCardRaces.Initialize(text, MyConfig.TAG_RACE);
            dicCardLevels.Initialize(text, MyConfig.TAG_LEVEL);
        }
        /// <summary>
        /// 规则
        /// </summary>
        public InfoDictionary dicCardRules = new InfoDictionary();
        /// <summary>
        /// 属性
        /// </summary>
        public InfoDictionary dicCardAttributes = new InfoDictionary();
        /// <summary>
        /// 种族
        /// </summary>
        public InfoDictionary dicCardRaces = new InfoDictionary();
        /// <summary>
        /// 等级
        /// </summary>
        public InfoDictionary dicCardLevels = new InfoDictionary();
        /// <summary>
        /// 系列名
        /// </summary>
        public InfoDictionary dicSetnames = new InfoDictionary();
        /// <summary>
        /// 卡片类型
        /// </summary>
        public InfoDictionary dicCardTypes = new InfoDictionary();
        /// <summary>
        /// 连接标志
        /// </summary>
        public InfoDictionary dicLinkMarkers = new InfoDictionary();
        /// <summary>
        /// 效果类型
        /// </summary>
        public InfoDictionary dicCardCategorys = new InfoDictionary();
    }
}

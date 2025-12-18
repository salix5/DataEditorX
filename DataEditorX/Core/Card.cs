/*
 * CreateDate :2014-02-07
 * desc :卡片类
 * ModiftyDate :2014-02-12
 */
using DataEditorX.Core.Info;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DataEditorX.Core
{
    public struct Card : IEquatable<Card>
    {
        public const int STR_SIZE = 16;
        public const int SETCODE_SIZE = 4;

        #region 构造
        /// <summary>
        /// 卡片
        /// </summary>
        /// <param name="cardCode">密码</param>
        public Card(long cardCode)
        {
            id = cardCode;
            name = "";
            ot = 0;
            alias = 0;
            setcode = 0;
            type = 0;
            atk = 0;
            def = 0;
            level = 0;
            race = 0;
            attribute = 0;
            category = 0;
            desc = "";
            str = new string[STR_SIZE];
            for (int i = 0; i < str.Length; i++)
            {
                str[i] = "";
            }
        }
        #endregion

        #region 成员
        /// <summary>卡片密码</summary>
        public long id;
        /// <summary>卡片规则</summary>
        public long ot;
        /// <summary>卡片同名卡</summary>
        public long alias;
        /// <summary>卡片系列号</summary>
        public long setcode;
        /// <summary>卡片种类</summary>
        public long type;
        /// <summary>攻击力</summary>
        public long atk;
        /// <summary>防御力</summary>
        public long def;
        /// <summary>卡片等级</summary>
        public long level;
        /// <summary>卡片种族</summary>
        public long race;
        /// <summary>卡片属性</summary>
        public long attribute;
        /// <summary>效果种类</summary>
        public long category;
        /// <summary>卡片名称</summary>
        public string name;
        /// <summary>描述文本</summary>
        public string desc;
        public string NormalizedDesc => desc.Replace("\n", Environment.NewLine);
        string[] str;
        /// <summary>脚本文件文字</summary>
        public string[] Str
        {
            get
            {
                if (str == null)
                {
                    str = new string[STR_SIZE];
                    for (int i = 0; i < str.Length; i++)
                    {
                        str[i] = "";
                    }
                }
                return str;
            }
            set { str = value; }
        }
        public long[] GetSetcode()
        {
            long[] list = new long[SETCODE_SIZE];
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = (setcode >> (16 * i)) & 0xffffL;
            }
            return list;
        }
        public void SetSetcode(long[] setcodes)
        {
            if (setcodes == null)
            {
                return;
            }
            List<long> valueList = new();
            HashSet<long> checker = new();
            foreach (long sc in setcodes)
            {
                if (sc <= 0 || sc > 0xffffL)
                {
                    continue;
                }
                if (checker.Contains(sc))
                {
                    continue;
                }
                checker.Add(sc);
                valueList.Add(sc);
                if (valueList.Count >= SETCODE_SIZE)
                {
                    break;
                }
            }
            long result = 0;
            for (int i = 0; i < valueList.Count; i++)
            {
                result |= (valueList[i] & 0xffffL) << (16 * i);
            }
            setcode = result;
        }
        public void SetSetcode(params string[] setcodes)
        {
            if (setcodes == null)
            {
                return;
            }
            List<long> valueList = new();
            foreach (string str in setcodes)
            {
                if (!long.TryParse(str, NumberStyles.HexNumber, null, out long value))
                {
                    continue;
                }
                valueList.Add(value);
            }
            SetSetcode(valueList.ToArray());
        }
        public long GetLeftScale()
        {
            return (level >> 24) & 0xffL;
        }
        public long GetRightScale()
        {
            return (level >> 16) & 0xffL;
        }
        public long GetLevel()
        {
            return level & 0xffffL;
        }
        #endregion

        #region 比较、哈希值、操作符
        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>结果</returns>
        public override bool Equals(object obj)
        {
            if (obj is Card card)
            {
                return Equals(card); // use Equals method below
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 比较卡片，除脚本提示文本
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualsData(Card other)
        {
            if (id != other.id)
            {
                return false;
            }
            if (ot != other.ot)
            {
                return false;
            }
            if (alias != other.alias)
            {
                return false;
            }
            if (setcode != other.setcode)
            {
                return false;
            }
            if (type != other.type)
            {
                return false;
            }
            if (atk != other.atk)
            {
                return false;
            }
            if (def != other.def)
            {
                return false;
            }
            if (level != other.level)
            {
                return false;
            }
            if (race != other.race)
            {
                return false;
            }
            if (attribute != other.attribute)
            {
                return false;
            }
            if (category != other.category)
            {
                return false;
            }
            if (!name.Equals(other.name))
            {
                return false;
            }
            if (!desc.Equals(other.desc))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 比较卡片是否一致？
        /// </summary>
        /// <param name="other">比较的卡片</param>
        /// <returns>结果</returns>
        public bool Equals(Card other)
        {
            if (!EqualsData(other))
            {
                return false;
            }
            if (str.Length != other.str.Length)
            {
                return false;
            }
            for (int i = 0; i < str.Length; i++)
            {
                if (!str[i].Equals(other.str[i]))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 得到哈希值
        /// </summary>
        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            int hashCode = id.GetHashCode() + name.GetHashCode();
            return hashCode;//member.GetHashCode();
        }
        /// <summary>
        /// 比较卡片是否相等
        /// </summary>
        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// 是否是某类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(CardType type)
        {
            if ((this.type & (long)type) == (long)type)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 是否是某系列
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public bool IsSetcode(long sc)
        {
            long settype = sc & 0x0fffL;
            long setsubtype = sc & 0xf000L;
            for (int i = 0; i < SETCODE_SIZE; i++)
            {
                long section = (setcode >> (16 * i)) & 0xffffL;
                if ((section & 0x0fffL) == settype && (section & setsubtype) == setsubtype)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 卡片是否不相等
        /// </summary>
        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }
        #endregion

        #region 卡片文字信息
        /// <summary>
        /// 密码字符串
        /// </summary>
        public string IdString => id.ToString("00000000");
        string LevelString => $"[★{GetLevel()}]";
        /// <summary>
        /// 字符串化
        /// </summary>
        public override string ToString()
        {
            return $"{name} [{IdString}]";
        }
        public string ToDisplayString()
        {
            string result;
            if (IsType(CardType.TYPE_MONSTER))
            {
                result = $"{name} [{IdString}]\n"
                    + $"[{YGOUtil.GetTypeString(type)}] {YGOUtil.GetRace(race)}/{YGOUtil.GetAttribute(attribute)}\n"
                    + $"{LevelString} {atk}/{def}\n"
                    + desc;
            }
            else
            {
                result = $"{name} [{IdString}]\n[{YGOUtil.GetTypeString(type)}]\n{desc}";
            }
            return result;
        }
        #endregion
    }

}

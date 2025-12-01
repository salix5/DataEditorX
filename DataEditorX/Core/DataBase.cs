/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 5月18 星期日
 * 时间: 17:01
 * 
 */
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using DataEditorX.Common;

namespace DataEditorX.Core
{
    /// <summary>
    /// SQLite 操作
    /// </summary>
    public static class DataBase
    {
        #region 默认
        static readonly string DefaultSQL =
            "SELECT id,datas.ot,datas.alias,datas.setcode,datas.type,datas.atk,datas.def,datas.level,datas.race,datas.attribute,datas.category,"
            + "texts.name,texts.desc,texts.str1,texts.str2,texts.str3,texts.str4,texts.str5,texts.str6,texts.str7,texts.str8,texts.str9,texts.str10,"
            + "texts.str11,texts.str12,texts.str13,texts.str14,texts.str15,texts.str16 FROM datas JOIN texts USING(id) WHERE 1 = 1";
        static readonly string DefaultTableSQL =
            "CREATE TABLE datas(id INTEGER PRIMARY KEY, ot INTEGER, alias INTEGER, setcode INTEGER, type INTEGER, atk INTEGER, def INTEGER, level INTEGER, race INTEGER, attribute INTEGER, category INTEGER);"
            + "CREATE TABLE texts(id INTEGER PRIMARY KEY, name TEXT, desc TEXT, str1 TEXT, str2 TEXT, str3 TEXT, str4 TEXT, str5 TEXT, str6 TEXT, str7 TEXT, str8 TEXT, str9 TEXT, str10 TEXT,"
            + " str11 TEXT, str12 TEXT, str13 TEXT, str14 TEXT, str15 TEXT, str16 TEXT);";

        static readonly string InsertDatas =
            " INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category) "
            + "VALUES(@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category);";
        static readonly string InsertTexts =
            " INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16) "
            + "VALUES(@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16);";
        static readonly string InsertReplaceSQL = $"INSERT OR REPLACE{InsertDatas}INSERT OR REPLACE{InsertTexts}";
        static readonly string InsertIgnoreSQL = $"INSERT OR IGNORE{InsertDatas}INSERT OR IGNORE{InsertTexts}";
        static readonly string UpdateSQL =
            "UPDATE datas SET ot=@ot, alias=@alias, setcode=@setcode, type=@type, atk=@atk, def=@def, level=@level, race=@race, attribute=@attribute, category=@category WHERE id=@id;"
            + "UPDATE texts SET name=@name, desc=@desc, str1=@str1, str2=@str2, str3=@str3, str4=@str4, str5=@str5, str6=@str6, str7=@str7, str8=@str8, str9=@str9, str10=@str10,"
            + " str11=@str11, str12=@str12, str13=@str13, str14=@str14, str15=@str15, str16=@str16 WHERE id=@id;";
        static readonly string DeleteSQL =
            "DELETE FROM datas WHERE id=@id;DELETE FROM texts WHERE id=@id;";

        static DataBase()
        {
        }
        #endregion

        #region 创建数据库
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="Db">新数据库路径</param>
        public static bool Create(string Db)
        {
            if (File.Exists(Db))
            {
                File.Delete(Db);
            }

            try
            {
                SQLiteConnection.CreateFile(Db);
                Command(Db, DefaultTableSQL);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool CheckTable(string db)
        {
            try
            {
                Command(db, DefaultTableSQL);
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 执行sql语句
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="DB">数据库</param>
        /// <param name="SQLs">sql语句</param>
        /// <returns>返回影响行数</returns>
        public static int Command(string DB, params string[] SQLs)
        {
            int result = 0;
            if (File.Exists(DB) && SQLs != null)
            {
                using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB}"))
                {
                    con.Open();
                    using (SQLiteTransaction trans = con.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(con))
                            {
                                foreach (string SQLstr in SQLs)
                                {
                                    cmd.CommandText = SQLstr;
                                    result += cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch
                        {
                            trans.Rollback();//出错，回滚
                            result = -1;
                        }
                        finally
                        {
                            trans.Commit();
                        }
                    }
                    con.Close();
                }
            }
            return result;
        }
        #endregion

        #region 根据SQL读取
        static Card ReadCard(SQLiteDataReader reader)
        {
            Card c = new Card(0)
            {
                id = reader.GetInt64(reader.GetOrdinal("id")),
                ot = reader.GetInt64(reader.GetOrdinal("ot")),
                alias = reader.GetInt64(reader.GetOrdinal("alias")),
                setcode = reader.GetInt64(reader.GetOrdinal("setcode")),
                type = reader.GetInt64(reader.GetOrdinal("type")),
                atk = reader.GetInt64(reader.GetOrdinal("atk")),
                def = reader.GetInt64(reader.GetOrdinal("def")),
                level = reader.GetInt64(reader.GetOrdinal("level")),
                race = reader.GetInt64(reader.GetOrdinal("race")),
                attribute = reader.GetInt64(reader.GetOrdinal("attribute")),
                category = reader.GetInt64(reader.GetOrdinal("category")),

                name = (string)reader["name"],
                desc = MyUtils.ConvertNewline((string)reader["desc"], true),
            };

            for (int i = 0; i < c.Str.Length; i++)
            {
                c.Str[i] = (string)reader[$"str{i + 1}"] ?? "";
            }
            return c;
        }

        /// <summary>
        /// 根据密码集合读取数据
        /// </summary>
        /// <param name="DB">数据库</param>
        /// <param name="ids">密码集合</param>
        public static Card[] ReadFromId(string DB, string[] ids)
        {
            string stmt1 = $"{DefaultSQL} AND id IN ({string.Join(",", ids)}) ORDER BY id";
            return Read(DB, stmt1);
        }
       
        public static Card[] Read(string DB, string SQL)
        {
            List<Card> list = new List<Card>();
            if (File.Exists(DB) && SQL != null)
            {
                using (SQLiteConnection sqliteconn = new SQLiteConnection($"Data Source={DB}"))
                {
                    sqliteconn.Open();
                    using (SQLiteTransaction trans = sqliteconn.BeginTransaction())
                    {
                        using (SQLiteCommand sqlitecommand = new SQLiteCommand(sqliteconn))
                        {
                            string stmt1;
                            if (SQL.StartsWith("SELECT", System.StringComparison.OrdinalIgnoreCase))
                            {
                                stmt1 = SQL;
                            }
                            else
                            {
                                stmt1 = DefaultSQL;
                            }

                            sqlitecommand.CommandText = stmt1;
                            using (SQLiteDataReader reader = sqlitecommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReadCard(reader));
                                }
                                reader.Close();
                            }
                        }
                        trans.Commit();
                    }
                    sqliteconn.Close();
                }
            }
            if (list.Count == 0)
            {
                return null;
            }

            return list.ToArray();
        }
        #endregion

        #region SQL parameters
        public static void InitParameters(SQLiteCommand cmd)
        {
            cmd.Parameters.Add("@id", System.Data.DbType.Int64);
            cmd.Parameters.Add("@ot", System.Data.DbType.Int64);
            cmd.Parameters.Add("@alias", System.Data.DbType.Int64);
            cmd.Parameters.Add("@setcode", System.Data.DbType.Int64);
            cmd.Parameters.Add("@type", System.Data.DbType.Int64);
            cmd.Parameters.Add("@atk", System.Data.DbType.Int64);
            cmd.Parameters.Add("@def", System.Data.DbType.Int64);
            cmd.Parameters.Add("@level", System.Data.DbType.Int64);
            cmd.Parameters.Add("@race", System.Data.DbType.Int64);
            cmd.Parameters.Add("@attribute", System.Data.DbType.Int64);
            cmd.Parameters.Add("@category", System.Data.DbType.Int64);
            cmd.Parameters.Add("@name", System.Data.DbType.String);
            cmd.Parameters.Add("@desc", System.Data.DbType.String);
            for (int i = 0; i < Card.STR_SIZE; i++)
            {
                cmd.Parameters.Add($"@str{i + 1}", System.Data.DbType.String);
            }
        }

        public static void AddParameters(SQLiteCommand cmd, Card c)
        {
            cmd.Parameters["@id"].Value = c.id;
            cmd.Parameters["@ot"].Value = c.ot;
            cmd.Parameters["@alias"].Value = c.alias;
            cmd.Parameters["@setcode"].Value = c.setcode;
            cmd.Parameters["@type"].Value = c.type;
            cmd.Parameters["@atk"].Value = c.atk;
            cmd.Parameters["@def"].Value = c.def;
            cmd.Parameters["@level"].Value = c.level;
            cmd.Parameters["@race"].Value = c.race;
            cmd.Parameters["@attribute"].Value = c.attribute;
            cmd.Parameters["@category"].Value = c.category;
            cmd.Parameters["@name"].Value = c.name;
            cmd.Parameters["@desc"].Value = c.NormalizedDesc();
            for (int i = 0; i < c.Str.Length; i++)
            {
                cmd.Parameters[$"@str{i + 1}"].Value = c.Str[i];
            }
        }
        #endregion

        #region 复制数据库
        /// <summary>
        /// 复制数据库
        /// </summary>
        /// <param name="DB">复制到的数据库</param>
        /// <param name="cards">卡片集合</param>
        /// <param name="ignore">是否忽略存在</param>
        /// <returns>更新数x2</returns>
        public static int CopyDB(string DB, bool ignore, params Card[] cards)
        {
            int result = 0;
            if (File.Exists(DB) && cards != null)
            {
                using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + DB))
                {
                    con.Open();
                    using (SQLiteTransaction trans = con.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            foreach (Card c in cards)
                            {
                                cmd.CommandText = GetInsertSQL(c, ignore);
                                result += cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    con.Close();
                }
            }
            return result;
        }
        #endregion

        #region 删除记录
        public static int DeleteDB(string DB, params Card[] cards)
        {
            int result = 0;
            if (File.Exists(DB) && cards != null)
            {
                using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB}"))
                {
                    con.Open();
                    using (SQLiteTransaction trans = con.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = DeleteSQL;
                            var parameter = cmd.Parameters.Add("@id", System.Data.DbType.Int64);
                            foreach (Card c in cards)
                            {
                                parameter.Value = c.id;
                                result += cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    con.Close();
                }
            }
            return result;
        }
        #endregion

        #region 压缩数据库
        public static void Compression(string db)
        {
            if (File.Exists(db))
            {
                using (SQLiteConnection con = new SQLiteConnection($"Data Source={db}"))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "VACUUM;";
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }

        }
        #endregion

        #region SQL语句
        #region 查询
        public static string GetSelectSQL(Card c)
        {
            StringBuilder sb = new StringBuilder(DefaultSQL);
            if (c == null)
            {
                return sb.ToString();
            }

            if (!string.IsNullOrEmpty(c.name))
            {
                if (c.name.IndexOf("%%") >= 0)
                {
                    c.name = c.name.Replace("%%", "%");
                }
                else
                {
                    c.name = "%" + c.name.Replace("%", "/%").Replace("_", "/_") + "%";
                }

                sb.Append(" AND texts.name LIKE '" + c.name.Replace("'", "''") + "' ");
            }
            if (!string.IsNullOrEmpty(c.desc))
            {
                sb.Append($" AND texts.desc LIKE '%{c.NormalizedDesc().Replace("'", "''")}%'");
            }

            if (c.ot > 0)
            {
                sb.Append($" AND datas.ot = {c.ot}");
            }

            if (c.attribute > 0)
            {
                sb.Append($" AND datas.attribute = {c.attribute}");
            }

            if (c.GetLevel() > 0)
            {
                sb.Append($" AND (datas.level & 0xffff) = {c.GetLevel()}");
            }

            if ((c.level & 0xff000000) > 0)
            {
                sb.Append($" AND (datas.level & 0xff000000) = {c.level & 0xff000000}");
            }

            if ((c.level & 0xff0000) > 0)
            {
                sb.Append($" AND (datas.level & 0xff0000) = {c.level & 0xff0000}");
            }

            if (c.race > 0)
            {
                sb.Append($" AND datas.race = {c.race}");
            }

            if (c.type > 0)
            {
                sb.Append($" AND datas.type & {c.type} = {c.type}");
            }

            if (c.category > 0)
            {
                sb.Append($" AND datas.category & {c.category} = {c.category}");
            }

            if (c.atk == -1)
            {
                sb.Append($" AND datas.type & 0x1 = 0x1 AND datas.atk = 0");
            }
            else if (c.atk < 0 || c.atk > 0)
            {
                sb.Append($" AND datas.atk = {c.atk}");
            }

            if (c.IsType(Info.CardType.TYPE_LINK))
            {
                sb.Append($" AND datas.def & {c.def} = {c.def}");
            }
            else
            {
                if (c.def == -1)
                {
                    sb.Append(" AND datas.type & 0x1 = 0x1 AND datas.def = 0");
                }
                else if (c.def < 0 || c.def > 0)
                {
                    sb.Append($" AND datas.def = {c.def}");
                }
            }

            if (c.id > 0 && c.alias > 0)
            {
                sb.Append($" AND datas.id BETWEEN {c.alias} AND {c.id}");
            }
            else if (c.id > 0)
            {
                sb.Append($" AND (datas.id={c.id} OR datas.alias={c.id}) ");
            }
            else if (c.alias > 0)
            {
                sb.Append($" AND datas.alias={c.alias}");
            }
            sb.Append(" ORDER BY id");

            return sb.ToString();

        }
        #endregion

        #region 插入
        /// <summary>
        /// 转换为插入语句
        /// </summary>
        /// <param name="c">卡片数据</param>
        /// <param name="ignore"></param>
        /// <returns>SQL语句</returns>
        public static string GetInsertSQL(Card c, bool ignore, bool hex = false)
        {
            string insertMode = ignore ? "INSERT OR IGNORE" : "INSERT OR REPLACE";
            string setcode = c.setcode.ToString();
            string type = hex ? $"0x{c.type:x}" : c.type.ToString();
            string level = hex ? $"0x{c.level:x}" : c.level.ToString();
            string race = hex ? $"0x{c.race:x}" : c.race.ToString();
            string attribute = hex ? $"0x{c.attribute:x}" : c.attribute.ToString();
            string category = hex ? $"0x{c.category:x}" : c.category.ToString();

            string name = c.name.Replace("'", "''");
            string desc = c.NormalizedDesc().Replace("'", "''");

            string[] strs = new string[c.Str.Length];
            for (int i = 0; i < c.Str.Length; i++)
            {
                strs[i] = c.Str[i].Replace("'", "''");
            }

            string stmt_datas =
                $"{insertMode} INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category) "
                + $"VALUES({c.id},{c.ot},{c.alias},{setcode},{type},{c.atk},{c.def},{level},{race},{attribute},{category});\n";

            string stmt_texts =
                $"{insertMode} INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16) "
                + $"VALUES({c.id},'{name}','{desc}','{string.Join("','", strs)}');\n";

            return $"{stmt_datas}{stmt_texts}";
        }
        #endregion

        #region 更新
        /// <summary>
        /// 转换为更新语句
        /// </summary>
        /// <param name="c">卡片数据</param>
        /// <returns>SQL语句</returns>
        public static string GetUpdateSQL(Card c)
        {
            string[] strEscaped = new string[c.Str.Length];
            for (int i = 0; i < c.Str.Length; i++)
            {
                strEscaped[i] = c.Str[i].Replace("'", "''");
            }

            string stmt_datas =
                $"UPDATE datas SET ot={c.ot},alias={c.alias},setcode={c.setcode},type={c.type},atk={c.atk},def={c.def},level={c.level},race={c.race},attribute={c.attribute},category={c.category} WHERE id={c.id};\n";

            string name = c.name.Replace("'", "''");
            string desc = c.NormalizedDesc().Replace("'", "''");
            string[] strAssignments = new string[c.Str.Length];
            for (int i = 0; i < c.Str.Length; i++)
            {
                strAssignments[i] = $"str{i + 1}='{strEscaped[i]}'";
            }
            string stmt_texts = $"UPDATE texts SET name='{name}',desc='{desc}', {string.Join(",", strAssignments)} WHERE id={c.id};\n";

            return $"{stmt_datas}{stmt_texts}";
        }
        #endregion

        #region 删除
        /// <summary>
        /// 转换删除语句
        /// </summary>
        /// <param name="c">卡片密码</param>
        /// <returns>SQL语句</returns>
        public static string GetDeleteSQL(Card c)
        {
            string id = c.id.ToString();
            return $"DELETE FROM datas WHERE id={id};\nDELETE FROM texts WHERE id={id};\n";
        }
        #endregion
        #endregion


        public static void ExportSQL(string file, params Card[] cards)
        {
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                foreach (Card c in cards)
                {
                    sw.WriteLine(GetInsertSQL(c, false, true));
                }
                sw.Close();
            }
        }

        public static CardPack FindPack(string db, long id)
        {
            CardPack cardpack = null;
            if (File.Exists(db) && id >= 0)
            {
                using (SQLiteConnection sqliteconn = new SQLiteConnection(@"Data Source=" + db))
                {
                    sqliteconn.Open();
                    using (SQLiteCommand sqlitecommand = new SQLiteCommand(sqliteconn))
                    {
                        sqlitecommand.CommandText = "select id,pack_id,pack,rarity,date from pack where id=" + id + " order by date desc";
                        using (SQLiteDataReader reader = sqlitecommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cardpack = new CardPack(id)
                                {
                                    pack_id = reader.GetString(1),
                                    pack_name = reader.GetString(2),
                                    rarity = reader.GetString(3),
                                    date = reader.GetString(4)
                                };
                            }
                            reader.Close();
                        }
                    }
                    sqliteconn.Close();
                }
            }
            return cardpack;
        }
    }
}

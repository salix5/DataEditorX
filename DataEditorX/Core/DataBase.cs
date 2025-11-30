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
        static readonly string _defaultSQL;
        static readonly string _defaultTableSQL;

        static DataBase()
        {
            _defaultSQL =
                "SELECT datas.*,texts.* FROM datas,texts WHERE datas.id=texts.id ";
            StringBuilder st = new StringBuilder();
            st.Append(@"CREATE TABLE texts(id integer primary key,name text,desc text");
            for (int i = 1; i <= 16; i++)
            {
                st.Append(",str");
                st.Append(i.ToString());
                st.Append(" text");
            }
            st.Append(");");
            st.Append(@"CREATE TABLE datas(");
            st.Append("id integer primary key,ot integer,alias integer,");
            st.Append("setcode integer,type integer,atk integer,def integer,");
            st.Append("level integer,race integer,attribute integer,category integer) ");
            _defaultTableSQL = st.ToString();
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
                Command(Db, _defaultTableSQL);
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
                Command(db, _defaultTableSQL);
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
                using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + DB))
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

        public static Card[] Read(string DB, params long[] ids)
        {
            List<string> idlist = new List<string>();
            foreach (long id in ids)
            {
                idlist.Add(id.ToString());
            }
            return Read(DB, idlist.ToArray());
        }
        /// <summary>
        /// 根据密码集合，读取数据
        /// </summary>
        /// <param name="DB">数据库</param>
        /// <param name="SQLs">SQL/密码语句集合集合</param>
        public static Card[] Read(string DB, params string[] SQLs)
        {
            List<Card> list = new List<Card>();
            List<long> idlist = new List<long>();
            if (File.Exists(DB) && SQLs != null)
            {
                using (SQLiteConnection sqliteconn = new SQLiteConnection($"Data Source={DB}"))
                {
                    sqliteconn.Open();
                    using (SQLiteTransaction trans = sqliteconn.BeginTransaction())
                    {
                        using (SQLiteCommand sqlitecommand = new SQLiteCommand(sqliteconn))
                        {
                            foreach (string str in SQLs)
                            {
                                int.TryParse(str, out int tmp);

                                string SQLstr;
                                if (string.IsNullOrEmpty(str))
                                {
                                    SQLstr = _defaultSQL;
                                }
                                else if (tmp > 0)
                                {
                                    SQLstr = $"{_defaultSQL} AND datas.id={str}";
                                }
                                else if (str.StartsWith("SELECT", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    SQLstr = str;
                                }
                                else
                                {
                                    SQLstr = _defaultSQL + " and texts.name like '%" + str + "%'";
                                }

                                sqlitecommand.CommandText = SQLstr;
                                using (SQLiteDataReader reader = sqlitecommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        Card c = ReadCard(reader);
                                        if (idlist.IndexOf(c.id) < 0)
                                        {//不存在，则添加
                                            idlist.Add(c.id);
                                            list.Add(c);
                                        }
                                    }
                                    reader.Close();
                                }
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
                using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + DB))
                {
                    con.Open();
                    using (SQLiteTransaction trans = con.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            foreach (Card c in cards)
                            {
                                cmd.CommandText = GetDeleteSQL(c);
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
                using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + db))
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
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM datas,texts WHERE datas.id=texts.id ");
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
                $"{insertMode} INTO datas VALUES({c.id},{c.ot},{c.alias},{setcode},{type},{c.atk},{c.def},{level},{race},{attribute},{category});\n";

            string stmt_texts =
                $"{insertMode} INTO texts VALUES({c.id},'{name}','{desc}','{string.Join("','", strs)}');\n";

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


        public static void ExportSql(string file, params Card[] cards)
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

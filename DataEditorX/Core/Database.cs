/*
 * 由SharpDevelop创建。
 * 用户： Acer
 * 日期: 5月18 星期日
 * 时间: 17:01
 * 
 */
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using DataEditorX.Common;

namespace DataEditorX.Core
{
    /// <summary>
    /// SQLite 操作
    /// </summary>
    public static class Database
    {
        #region SQL statements
        public const string DefaultSQL =
            "SELECT id,datas.ot,datas.alias,datas.setcode,datas.type,datas.atk,datas.def,datas.level,datas.race,datas.attribute,datas.category,"
            + "texts.name,texts.desc,texts.str1,texts.str2,texts.str3,texts.str4,texts.str5,texts.str6,texts.str7,texts.str8,texts.str9,texts.str10,"
            + "texts.str11,texts.str12,texts.str13,texts.str14,texts.str15,texts.str16 FROM datas JOIN texts USING(id) WHERE 1 = 1";
        public const string DefaultTableSQL =
            "CREATE TABLE datas(id INTEGER PRIMARY KEY, ot INTEGER, alias INTEGER, setcode INTEGER, type INTEGER, atk INTEGER, def INTEGER, level INTEGER, race INTEGER, attribute INTEGER, category INTEGER);\n"
            + "CREATE TABLE texts(id INTEGER PRIMARY KEY, name TEXT, desc TEXT, str1 TEXT, str2 TEXT, str3 TEXT, str4 TEXT, str5 TEXT, str6 TEXT, str7 TEXT, str8 TEXT, str9 TEXT, str10 TEXT,"
            + " str11 TEXT, str12 TEXT, str13 TEXT, str14 TEXT, str15 TEXT, str16 TEXT);";

        const string InsertDatas =
            " INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category) "
            + "VALUES (@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category);";
        const string InsertTexts =
            " INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16) "
            + "VALUES (@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16);";
        const string InsertReplaceSQL = "INSERT OR REPLACE" + InsertDatas + "INSERT OR REPLACE" + InsertTexts;
        const string InsertIgnoreSQL = "INSERT OR IGNORE" + InsertDatas + "INSERT OR IGNORE" + InsertTexts;
        const string UpdateDatas =
            "ot=@ot, alias=@alias, setcode=@setcode, type=@type, atk=@atk, def=@def, level=@level, race=@race, attribute=@attribute, category=@category";
        const string UpdateTexts =
            "name=@name, desc=@desc, str1=@str1, str2=@str2, str3=@str3, str4=@str4, str5=@str5, str6=@str6, str7=@str7, str8=@str8, str9=@str9, str10=@str10,"
            + " str11=@str11, str12=@str12, str13=@str13, str14=@str14, str15=@str15, str16=@str16";
        const string UpdateSQL =
            "UPDATE OR IGNORE datas SET " + UpdateDatas + " WHERE id=@id; UPDATE OR IGNORE texts SET " + UpdateTexts + " WHERE id=@id;";
        const string MoveSQL =
            "UPDATE OR IGNORE datas SET id=@id, " + UpdateDatas + " WHERE id=@old_id; UPDATE OR IGNORE texts SET id=@id, " + UpdateTexts + " WHERE id=@old_id;";
        const string DeleteSQL =
            "DELETE FROM datas WHERE id=@id;DELETE FROM texts WHERE id=@id;";
        const string PragmaSQL = "PRAGMA trusted_schema = OFF;";
        public static readonly Card EmptyCard = new(0);
        #endregion

        #region Create table
        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="db">New database file path</param>
        public static bool CreateDatabase(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                return false;
            }

            if (File.Exists(db))
            {
                File.Delete(db);
            }
            SQLiteConnection.CreateFile(db);
            return Command(db, DefaultTableSQL) >= 0;
        }
        public static bool CheckTable(string db)
        {
            return Command(db, DefaultTableSQL) >= 0;
        }
        #endregion

        #region Execute SQL
        /// <summary>
        /// Execute SQL statements.
        /// </summary>
        /// <param name="db">Database file path</param>
        /// <param name="SQLs">SQL statements</param>
        /// <returns>Number of affected rows</returns>
        public static int Command(string db, params string[] SQLs)
        {
            if (SQLs == null || SQLs.Length == 0)
            {
                return 0;
            }
            if (!File.Exists(db))
            {
                return -1;
            }

            int result = 0;
            using SQLiteConnection con = new($"Data Source={db}");
            con.Open();
            using (SQLiteTransaction trans = con.BeginTransaction())
            {
                try
                {
                    using SQLiteCommand cmd = new(PragmaSQL, con, trans);
                    cmd.ExecuteNonQuery();
                    foreach (string SQLstr in SQLs)
                    {
                        cmd.CommandText = SQLstr;
                        result += cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch (Exception rbEx)
                    {
                        Trace.TraceError($"Database.Command rollback failed on '{db}': {rbEx}");
                    }
                    Trace.TraceError($"Database.Command failed on '{db}': {ex}");
                    result = -1;
                }
            }
            return result;
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
            cmd.Parameters["@desc"].Value = c.desc;
            for (int i = 0; i < c.Str.Length; i++)
            {
                cmd.Parameters[$"@str{i + 1}"].Value = c.Str[i];
            }
        }
        #endregion

        #region Read database
        static Card ReadCard(SQLiteDataReader reader)
        {
            Card c = new(0)
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
                desc = MyUtils.ConvertNewline((string)reader["desc"], false),
            };

            for (int i = 0; i < c.Str.Length; i++)
            {
                c.Str[i] = (string)reader[$"str{i + 1}"] ?? "";
            }
            return c;
        }

        /// <summary>
        /// Read cards from database by IDs.
        /// </summary>
        /// <param name="db">Database file path</param>
        /// <param name="ids">Collection of IDs</param>
        public static Card[] ReadFromId(string db, long[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Array.Empty<Card>();
            }
            string idCondition = $" AND id IN ({string.Join(",", ids)})";
            return Read(db, idCondition);
        }

        public static Card[] Read(string db, string queryCondition)
        {
            if (!File.Exists(db))
            {
                return Array.Empty<Card>();
            }
            List<Card> list = new();
            using SQLiteConnection sqliteconn = new($"Data Source={db}");
            sqliteconn.Open();
            using (SQLiteTransaction trans = sqliteconn.BeginTransaction())
            {
                using SQLiteCommand cmd = new(PragmaSQL, sqliteconn, trans);
                cmd.ExecuteNonQuery();
                cmd.CommandText = string.IsNullOrEmpty(queryCondition) ?
                    $"{DefaultSQL} ORDER BY id;" :
                    $"{DefaultSQL}{queryCondition} ORDER BY id;";
                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadCard(reader));
                }
                trans.Commit();
            }
            return list.ToArray();
        }
        #endregion

        #region Write database
        /// <summary>
        /// Insert cards into database
        /// </summary>
        /// <param name="db">Database file path</param>
        /// <param name="ignore">Ignore existing entries</param>
        /// <param name="cards">Collection of cards</param>
        /// <returns>Number of updated cards * 2</returns>
        public static int InsertCards(string db, bool ignore, Card[] cards)
        {
            if (cards == null || cards.Length == 0)
            {
                return 0;
            }
            if (!File.Exists(db))
            {
                return 0;
            }
            int result = 0;
            using SQLiteConnection con = new($"Data Source={db}");
            con.Open();
            using (SQLiteTransaction trans = con.BeginTransaction())
            {
                using SQLiteCommand cmd = new(PragmaSQL, con, trans);
                cmd.ExecuteNonQuery();
                cmd.CommandText = ignore ? InsertIgnoreSQL : InsertReplaceSQL;
                InitParameters(cmd);
                foreach (Card c in cards)
                {
                    AddParameters(cmd, c);
                    result += cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            return result;
        }

        public static int DeleteCards(string db, Card[] cards)
        {
            if (cards == null || cards.Length == 0)
            {
                return 0;
            }
            if (!File.Exists(db))
            {
                return 0;
            }
            int result = 0;
            using SQLiteConnection con = new($"Data Source={db}");
            con.Open();
            using (SQLiteTransaction trans = con.BeginTransaction())
            {
                using SQLiteCommand cmd = new(PragmaSQL, con, trans);
                cmd.ExecuteNonQuery();
                cmd.CommandText = DeleteSQL;
                var parameter = cmd.Parameters.Add("@id", System.Data.DbType.Int64);
                foreach (Card c in cards)
                {
                    parameter.Value = c.id;
                    result += cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            return result;
        }

        public static bool UpdateCard(string db, Card c, long oldId)
        {
            if (c is null || oldId < 0)
            {
                return false;
            }
            if (!File.Exists(db))
            {
                return false;
            }
            int result = 0;
            using SQLiteConnection con = new($"Data Source={db}");
            con.Open();
            using (SQLiteTransaction trans = con.BeginTransaction())
            {
                using SQLiteCommand cmd = new(PragmaSQL, con, trans);
                cmd.ExecuteNonQuery();
                cmd.CommandText = oldId == 0 ? UpdateSQL : MoveSQL;
                InitParameters(cmd);
                AddParameters(cmd, c);
                if (oldId != 0)
                {
                    cmd.Parameters.Add("@old_id", System.Data.DbType.Int64).Value = oldId;
                }
                result += cmd.ExecuteNonQuery();
                trans.Commit();
            }
            return result > 0;
        }

        public static bool AddCard(string db, Card c) => InsertCards(db, true, new Card[] { c }) == 2;
        public static bool RemoveCard(string db, Card c) => DeleteCards(db, new Card[] { c }) == 2;
        #endregion

        #region VACUUM
        public static void Vacuum(string db)
        {
            if (!File.Exists(db))
            {
                return;
            }
            using SQLiteConnection con = new($"Data Source={db}");
            con.Open();
            using SQLiteCommand cmd = new("VACUUM;", con);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region SELECT
        public static string GetSelectCondition(Card c)
        {
            if (c is null || EmptyCard.Equals(c))
            {
                return "";
            }

            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(c.name))
            {
                string escapedName = c.name.Replace("%", "$%").Replace("_", "$_");
                sb.Append($" AND texts.name LIKE '%{escapedName.Replace("'", "''")}%' ESCAPE '$'");
            }
            if (!string.IsNullOrEmpty(c.desc))
            {
                sb.Append($" AND texts.desc LIKE '%{c.desc.Replace("'", "''")}%'");
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

            if (c.GetLeftScale() > 0)
            {
                sb.Append($" AND (datas.level >> 24 & 0xff) = {c.GetLeftScale()}");
            }

            if (c.GetRightScale() > 0)
            {
                sb.Append($" AND (datas.level >> 16 & 0xff) = {c.GetRightScale()}");
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

        #region INSERT
        /// <summary>
        /// Generate INSERT statements.
        /// </summary>
        /// <param name="c">Card data</param>
        /// <param name="ignore">Ignore existing entries</param>
        /// <returns>SQL statements</returns>
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
            string desc = c.desc.Replace("'", "''");

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

        #region UPDATE
        /// <summary>
        /// Generate UPDATE statements.
        /// </summary>
        /// <param name="c">Card data</param>
        /// <returns>SQL statements</returns>
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
            string desc = c.desc.Replace("'", "''");
            string[] strAssignments = new string[c.Str.Length];
            for (int i = 0; i < c.Str.Length; i++)
            {
                strAssignments[i] = $"str{i + 1}='{strEscaped[i]}'";
            }
            string stmt_texts = $"UPDATE texts SET name='{name}',desc='{desc}', {string.Join(",", strAssignments)} WHERE id={c.id};\n";

            return $"{stmt_datas}{stmt_texts}";
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Generate DELETE statements.
        /// </summary>
        /// <param name="c">Card ID</param>
        /// <returns>SQL statements</returns>
        public static string GetDeleteSQL(Card c)
        {
            string id = c.id.ToString();
            return $"DELETE FROM datas WHERE id={id};\nDELETE FROM texts WHERE id={id};\n";
        }
        #endregion


        public static void ExportSQL(string file, Card[] cards)
        {
            using FileStream fs = new(file, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new(fs, Encoding.UTF8);
            foreach (Card c in cards)
            {
                sw.WriteLine(GetInsertSQL(c, false, true));
            }
            sw.Close();
        }

        public static CardPack FindPack(string db, long id)
        {
            if (id < 0 || !File.Exists(db))
            {
                return new CardPack(0);
            }

            CardPack cardpack = new(0);
            using SQLiteConnection sqliteconn = new($"Data Source={db}");
            sqliteconn.Open();
            using SQLiteCommand cmd = new(PragmaSQL, sqliteconn);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT id,pack_id,pack,rarity,date FROM pack WHERE id=@id ORDER BY date DESC;";
            cmd.Parameters.Add("@id", System.Data.DbType.Int64).Value = id;
            using SQLiteDataReader reader = cmd.ExecuteReader();
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
            return cardpack;
        }
    }
}

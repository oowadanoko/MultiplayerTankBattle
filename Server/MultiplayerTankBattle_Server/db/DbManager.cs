using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Text.Json;


public class DbManager
{
    public static MySqlConnection mysql;

    public static bool Connect(string db, string ip, int port, string user, string pw)
    {
        mysql = new MySqlConnection();
        string s = string
            .Format("Database={0}; Data Source={1}; Port={2}; User Id={3}; Password={4}",
            db, ip, port, user, pw);
        mysql.ConnectionString = s;
        try
        {
            mysql.Open();
            Console.WriteLine("[数据库]connect succ");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库]connect fail " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 检查字符串是否安全（不包含一些特殊字符），防止sql注入攻击
    /// </summary>
    /// <param name="str">待检查字符串</param>
    /// <returns>是否安全</returns>
    public static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    public static bool IsAccountExist(string id)
    {
        if (!DbManager.IsSafeString(id))
        {
            return false;
        }
        string s = string.Format("select * from account where id = '{0}';", id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] IsAccountExist err, " + e.Message);
            return false;
        }
    }

    public static bool Register(string id, string pw)
    {
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[数据库] Register fail, id not safe");
            return false;
        }
        if (!DbManager.IsSafeString(pw))
        {
            Console.WriteLine("[数据库] Register fail, pw not safe");
            return false;
        }
        if (IsAccountExist(id))
        {
            Console.WriteLine("[数据库] Register fail, id exist");
            return false;
        }
        string sql = string.Format("insert into account set id = '{0}', pw = '{1}';", id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] Register fail " + e.Message);
            return false;
        }
    }
    
    /// <summary>
    /// 创建玩家，新注册的账号需要创建一个对应的玩家
    /// </summary>
    /// <param name="id">账号id</param>
    /// <returns>是否成功创建玩家</returns>
    public static bool CreatPlayer(string id)
    {
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[数据库] CreatePlayer fail, id not safe");
            return false;
        }
        PlayerData playerData = new PlayerData();
        string data = JsonSerializer.Serialize(playerData);
        string sql = string.Format("insert into player set id = '{0}', data = '{1}';", id, data);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] CreatePlayer err, " + e.Message);
            return false;
        }
    }

    public static bool CheckPassword(string id, string pw)
    {
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[数据库] CheckPassword fail, id not safe");
            return false;
        }
        if (!DbManager.IsSafeString(pw))
        {
            Console.WriteLine("[数据库] CheckPassword fail, pw not safe");
            return false;
        }
        string sql = string.Format("select * from account where id = '{0}' and pw = '{1}';", id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] CheckPassword err, " + e.Message);
            return false;
        }
    }

    public static PlayerData GetPlayerData(string id)
    {
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[数据库] GetPlayerData fail, id not safe");
            return null;
        }
        string sql = string.Format("select * from player where id = '{0}';", id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return null;
            }
            dataReader.Read();
            string data = dataReader.GetString("data");
            PlayerData playerData = JsonSerializer.Deserialize(data, typeof(PlayerData)) as PlayerData;
            dataReader.Close();
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] GetPlayerData fail, " + e.Message);
            return null;
        }
    }

    public static bool UpdatePlayerData(string id, PlayerData playerData)
    {
        string data = JsonSerializer.Serialize(playerData);
        string sql = string.Format("update player set data = '{0}' where id = '{1}';", data, id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] UpdatePlayerData err, " + e.Message);
            return false;
        }
    }
}


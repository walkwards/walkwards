using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using walkwards_api.Sql;
using walkwards_api.UserManager;
using walkwards_api.Utilities;

namespace walkwards_api
{
    public static class AdminMethods
{
    public static async Task<object> AddColumnInUserData(string fieldname, string type, string tablename)
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.base");

        List<int> readyUser = new List<int>();

        foreach (var user in data)
        {
            try
            {
                await SqlManager.ExecuteNonQuery($"alter table user_{user["id"]}.{tablename} drop column {fieldname};");
                readyUser.Add(user["id"]);
                Console.WriteLine($"Dodano kolumne {fieldname} ({type}) do usera {user["id"]}");
            }
            catch (Exception e)
            {
                foreach (var item in readyUser)
                {
                    await SqlManager.ExecuteNonQuery($"alter table user_{item}.user drop column {fieldname} not null;");
                }

                break;
            }
        }

        return readyUser;
    }
    public static async Task<object> AddTableToUserData(string tablename, JArray fields)
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.base");
        
        List<int> readyUser = new List<int>();

        foreach (var user in data)
        {
            try
            {
                try
                {
                    var isexist = await SqlManager.Reader($"SELECT * FROM user_{user["id"]}.{tablename}");
                    Console.WriteLine(JsonConvert.SerializeObject(isexist));
                }
                catch (Exception e)
                {
                    string sql = $"CREATE TABLE user_{user["id"]}.{tablename} (";

                    for (int i = 0; i != fields.Count; i++)
                    {
                        if (i != fields.Count - 1) sql += fields[i] + ", ";
                        else sql += fields[i];
                    }

                    sql += ");";
                    await SqlManager.ExecuteNonQuery(sql);
                    readyUser.Add(user["id"]);
                    Console.WriteLine($"Dodano tabele {tablename} do usera {user["id"]}");
                }
            }
            catch (Exception e)
            {
                foreach (var item in readyUser)
                {
                    await SqlManager.ExecuteNonQuery($"DROP table user_{item}.{tablename};");
                }

                Console.WriteLine("err");
                break;
            }
        }

        return readyUser;
    }
    
    public static async Task<object> EditValueType(string tablename, string fieldname, string newtype)
    {
        
        //alter table user_130109.user alter column walkcoins type float using walkcoins::float;

        var data = await SqlManager.Reader($"SELECT * FROM base.base");
        
        List<int> readyUser = new List<int>();

        foreach (var user in data)
        {
            try
            {
                string sql = $"alter table user_{user["id"]}.{tablename} alter column {fieldname} type {newtype} using {fieldname}::{newtype}";
                
                await SqlManager.ExecuteNonQuery(sql);
                readyUser.Add(user["id"]);
                await Utilities.LoggerManager.WriteLog($"Set {tablename}.{fieldname} - {newtype}");
            }
            catch (Exception e)
            {
                
                break;
            }
        }

        return readyUser;
    }    
    
    public static async Task<object> DropData(string tablename)
    {
        if (tablename == "user") throw new CustomError("Janek nie ma tak dobrze :)");
        var data = await SqlManager.Reader($"SELECT * FROM base.base");
        
        foreach (var user in data)
        {
            await SqlManager.ExecuteNonQuery($"DELETE FROM user_{user["id"]}.{tablename};");
        }

        return true;
    }
    
    public static async Task<object> SetValueInAllUser(string tablename, string fieldname, string value)
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.base");

        List<int> readyUser = new List<int>();

        foreach (var user in data)
        {
            string sql = $"UPDATE user_{user["id"]}.{tablename} SET {fieldname} = {value};";

            await SqlManager.ExecuteNonQuery(sql);
            readyUser.Add(user["id"]);
            Console.WriteLine($"Ustwainowartość dla usera {user["id"]}");
        }

        return readyUser;
    }
    public static async Task<object> DropTableToUserData(string tablename)
    {
        var data = await SqlManager.Reader($"SELECT * FROM base.base");

        List<int> readyUser = new List<int>();

        foreach (var user in data)
        {
            string sql = $"DROP TABLE user_{user["id"]}.{tablename};";

            await SqlManager.ExecuteNonQuery(sql);
            readyUser.Add(user["id"]);
            Console.WriteLine($"dropnieto tabele '{tablename}' z usera {user["id"]}");
        }

        return readyUser;
    }

    public static async Task<object> DropUser(int id)
    {

        var data = await SqlManager.Reader("SELECT * FROM base.base");

        User user = await UserMethod.GetUserData(id, false, false);

        foreach (var item in data)
        {
            User second = await UserMethod.GetUserData(item["id"], false, false);
            var friends = await user.GetAllFriend(0, int.MaxValue);
            // 
            if (friends.Where(u => u.Id == id).ToList().Count != 0)
            {
                await second.RemoveFriend(id);
            }
        }

        await SqlManager.ExecuteNonQuery(
            $"DROP SCHEMA user_{id} CASCADE;");
        await SqlManager.ExecuteNonQuery(
            $"DELETE FROM base.base WHERE id = {id};");


        return true;
    }

    public static async Task<object> GetToken(int id)
    {
        return await SqlManager.Reader($"SELECT * FROM user_{id}.tokens;");
    }

    public static async Task<object> SendAndroidObject(int id, string obj)
    {
        obj = (await UserMethod.GetUserData(id, false, false)).Username + " - " + obj;
        string file = await File.ReadAllTextAsync("androidObject");
        await File.WriteAllTextAsync("androidObject", file+"\n"+obj);
        return true;
    }
}
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json.Linq;
using walkwards_api.Notifications;
using walkwards_api.Sql;
using walkwards_api.UserManager;
using walkwards_api.Utilities;

namespace walkwards_api.Competition
{
    public static class GuildManager
    {
        private static async Task<List<User>> GetGuildUsers(int gid)
        {
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE gid = {gid};");
            List<User> users = new List<User>();
            for (int i = 0; i < data.Count; i++)
            {
                User user = await UserMethod.GetUserData(data[i]["uid"], false, false);
                users.Add(user);
            }

            return users;
        }
        public static async Task<Guild> GetUserGuild(int id)
        {
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id};");

            if (data.Count == 0) throw new CustomError("UserHaveNotGuild");
            
            return await GetGuild(data[0]["gid"]);
        }
        public static async Task<Guild> GetGuild(int guildId)
        {
            var data = await SqlManager.Reader($"SELECT * FROM guild.guilds WHERE id = {guildId};");

            if (data.Count == 0) throw new CustomError("GuildIsNotExist");

            Guild guild = new Guild(data[0]["id"],
                await GetGuildUsers(guildId), data[0]["name"],
                await UserMethod.GetUserData(data[0]["creator"], false, false), data[0]["canmembersaddmembers"]);

            data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE gid = {guildId};");

            List<User> users = new List<User>();
            foreach (var item in data)
            {
                User user = await UserMethod.GetUserData(item["uid"], false, false);
                
                user.Steps = (await user.GetActivityCurrentDay()).y;

                guild.StepSum += user.Steps;
                
                guild.WalkcoinsSum += (int)user.WalkCoins;

                users.Add(user);
            }

            if (guild.StepSum > 0)
            {
                guild.StepAvg += guild.StepSum / users.Count;
            }
            else
            {
                guild.StepAvg = 0;
            }

            users = users.OrderByDescending(item => item.Steps).ToList();

            guild.Users = users;
            
            
            return guild;
        }
        public static async Task<object> GetAllGuildsRankingSum(int page)
        {
            int resultsPerPage = 21;
            var data = await SqlManager.Reader($"SELECT * FROM guild.guilds;");

            List<Guild> guilds = new();
            foreach (var item in data)
            {
                Guild guild = await GetGuild(item["id"]);
                
                guilds.Add(guild);
            }

            guilds = guilds.OrderByDescending(g => g.StepSum).ThenBy(g => g.Id).ToList();
            
            var result = new List<Guild>();

            for (var i = (page * resultsPerPage); i != (resultsPerPage * page) + resultsPerPage - 1; i++)
            {
                if (i == guilds.Count) break;
                result.Add(guilds[i]);
            }
          
            return result.ToArray();
        }
        public static async Task<object> GetAllGuildsRankingAvg(int page)
        {
            int resultsPerPage = 21;

            var data = await SqlManager.Reader($"SELECT * FROM guild.guilds;");

            List<Guild> guilds = new();
            foreach (var item in data)
            {
                Guild guild = await GetGuild(item["id"]);

                guild.StepSum /= guild.Users.Count;
                
                guilds.Add(guild);
            }

            guilds = guilds.OrderByDescending(g => g.StepSum).ThenBy(g => g.Id).ToList();
            
            var result = new List<Guild>();

            for (var i = (page * resultsPerPage); i != (resultsPerPage * page) + resultsPerPage - 1; i++)
            {
                if (i == guilds.Count) break;
                result.Add(guilds[i]);
            }
          
            return result.ToArray();
        }
        public static async Task<object> CreateGuild(string name, int creator, string avatar, bool canMembersAddMembers)
        {
            var isexist = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {creator};");

            User user = await UserMethod.GetUserData(creator, false, false);
            
            if (isexist.Count > 0) throw new CustomError("UserHaveGuild");
            if (user.WalkCoins < 1000) throw new CustomError("UserHaveNotEnoughMoney");


            await SqlManager.ExecuteNonQuery($"UPDATE user_{creator}.user SET walkcoins = {user.WalkCoins - 1000}");

            var data = await SqlManager.Reader($"SELECT * FROM guild.guilds WHERE name = '{name}';");
            if (data.Count != 0) throw new CustomError("GuildIsExist");

            Random rnd = new Random();
            int id = rnd.Next(100000, 999999);
            Guild guild = new Guild(id, name, await UserMethod.GetUserData(creator, false, false));
            string sql = 
                $"INSERT INTO guild.guilds VALUES({guild.Id}, '{guild.Name}', {guild.Creator.Id}, {canMembersAddMembers});";
            await SqlManager.ExecuteNonQuery(sql);
            await SqlManager.ExecuteNonQuery($"INSERT INTO guild.guildusers VALUES({creator}, {guild.Id}, 1);");
            
            await File.WriteAllBytesAsync($"/home/ubuntu/api/avatars/guild/{guild.Id}.txt", Convert.FromBase64String(avatar)); 
            
            return guild;
        }
        public static async Task<List<Guild>> GuildSearch(string query, int page, int objPerPage = 21)
        {
            List<Guild> guilds= new ();

            var data = await SqlManager.Reader($"SELECT id FROM guild.guilds WHERE lower (name) LIKE lower ('%{query}%');");

            if (data.Count <= 0) return guilds;
            
            List<int> ids = data.Select(item => item["id"]).Cast<int>().ToList();

            int i = 0;
            foreach (var item in ids)
            {
                i++;

                if(i < page * objPerPage) continue;

                var guild = await GetGuild(item);
                
                guilds.Add(guild);
                if(i == (page * objPerPage) + objPerPage -1) break;
            }

            guilds = guilds.OrderByDescending(userActivity => userActivity.WalkcoinsSum).ToList();

            return guilds;
        }
        public static async Task<object> GuildJoinRequest(int id, int gid)
        {
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE uid = {id} AND gid = {gid}")).Count > 0)
                throw new CustomError("InvitesIsExist");
            
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id};")).Count > 0)
                throw new CustomError("UserIsInGuild");
            
            await SqlManager.ExecuteNonQuery($"INSERT INTO guild.guildinvites VALUES ({id}, {gid}, 0, false, false);");

            User user = await UserMethod.GetUserData(id, false, false);
            Guild guild = await GetGuild(gid);

            List<string> adminIds = new();
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE role != 0 AND gid = {guild.Id}");

            foreach (var item in data)
            {
                adminIds.Add(item["uid"].ToString());
            }
            
            await NotificationsManager.SendNotifications(NotificationType.join_request_guild,
                $"Prośba o dołączenie do gildi",
                $"Użytkownik {user.Username} wysłał Ci prośbę o możliwosc dołączenia do gildi {guild.Name}!",
                $"walkwards://showUser/{user.Id}", adminIds.ToArray(), long.Parse(guild.Id.ToString()+id.ToString()));
            return true;
        }
        public static async Task<object> AnswerGuildJoinRequest(int gid, int uid, int id, bool accepted)
        {
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id} AND gid = {gid};");
            if (data.Count == 0) throw new CustomError("UserIsNotAminOfGuild");
            if (data[0]["role"] == 0) throw new CustomError("UserIsNotAminOfGuild");
            
            Guild guild = await GetGuild(gid);
            if (accepted)
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO guild.guildusers VALUES({uid}, {gid}, 0);");
                await NotificationsManager.SendNotifications(NotificationType.accepted_invite_to_guild,
                    $"Zaakceptowano twoje prośbę",
                    $"Aministrator Gildi {(await UserMethod.GetUserData(id, false, false)).Username} Zaakceptował twoją prośbę od dołączenie do gildi {guild.Name}!",
                    $"walkwards://showGuild/{guild.Id}", new[] {uid.ToString()}, guild.Id);
                
                
            }
            else
            {
                await NotificationsManager.SendNotifications(NotificationType.reject_invite_to_guild,
                    $"Zaakceptowano twoje prośbę",
                    $"Aministrator gildi {(await UserMethod.GetUserData(id, false, false)).Username} Odrzucił twoją prośbę od dołączenie do gildi {guild.Name}!",
                    $"walkwards://showGuild/{guild.Id}", new[] {uid.ToString()}, guild.Id);
            }
            
            data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE role != 0 AND gid = {guild.Id}");

            foreach (var item in data)
            {
                await SqlManager.ExecuteNonQuery($"DELETE FROM user_{item["uid"]}.notifications WHERE type = 15 AND objid = {gid.ToString()+uid.ToString()};");
            }
            
            await SqlManager.ExecuteNonQuery($"DELETE FROM guild.guildinvites WHERE uid = {uid};");

            return true;
        }
        public static async Task<object> GetGuildJoinRequest(int gid, int id)
        {
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id}");

            data = await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE gid = {gid} AND isinvite = false;");

            List<object> result = new();
            foreach (var item in data)
            {
                result.Add(new{userId=item["uid"], guildId=item["gid"]});
            }
            
            return result;
        }        
        public static async Task<object> CancelGuildJoinRequest(int gid, int id)
        {
            await SqlManager.Reader($"DELETE FROM guild.guildinvites WHERE uid = {id} AND gid = {gid} AND isinvite = false ;");
            List<string> adminIds = new();
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE role != 0 AND gid = {gid}");

            foreach (var item in data)
            {
                await SqlManager.ExecuteNonQuery($"DELETE FROM user_{item["uid"]}.notifications WHERE type = 17 AND objid = {gid};");
            }
            return true;
        }
        public static async Task<object> DropFromGuild(int uid, int gid)
        {
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {uid} AND gid = {gid};")).Count ==
                0)
            {
                throw new CustomError("InGuildIsNotExistThisUser");
            }
            
            
            await SqlManager.ExecuteNonQuery($"DELETE FROM guild.guildusers WHERE uid = {uid} AND gid = {gid};");
            
            await NotificationsManager.SendNotifications(NotificationType.remove_from_guild,
                $"Usunięto cię z gildi {(await GetGuild(gid)).Name}",
                $"Usunięto cię z gildi {(await GetGuild(gid)).Name}!",
                $"walkwards://dashboard", new[] {uid.ToString()}, gid);
            
            return true;
        }
        public static async Task<object> DeleteGuild(int gid)
        {
            await SqlManager.ExecuteNonQuery($"DELETE FROM guild.guilds WHERE id = {gid};");
            await SqlManager.ExecuteNonQuery($"DELETE FROM guild.guildusers WHERE gid = {gid};");
            return true;
        }
        public static async Task<object> InviteToGuild(int uid, int gid, int sid)
        {
            
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {uid} AND gid = {gid};")).Count > 0 || (await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE uid = {uid} AND gid = {gid};")).Count > 0)
            {
                throw new CustomError("InviteIsExist");
            }
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {uid};")).Count > 0)
            {
                throw new CustomError("UserIsInGuild");
            }

            Guild guild = await GetGuild(gid);

            if (guild.CanMembersAddMembers == false && (await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {sid} AND role > 0;")).Count == 0) throw new CustomError("UserCanNotInvite");

            foreach (var item in guild.Users)
            {
                if (item.Id == uid) throw new CustomError("UserIsAlreadyInOtherGuild");
            }
            
            bool inOtherGuild;
            var data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {uid};");

            if (data.Count > 0) inOtherGuild = true;
            else inOtherGuild = false;

            await SqlManager.ExecuteNonQuery($"INSERT INTO guild.guildinvites VALUES ({uid}, {gid}, {sid}, {inOtherGuild}, true);");


            await NotificationsManager.SendNotifications(NotificationType.invite_to_guild,
                $"Zaproszenie do gildi {guild.Name}",
                $"Użytkownik {(await UserMethod.GetUserData(sid, false, false)).Username} wysłał Ci wysłał ci zaproszenie do gildi {guild.Name}!",
                $"walkwards://showGuild/{guild.Id}", new[] {uid.ToString()}, guild.Id);

            return true;
        }
        public static async Task<object> ResponseToInvite(int uid, int gid, bool accepted)
        {
            int sid = (await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE uid = {uid} AND gid = {gid};"))[0]["sid"];

            Guild guild = await GetGuild(gid);

            if (accepted)
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO guild.guildusers VALUES({uid}, {gid}, 0);");
                await NotificationsManager.SendNotifications(NotificationType.accepted_invite_to_guild,
                    $"Zaakceptowano twoje zaproszenie",
                    $"Użytkownik {(await UserMethod.GetUserData(uid, false, false)).Username} zaakceptował twoje zaproszenie do gildi {guild.Name}!",
                    $"walkwards://showGuild/{guild.Id}", new[] {sid.ToString()}, guild.Id);
            }
            else
            {
                await NotificationsManager.SendNotifications(NotificationType.reject_invite_to_guild,
                    $"Odrzucono twoje zaproszenie",
                    $"Użytkownik {(await UserMethod.GetUserData(uid, false, false)).Username} odrzucił twoje zaproszenie do gildi {guild.Name}!",
                    $"walkwards://showGuild/{guild.Id}", new[] {sid.ToString()}, guild.Id);
            }

            await SqlManager.ExecuteNonQuery($"DELETE FROM guild.guildinvites WHERE uid = {uid};");
            await SqlManager.ExecuteNonQuery($"DELETE FROM user_{uid}.notifications WHERE objid = {gid};");
            return true;
        }
        public static async Task<List<object>> GetGuildInvite(int id)
        {
            List<object> guilds = new();

            var data = await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE uid = {id};");

            foreach (var item in data)
            {
                try
                {
                    guilds.Add(new {guild=(await GetGuild(item["gid"])), inOtherGuild=item["inotherguild"], sender=(await UserMethod.GetUserData(item["sid"], false, false))});
                }
                catch (Exception e){continue;}
            }
            
            return guilds;
        }        
        public static async Task<List<User>> GetSentInvite(int gid)
        {
            List<User> users = new();

            var data = await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE gid = {gid};");

            foreach (var item in data)
            {
                users.Add(await UserMethod.GetUserData(item["uid"]));
            }
            
            return users;
        }
        public static async Task<object> EditGuild(string field, string value, int guildId)
        {
            if (field == "avatar")
            {
                await File.WriteAllBytesAsync($"/home/ubuntu/api/avatars/guild/{guildId}.txt", Convert.FromBase64String(value));
            }
            else
            {

                if (int.TryParse(value, out int intValue))
                {
                    if ((await SqlManager.Reader($"SELECT * FROM guild.guilds WHERE {field} = {value} ")).Count > 0 && field != "canmembersaddmembers")
                        throw new CustomError("GuildIsExist");
                    await SqlManager.ExecuteNonQuery($"UPDATE guild.guilds SET {field} = {intValue} WHERE id = {guildId};");
                }
                else
                {
                    if ((await SqlManager.Reader($"SELECT * FROM guild.guilds WHERE {field} = '{value}'")).Count > 0 && field != "canmembersaddmembers")
                        throw new CustomError("GuildIsExist");
                    await SqlManager.ExecuteNonQuery($"UPDATE guild.guilds SET {field} = '{value}' WHERE id = {guildId};");

                }
            }
            return true;
        }

        public static async Task<int> GetUserRelation(int guildId, int id)
        {
            //0 - nie ma gildii
            //1 - wyslal prosbe do gildii
            //2 - jest w tej gildii
            //3 - jest w innej gildii

            int status = 0;
            
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildinvites WHERE isinvite = false AND uid = {id} AND gid = {guildId};")).Count > 0)
            {
                return 1;
            }
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id}")).Count == 0)
            {
                return 0;
            }
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id} AND gid = {guildId};")).Count > 0)
            {
                return 2;
            }
            if ((await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {id} AND gid != {guildId};")).Count > 0)
            {
                return 3;
            }

            return status;
        }
    }
}
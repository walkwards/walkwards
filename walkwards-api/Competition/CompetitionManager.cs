using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;
using walkwards_api.Notifications;
using walkwards_api.Sql;
using walkwards_api.UserManager;
using walkwards_api.Utilities;

namespace walkwards_api.Competition
{
    public static class CompetitionManager
    {
        public async static Task<Competition> GetCompetition(int competitionId, int id = 0)
        {
            var data = await SqlManager.Reader($"SELECT * FROM base.competition WHERE id = {competitionId};");

            if (data.Count == 0) throw new CustomError("CompetitionNotExist");

            Competition competition = new Competition(data[0]["id"], data[0]["startdate"], data[0]["enddate"],
                await GetCompetitionUsers(competitionId), data[0]["name"], data[0]["companyName"],
                data[0]["ispublic"], data[0]["ctoken"],data[0]["isofficial"],data[0]["avatar"], data[0]["description"], data[0]["entrancefee"]);

            data = await SqlManager.Reader($"SELECT * FROM competitionusers.c{competitionId};");

            bool joined = false;

            List<User> users = new List<User>();
            foreach (var item in data)
            {
                User user = await UserMethod.GetUserData(item["id"], false, false);

                if (id == user.Id) joined = true;
                
                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(competition.StartDate)).TotalDays;
                var activities = await user.GetLastWeekActivity(days+1);

                int steps = 0;

                foreach (var activity in activities)
                {
                    steps += activity.y;
                }

                user.Steps = steps;
                
                users.Add(user);
            }

            
            users = users.OrderByDescending(item => item.Steps).ToList();
            
            competition.Useres = users;
            
            competition.DayCount = (DateTime.ParseExact(competition.EndDate.Split('.')[1] + "/" + competition.EndDate.Split('.')[0] + "/" + competition.EndDate.Split('.')[2], "dd/MM/yyyy", null) - DateTime.Today).Days;

            competition.Joined = joined;
            
            return competition;
        }
        
        public static async Task<object> GetCompetitions(int id)
        {
            var data = await SqlManager.Reader($"SELECT * FROM base.competition;");

            List<Competition> competitions = new();
            foreach (var item in data)
            {
                Competition competition = await GetCompetition(item["id"], id);
                competitions.Add(competition);
            }
            
            return competitions;
        }

        public async static Task<List<Competition>> GetUserActiveCompetitions(int id)
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{id}.mycompetitions;");
            List<Competition> competitions = new List<Competition>();
            for (int i = 0; i < data.Count; i++)
            {
                competitions.Add(await GetCompetition(data[i]["id"]));
            }


            return competitions;
        }

        private async static Task<List<User>> GetCompetitionUsers(int cid)
        {
            var data = await SqlManager.Reader($"SELECT * FROM competitionusers.c{cid};");
            List<User> users = new List<User>();
            for (int i = 0; i < data.Count; i++)
            {
                users.Add(await UserMethod.GetUserData(data[i]["id"], false, false));
            }

            
            return users;
        }
        
        public async static Task<object> CreateCompetition(string name, string startDate, string endDate, bool isPublic, int creator, bool isOfficial, string avatar, string description, int entranceFee, string companyName)
        {
            Random rnd = new Random();
            int id = rnd.Next(100000, 999999);

            Competition competition = new Competition(id, startDate, endDate, new(), name,
                companyName, isPublic, await GenerateToken(), isOfficial, avatar, description, entranceFee);

            string sql = "INSERT INTO base.competition VALUES(" +
                         $"{competition.Id}, '{competition.StartDate}', '{competition.EndDate}', '{competition.Name}', '{competition.CompanyName}', '{competition.Ctoken}', {competition.IsOfficial}, {competition.IsPublic}, '{competition.Avatar}', '{competition.Description}', {competition.EntranceFee})";
            
            await SqlManager.ExecuteNonQuery(sql);

            await SqlManager.ExecuteNonQuery($"CREATE TABLE competitionusers.c{id} (id int)");
            
            return competition;
        }
        
        public async static Task<object> JoinCompetition(int uid, string ctoken)
        {
            int userWalkCoins = (int)(await UserMethod.GetUserData(uid, false, false)).WalkCoins;
            
            int cid = 0;

            var data = await SqlManager.Reader($"SELECT * FROM user_{uid}.mycompetitions");
             //if(data.Count > 0) throw new CustomError("UserInCompetition");

            data = await SqlManager.Reader($"SELECT * FROM base.competition WHERE ctoken = '{ctoken}'");

            if (userWalkCoins < data[0]["entrancefee"]) throw new CustomError("DontHaveEnoughtWalkCoins");
            
            await SqlManager.ExecuteNonQuery($"UPDATE user_{uid}.user SET walkcoins = {userWalkCoins - data[0]["entrancefee"]};");

            if (data.Count == 0) throw new CustomError("InvalidCToken");
            else cid = data[0]["id"];
            
            data = await SqlManager.Reader($"SELECT * FROM competitionusers.c{cid} WHERE id = '{uid}'");
            if (data.Count > 0) throw new CustomError("UserIsExistInThisCompetition");
            
            await SqlManager.ExecuteNonQuery($"INSERT INTO competitionusers.c{cid} VALUES({uid});");
            await SqlManager.ExecuteNonQuery($"INSERT INTO user_{uid}.mycompetitions VALUES({cid});");
            
            return true;
        }
        
        public async static Task<object> DropFromCompetition(int uid, int cid)
        {
            await SqlManager.ExecuteNonQuery($"DELETE FROM competitionusers.c{cid} WHERE id = {uid};");
            await SqlManager.ExecuteNonQuery($"DELETE FROM user_{uid}.mycompetitions WHERE id = {cid};");
            return true;
        }

        public async static Task<object> EditCompetition(int cid, string fieldName, object value)
        {
            await SqlManager.ExecuteNonQuery($"UPDATE base.competition SET {fieldName} = '{value}' WHERE id ={cid};");
            return true;
        }
        
        public async static Task<object> DropCompetition(int cid)
        {
            var users = await GetCompetitionUsers(cid);

            foreach (User user in users)
            {
                await DropFromCompetition(user.Id, cid);
            }
            
            await SqlManager.ExecuteNonQuery($"DELETE FROM base.competition WHERE id = {cid};");
            await SqlManager.ExecuteNonQuery($"DROP TABLE competitionusers.c{cid};");
                
            return true;
        }
        
        public async static Task<object> EndCompetition() //TODO awards, finsh, walkcoins
        {
            var data = await SqlManager.Reader("SELECT * FROM base.competition");

            foreach (var item in data)
            {
                if (item["enddate"] == DateTime.Now.ToString("MM.dd.yyyy"))
                {
                    await SqlManager.ExecuteNonQuery($"DELETE FROM base.competition WHERE id = {item["id"]};");
                    await SqlManager.ExecuteNonQuery($"DROP TABLE competitionusers.c{item["id"]};");
                    
                    Competition c = await GetCompetition(item["id"]);

                    string[] ids = new string[c.Useres.Count];


                    for (int i = 0; i < c.Useres.Count; i++)
                    {
                        ids[i] = c.Useres[i].Id.ToString();
                        await SqlManager.ExecuteNonQuery($"DELETE FROM user_{ids[i]}.mycompetition;");

                    }
                    
                    await NotificationsManager.SendNotifications(NotificationType.give_up_challenge, "Konkurs się zakończył!",
                        $"Konkurs {c.Name} zakończył się wejdź do aplikacji i zobacz kto wygrał nagrody!",
                        $"walkwards://showUser/{c.Id}", ids, c.Id);
                    
                    //System nagród 
                }
            }
            
            
            return true;
        }
        //
        private static async Task<string> GenerateToken()
        {
            List<char> chars = new ();
            const int lengthToken = 6;

            for (int i = 65; i != 90; i++)
            {
                chars.Add((char)i);
            }
                
            Random random = new();
            string token = "";

            for (int i = 0; i != lengthToken; i++)
            {
                token += chars[random.Next(0, chars.Count - 1)];
            }

            return token;
        }

    }
}
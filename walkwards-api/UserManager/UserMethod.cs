using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using walkwards_api.Competition;
using walkwards_api.Notifications;
using walkwards_api.Sql;
using walkwards_api.structure;
using walkwards_api.Utilities;

namespace walkwards_api.UserManager
{
    public static class UserMethod 
    {
        public static async Task<bool> ResetPassword(int id, string? pass1, string? pass2)
        {
            if (pass1 != pass2)
            {
                throw new CustomError("PasswordIsNotSame");
            }

            await SqlManager.ExecuteNonQuery(
                $"UPDATE user_{id}.user SET password = '{Convert.ToBase64String(Encoding.UTF8.GetBytes(pass1))}';");
                
            return true;
        }
        public static async Task<bool> ActivateUser(int id)
        {
            var data = await SqlManager.Reader($"SELECT id FROM base.base WHERE id = {id};");
            
            if (data.Count == 0)
            {
                throw new CustomError("InvalidLogin");
            }

            await SqlManager.ExecuteNonQuery($"UPDATE user_{id}.user SET isActivated = true;");

            return true;
        }
        public static async Task<User> IsUserExist(int id)
        {
            string sql = $"SELECT * FROM base.base WHERE id = {id};";
            
            var data = await SqlManager.Reader(sql);
            
            if(data.Count == 0)
            {
                throw new CustomError("UserNotExist");
            }
            
            return await GetUserData(id, true, false);
        }
        public static async Task<object> Login(string? loginOrEmail, string? password)
        {
            if (loginOrEmail != null && (loginOrEmail.Contains('\'') || loginOrEmail.Contains('"'))) throw new CustomError("TrySqlInjection");
            
            string sql = $"SELECT id, email, login FROM base.base WHERE login = '{loginOrEmail}' OR email = '{loginOrEmail}';";
            var data = await SqlManager.Reader(sql);
            int id;

            if(data.Count == 0)
            {
                throw new CustomError("InvalidLogin");
            }
            
            id = data[0]["id"];
            
            if (password != await GetUserPassword(id))
            {
                throw new CustomError("InvalidLogin");
            }

            User user = await GetUserData(id, true, false);

            if (!user.IsActivated)
            {
                throw new CustomError("NotActivated");
            }
            
            user.Token = await LoginMethod.GenerateToken(user);

            return user;
        }
        public static async Task<List<User>> FriendSearch(string token, int id, string query, int page, int objPerPage = 21)
        {
            //if (await (await GetUserData(id)).CheckToken(token)) throw new CustomError("InvalidToken");
            var user = await GetUserData(id, false);
            var ctoken = await user.CheckToken(token);
            
            if(!ctoken)
            {
                throw new CustomError("InvalidToken");
            }
            
            List<User> friends = new ();

            var data = await SqlManager.Reader($"SELECT id FROM base.base WHERE lower (login) LIKE lower ('%{query}%');");

            if (data.Count <= 0) return friends;
            
            List<int> ids = data.Select(item => item["id"]).Cast<int>().ToList();


            int i = 0;
            foreach (var item in ids)
            {
                i++;

                if(i < page * objPerPage) continue;

                var friend = await GetUserData(item, false, false);

                if (!friend.IsActivated) continue;
                
                
                friends.Add(friend);
                if(i == (page * objPerPage) + objPerPage -1) break;
            }

            friends = friends.OrderByDescending(userActivity => userActivity.PlaceInRanging).ToList();

            return friends;
        }
        
        public static async Task<UserActivity[]> GetRanking(int page, string token, int id, int resultsPerPage = 21)
        {
            //if (!await (await GetUserData(id)).CheckToken(token)) throw new CustomError("InvalidToken");

            var data = await SqlManager.Reader($"SELECT * FROM base.ranking ORDER BY steps DESC, id LIMIT {resultsPerPage * (page + 1)};");
            
            List<int> ids = data.Select(item => item["id"]).Cast<int>().ToList();
            ids.Distinct().ToList();
            
            List<UserActivity> allActivities = new ();
            
            foreach (var item in ids) 
            {
                var userActivity = new UserActivity(
                    await (await GetUserData(item, false, false)).GetActivityCurrentDay(),
                await GetUserData(item, false, false));
            
                if (!userActivity.IsActivated) continue;
            
                allActivities.Add(userActivity);
            }

            allActivities = allActivities.OrderByDescending(userActivity => userActivity.Steps).ToList();

            var activities = new List<UserActivity>();

            for (var i = (page * resultsPerPage); i != (resultsPerPage * page) + resultsPerPage - 1; i++)
            {
                if (i == allActivities.Count) break;
                activities.Add(allActivities[i]);
            }
          
            return activities.ToArray();
        }
        
        public static async Task<UserActivity[]> GetRankingAllSteps(int page, string token, int id, int resultsPerPage = 21)
        {
            var data = await SqlManager.Reader($"SELECT * FROM base.rankingglobal ORDER BY steps DESC, id LIMIT {resultsPerPage * (page + 1)}");
            List<int> ids = data.Select(item => item["id"]).Cast<int>().ToList();
            List<UserActivity> allActivities = new();
            
            foreach (var item in ids)
            {
                User user = await GetUserData(item, false, false);

                List<Activity> allActivity = await user.GetAllActivity();
                int allsteps = allActivity.Sum(day => day.y);

                var userActivity = new UserActivity(new Activity(null, allsteps), user);

                if (!userActivity.IsActivated) continue;

                allActivities.Add(userActivity);
            }

            allActivities = allActivities.OrderByDescending(userActivity => userActivity.Steps).ToList();

            var activities = new List<UserActivity>();

            for (var i = (page * resultsPerPage); i != (resultsPerPage * page) + resultsPerPage - 1; i++)
            {
                if (i == allActivities.Count) break;
                activities.Add(allActivities[i]);
            }
          
            return activities.ToArray();
        }
        public static async Task<User> GetUserData(int id, bool getPlaceInRanking = true, bool reqCheckId = true)
        {
            //
            User user;
            if (reqCheckId)
            {
                try
                {
                    user = await IsUserExist(id);
                    return user;
                }
                catch (CustomError cf)
                {
                    string name = cf.Name;
                    throw new CustomError(name);
                }
            }

            string sql = $"SELECT * FROM user_{id}.user;";

            
            var data = await SqlManager.Reader(sql);

           
            user = new User(id, 
                    data[0]["login"], 
                    data[0]["email"],
                    data[0]["goal"],
                    data[0]["avatar"],
                    data[0]["joineddata"],
                    (int)data[0]["walkcoins"], 
                    data[0]["isactivated"],
                    data[0]["accountprivacytype"]);
            
            if (getPlaceInRanking)
            {
                user.PlaceInRanging = await user.GetPlaceInRanking(true);
                user.PlaceInRangingGloabal = await user.GetPlaceInRanking(false);

                bool haveGuild = true;
                int place = 0;
                int guildId = 0;
                try
                {
                    Guild guild = await GuildManager.GetUserGuild(user.Id);
                    guildId = guild.Id;
                    foreach (var item in guild.Users)
                    {   
                        if (item.Id == id) break;
                        place++;
                    }
                }
                catch (Exception e)
                {
                    place = -1;
                    haveGuild = false;
                }
                
                user.PlaceUserInGuildRanking = place+1;
//
                place = -1;
//
                if (haveGuild)
                {
                    place = 0;
                    data = await SqlManager.Reader($"SELECT * FROM guild.guilds;");
                    List<Guild> guilds = new();
                    foreach (var item in data)
                    {
                        guilds.Add(await GuildManager.GetGuild(item["id"]));
                    }
//
                    guilds = guilds.OrderByDescending(s => s.StepSum).ToList();
                    foreach (var item in guilds)
                    {
                        if (item.Id == guildId) break;
                        place++;
                    }

                    data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {user.Id};");
                    user.Role = data[0]["role"];
                }
//
                user.PlaceGuildInRanking = place + 1;
            }

            try
            {
                data = await SqlManager.Reader($"SELECT * FROM guild.guildusers WHERE uid = {user.Id};");
                user.Role = data[0]["role"];
            }
            catch (Exception e)
            {
                user.Role = -1;

            }
            try
            {
                user.WasActive = data[0]["wasactive"];
            }
            catch (Exception e)
            {
                user.WasActive = -1;
            }
            return user;
        }
        private static async Task<string> GetUserPassword(int id)
        {
            string sql = $"SELECT password, login FROM user_{id}.user;";
            var data = await SqlManager.Reader(sql);
            
            return data.Count == 0 ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(data[0]["password"]));
        }
        public static async Task<User> CreateUser(string? login, string? email, string? password)
        {
            if (login != null && (login.Contains('\'') || login.Contains('"'))) throw new CustomError("TrySqlInjection");
            if (email != null && (email.Contains('\'') || email.Contains('"'))) throw new CustomError("TrySqlInjection");

            
            var id = await UserCreateUserMethods.GenerateId();
            User user = new User(id, login, email);
            
            if (!UserCreateUserMethods.ValidateLogin(login, email, password))
            {
                throw new CustomError("InvalidLogin");
            }
            
            if (await UserCreateUserMethods.IsLoginOrEmailExist(user))
            {
                throw new CustomError("UserIsExist");
            }

            await UserCreateUserMethods.CreateDataBase(user, password);
            await UserCreateUserMethods.SendActivateLink(user);

            return user;
        }
        public static async Task<object> ForgotPassword(string? loginOrEmail)
        {
            string sql = $"SELECT email, id FROM base.base WHERE login = '{loginOrEmail}' OR email = '{loginOrEmail}';";
            var data = await SqlManager.Reader(sql);
            
            if (data.Count == 0) throw new CustomError("InvalidLogin");
            
            string email = data[0]["email"];
            int id = data[0]["id"];

            using MailMessage msg = new();
            msg.From = new MailAddress("walkwardsdev@gmail.com");
            msg.To.Add(email);
            msg.Subject = "Resetowanie hasła - WalkWards"; 
            msg.Body = await GetMailText(await GetUserData(id, false, false), "mail_reset_password.txt");
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = true;

            using SmtpClient client = new ();
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("walkwardsdev@gmail.com", "uK8ujw@vH@qh2e6g!V_phWu*P!"); //fajne hasło xD
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network; 
                
            await client.SendMailAsync(msg);

            return true;
        }

        private static async Task<string> GetMailText(User user, string filename)
        {
            string text = await File.ReadAllTextAsync(filename);
            text = text.Replace("{username}", user.Username);
            text = text.Replace("{id}", user.Id.ToString());
            return text;
        }
        private static class UserCreateUserMethods
        {
            public static async Task<int> GenerateId()
            {
                Random rnd = new Random();

                int id = rnd.Next(100000, 999999);

                var data = await SqlManager.Reader($"SELECT id FROM base.base");
                List<int> ids = data.Select(item => item["id"]).Cast<int>().ToList();

                while (ids.Contains(id))
                {
                    id = rnd.Next(100000, 999999);
                }

                return id;
            }
            public static async Task CreateDataBase(User user, string? password)
            {
                string schemaName = "user_" + user.Id;

                if (password != null)
                {
                    List<string> sqls = new List<string>()
                    {
                        $"create schema {schemaName}; alter schema {schemaName} owner to postgres;",
                        $"create table user_{user.Id}.user(id int, login varchar(255), email varchar(255), password varchar(255), avatar bool, isActivated bool, AccountPrivacyType int, JoinedData varchar(100), Goal int, walkcoins int, wasactive int);",
                        $"create table user_{user.Id}.tokens(token varchar(6), isActivated bool);",
                        $"create table user_{user.Id}.friends(recipient int, type int);",
                        $"create table user_{user.Id}.activity(day varchar(255), steps int);",
                        $"create table user_{user.Id}.mycompetitions(id int);",
                        $"create table user_{user.Id}.notifications(type int, title varchar(255), message varchar(255), date varchar(255), objid int, id serial PRIMARY KEY);",
                        
                        $"Insert into base.base (id, login, email)  VALUES({user.Id},'{user.Username}','{user.Email}');",
                        $"Insert into user_{user.Id}.user (id, login, email, password, avatar, isactivated, accountprivacytype, joineddata, goal, walkcoins) VALUES({user.Id},'{user.Username}','{user.Email}','{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}', {false}, {false},{0}, '{ DateTime.Today.ToString("MM.dd.yyyy")}', 6000, 0);",
                        
                        //$"create sequence user_{user.Id}.notifications_id_seq as integer;",
                        $"alter table user_{user.Id}.notifications alter column id set default nextval('user_{user.Id}.notifications_id_seq'::regclass);",
                        $"alter sequence user_{user.Id}.notifications_id_seq owned by user_{user.Id}.notifications.id;"
                    };

                    await SqlManager.ExecuteFromList(sqls);
                }
            }
            public static async Task SendActivateLink(User user)
            {
                MailMessage msg = new();
                
                msg.From = new MailAddress("walkwardsdev@gmail.com");
                if (user.Email != null) msg.To.Add(user.Email);
                else throw new CustomError("InvalidLogin");
                msg.Subject = "Aktywuj konto";
                msg.Body = await GetMailText(user, "mail_activate.txt");
                msg.Priority = MailPriority.High;
                msg.IsBodyHtml = true;

                using SmtpClient client = new();
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("walkwardsdev@gmail.com", "uK8ujw@vH@qh2e6g!V_phWu*P!");
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                
                await client.SendMailAsync(msg);
            }
            public static async Task<bool> IsLoginOrEmailExist(User user)
            {
                return (await SqlManager.Reader($"SELECT * FROM base.base WHERE login = '{user.Username}' OR email = '{user.Email}';")).Count > 0;
            }
            public static bool ValidateLogin(string? login, string? email, string? password)
            {
                if (login != null && (login.Length <= 3 || login.Length >= 20))
                    return false;
                if (password != null && (password.Length <= 5 || password.Length >= 30))
                    return false;
                if (password != null && !password.Any(char.IsUpper))
                    return false;
                if (email != null && !email.Contains('@'))
                    return false;
                if (email != null && !email.Contains('.'))
                    return false;
                if (password != null && !password.Any(char.IsLower))
                    return false;

                return true;
            }
        }
        private static class LoginMethod
        {
            public async static Task<string> GenerateToken(User user)
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

                await SqlManager.ExecuteNonQuery($"Insert into user_{user.Id}.tokens VALUES('{token}', true);");
                
                return token;
            }
        }

        public static async Task<object> PayOutWalkCoins()
        {
            var data = await SqlManager.Reader("SELECT * FROM base.base;");

            foreach (var item in data)
            {
                
            }

            return true;
        }
    }
}
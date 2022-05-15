using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using Npgsql.Internal;
using Renci.SshNet;
using walkwards_api.structure;
using walkwards_api.Utilities;
using walkwards_api.Notifications;
using walkwards_api.Sql;

namespace walkwards_api.UserManager
{
    public class User
    {
        private readonly string _date = DateTime.Now.ToString("MM.dd.yyyy");
        
        public User(int id, string? username, string? email, int goal, bool avatar, string joinDate, float walkcoins = 0, bool isActivated = false, int accoutPrivacy = 0, int steps = 0, int wasActive = 0)
        {
            Id = id;
            Username = username;
            Email = email;
            Avatar = avatar;
            IsActivated = isActivated;
            AccoutPrivacy = (PrivacyType)accoutPrivacy;
            Goal = goal;
            WalkCoins = walkcoins;
            JoinDate = joinDate;
            Steps = steps;
            WasActive = wasActive;
        }
        public User(int id, string? username, string? email)
        {
            Id = id;
            Username = username;
            Email = email;
        }
        //propertis
        public int Id { get ; set; }
        public string? Username { get ; set; }
        public string? Email { get ; set; }
        
        public int PlaceInRanging              {get; set; } // 1 to pierwsze miejsce
        public int PlaceInRangingGloabal       {get; set ;} // 1tak jak wyzejk
        public int PlaceUserInGuildRanking     {get; set ;} // -1 nie ma gildi liczone od 0
        public int PlaceGuildInRanking         {get; set ;} // to samo
        public bool IsActivated { get; set; }
        public bool Avatar { get; set; }
        public PrivacyType? AccoutPrivacy;
        
        public string Token;
        public int Goal;
        public float WalkCoins { get; set; }
        public int Steps { get; set; }
        public int Role { get; set; }
        public string JoinDate;
        public int WasActive;

        protected User(string token)
        {
            Token = token;
        }

        //other
        public async Task<bool> CheckToken(string token)
        {
            //token for postman
            if(token == "2ykrohna")
            {
                return true;
            }

            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.tokens WHERE token = '{token}';");

            if (data.Count == 0)
            {
                throw new CustomError("InvalidToken");
            }

            return true;
        }
        
        //friends

        public async Task<object> ProposalUsers()
        {
            //WTF
            return null;
        }
        
        public async Task<object> AddFriend(int recipient)
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.friends WHERE recipient = {recipient};");
            if (data.Count != 0)
            {
                throw new CustomError("AlreadyFriend");
            }

            await SqlManager.ExecuteNonQuery($"INSERT INTO user_{this.Id}.friends VALUES ({recipient}, 1);");
            await SqlManager.ExecuteNonQuery($"INSERT INTO user_{recipient}.friends VALUES ({this.Id}, 0);");

            try
            {
                await UserMethod.IsUserExist(recipient);
            }
            catch (CustomError ce)
            {
                throw new CustomError(ce.Name);
            }

            User friend = await UserMethod.GetUserData(recipient); 

           await NotificationsManager.SendNotifications(NotificationType.send_friend_requst,
                "Prośba o dodanie do grona znajomych",
                $"Użytkownik {this.Username} wysłał Ci prośbę o dodanie do grona znajomych!",
                $"walkwards://showUser/{Id}", new[] {recipient.ToString()}, Id);
            
            return true;
        }

        public async Task<List<UserActivity>> GetAllFriend(int page, int objPerPage = 21) //Async 
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.friends;");
            
            if (data.Count == 0)
            {
                throw new CustomError("NoFriends");
            }
            
            var users = new List<UserActivity>();
            
           
            int i = 0;

            //allActivities = allActivities.OrderByDescending(userActivity => userActivity.Steps).ToList();
//
            //var activities = new List<UserActivity>();
//
            //for (var i = (page * resultsPerPage); i != (resultsPerPage * page) + resultsPerPage - 1; i++)
            //{
            //    if (i == allActivities.Count) break;
            //    activities.Add(allActivities[i]);
            //}
          //
            //return activities.ToArray();
            
            foreach (var userData in data)
            {
                i++;
                if(i < page * objPerPage) continue;

                
                if ((FriendType) await GetFriendShip(userData["recipient"]) != FriendType.Friend) continue;
                
                User tempUser = await UserMethod.GetUserData(userData["recipient"], false, false);
                users.Add(new UserActivity(await tempUser.GetActivityCurrentDay(), tempUser));
                
                if(i == (page * objPerPage) + objPerPage -1) break;

            }
            
            users = users.OrderBy(o => o.Steps).ToList();

            return users;
        }

        public async Task<object> GetFriendShip(int recipient) 
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{Id}.friends WHERE recipient = {recipient};");
            if (data.Count == 0) return 3;
            else return data[0]["type"];
        }
        public async Task<object> GetFriendsRequest()
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.friends WHERE type = {(int)FriendType.Request};");
            
            if (data.Count == 0)
            {
                throw new CustomError("NoFriends");
            }
            
            var users = new List<UserActivity>();
            
            foreach (var userData in data)
            {
                User tempUser = await UserMethod.GetUserData(userData["recipient"], false, false);
                users.Add(new UserActivity(await tempUser.GetActivityCurrentDay(), tempUser));
            }
            
            users = users.OrderBy(o => o.Steps).ToList();

            return users;
        }
        public async Task<object> AcceptOrCancelFriendRequest(int recipient, bool accept) //Async
        {
            try
            {
                FriendType type = (FriendType)await GetFriendShip(recipient);

                await UserMethod.IsUserExist(recipient);
                if (type == FriendType.Request)
                {
                    if (accept)
                    {
                        await SqlManager.ExecuteNonQuery(
                            $"UPDATE user_{Id}.friends SET type = {(int) FriendType.Friend} WHERE recipient = {recipient};");

                        await SqlManager.ExecuteNonQuery(
                            $"UPDATE user_{recipient}.friends SET type = {(int) FriendType.Friend} WHERE recipient = {Id};");

                        await SqlManager.ExecuteNonQuery(
                                $"UPDATE user_{recipient}.friends SET type = {(int) FriendType.Friend} WHERE recipient = {Id};")
                            ;

                        User friend = await UserMethod.GetUserData(recipient);

                        await NotificationsManager.SendNotifications(NotificationType.accept_friend_request,
                            "Prośba o dodanie do grona znajomych",
                            $"Użytkownik {Username} zaakceptował Twoją prośbę o dodanie do grona znajomych!",
                            $"walkwards://showUser/{Id}", new[] {recipient.ToString()}, Id);
                        string sql = $"DELETE FROM user_{Id}.notifications WHERE objid = {friend.Id} AND type = 0;";

                        await SqlManager.ExecuteNonQuery(sql);


                    }
                    else
                    {
                        await SqlManager.ExecuteNonQuery($"DELETE FROM user_{Id}.friends WHERE recipient = {recipient};");
                        await SqlManager.ExecuteNonQuery($"DELETE FROM user_{recipient}.friends WHERE recipient = {Id};");
                            
                        User friend = await UserMethod.GetUserData(recipient); 

                        await NotificationsManager.SendNotifications(NotificationType.reject_friend_request,
                            "Prośba o dodanie do grona znajomych",
                            $"Użytkownik {friend.Username} odrzucił Twoją prośbę o dodanie do grona znajomych!",
                            $"walkwards://showUser/{Id}", new[] {recipient.ToString()}, Id);
                        string sql = $"DELETE FROM user_{Id}.notifications WHERE objid = {friend.Id} AND type = 0;";
                        await SqlManager.ExecuteNonQuery(sql);

                    }

                }
                else
                {
                    throw new CustomError("UserIsNotFriend");
                }
            }
            catch (CustomError cf)
            {
                throw new CustomError(cf.Name);
            }

            return true;
        }
        public async Task<object> RemoveFriend(int recipient) //Async
        {
            try
            {
                await UserMethod.IsUserExist(recipient);
            }
            catch
            {
                throw new CustomError("UserIsNotFriend");
            }

            if ((FriendType)await GetFriendShip(recipient) != FriendType.None)
            {
                await SqlManager.ExecuteNonQuery($"DELETE FROM user_{Id}.friends WHERE recipient = {recipient};");
                await SqlManager.ExecuteNonQuery($"DELETE FROM user_{recipient}.friends WHERE recipient = {Id};");

                await NotificationsManager.SendNotifications(NotificationType.remove_friend, "Usunięto Cię z grona znajomych!", 
                    $"Użytkownik {Username} usunął Cię z grona znajomych!",
                    $"walkwards://showUser/{Id}", new []{recipient.ToString()}, Id);
                
            }
            else
            {
                throw new CustomError("UserIsNotFriend");
            }


            return true;
        }

        //activity
        public async Task<object> SendDailyRaport()
        {
            var acti = await GetLastWeekActivity(2);

            string surpassedText = "";
            if (acti[1].y > Goal)
                surpassedText =
                    $"Wczoraj był udany dzień przebiłeś swój cel kroków o {acti[1].y - Goal} kroków trzymaj tak dalej";
            else if (acti[1].y == Goal)
                surpassedText =
                    $"Wczoraj było dobrze udało ci się wyrównać cel wynoszący {Goal} Dzisiaj też bądź aktywny";
            else if (acti[1].y < Goal)
                surpassedText =
                    $"Wczoraj nie do końca udało ci się być aktywnym zabrakło ci  {Goal - acti[1].y} Dzisiaj będzie lepiej";

            return await NotificationsManager.SendNotifications(NotificationType.send_daily_raport,
                "Dzienny Raport Twojej Aktywności",
                $"Cześć {Username} zaczyna się kolejny piękny dzień 😁. Wczorajsza liczba twoich kroków wynosiła {acti[1].y}. {surpassedText}",$"walkwards://",new[] {Id.ToString()}, Id);
        }

        public async Task<object> SetGoal(int newGoal)
        {
            await SqlManager.ExecuteNonQuery($"UPDATE user_{Id}.user SET goal = {newGoal};");
            return true;
        }
        public async Task<object> AddActivity(float steps)
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.activity WHERE day = '{_date}';");

            if (steps > 40000) throw new CustomError("ToManySteps");
            if (data.Count > 0)
            {
                if (((data[0]["day"].ToString() as string)!).Contains("ban"))
                {
                    await SqlManager.ExecuteNonQuery(
                        $"UPDATE user_{this.Id}.activity SET steps = {steps} WHERE day = '{_date + " ban"}';");
                    throw new CustomError("ban");
                }
            }

            if(data.Count > 0)
            {
                int todaySteps = (await SqlManager.Reader($"SELECT * FROM user_{Id}.activity WHERE day = '{_date}'"))[0]["steps"];
                
                await SqlManager.ExecuteNonQuery($"UPDATE user_{this.Id}.activity SET steps = {steps} WHERE day = '{_date}';");
                
                
                float walkcoins = (float)(await SqlManager.Reader($"SELECT * FROM user_{Id}.user;"))[0]["walkcoins"];

                if (steps - todaySteps > 100)
                {
                    walkcoins += (steps - todaySteps) / 100;

                    string sql = $"UPDATE user_{Id}.user SET walkcoins = {Math.Round(walkcoins, 2).ToString().Replace(',','.')};";
                    await SqlManager.ExecuteNonQuery(sql);
                }
            }
            else
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO user_{this.Id}.activity VALUES('{_date}', {steps});");
                float walkcoins = (float)(await SqlManager.Reader($"SELECT * FROM user_{Id}.user;"))[0]["walkcoins"];

                if (steps > 100)
                {
                    walkcoins += (steps / 100);

                    string sql = $"UPDATE user_{Id}.user SET walkcoins = {Math.Round(walkcoins, 2).ToString().Replace(',','.')};";
                    await SqlManager.ExecuteNonQuery(sql);
                }
            }
            
            data = await SqlManager.Reader($"SELECT * FROM base.ranking WHERE id = {Id};");
            
            if(data.Count > 0)
            {
                await SqlManager.ExecuteNonQuery($"UPDATE base.ranking SET steps = {steps} WHERE id = {Id};");
            }
            else
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO base.ranking VALUES({Id}, {steps});");
                float walkcoins = (float)(await SqlManager.Reader($"SELECT * FROM user_{Id}.user;"))[0]["walkcoins"];

                if (steps > 100)
                {
                    walkcoins += (steps / 100);

                    string sql = $"UPDATE user_{Id}.user SET walkcoins = {Math.Round(walkcoins, 2).ToString().Replace(',','.')};";
                    await SqlManager.ExecuteNonQuery(sql);
                }
            }

            
            data = await SqlManager.Reader($"SELECT * FROM base.rankingglobal WHERE id = {Id};");
            
            //if(data.Count > 0)
            //{
            //    steps = (await GetAllActivity()).Sum(activity => steps);
            //    await SqlManager.ExecuteNonQuery($"UPDATE base.rankingglobal SET steps = {steps} WHERE id = {Id};");
            //}
            //else
            //{
            //    steps = (await GetAllActivity()).Sum(activity => steps);
//
            //    await SqlManager.ExecuteNonQuery($"INSERT INTO base.rankingglobal VALUES({Id}, {steps});");
            //}

            if (WasActive == -1)
            {
                await SqlManager.ExecuteNonQuery($"UPDATE user_{Id}.user SET wasactive = {DateTime.Now.Hour}");
            }
            
            


            return true;
        }

        public async Task<object> AddActivityFromArray(JArray obj)
        {
            int y = 0;
            foreach (var item in obj)
            {
                await AddActivityForDate((int)item["steps"], (string)item["date"]);
            }
            return true;
        }
        
        public async Task<object> AddActivityForDate(int steps, string date)
        {
            
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.activity WHERE day = '{date}';");
            
            if (steps > 40000) throw new CustomError("ToManySteps");

            if (data.Count > 0)
            {
                if (((data[0]["day"].ToString() as string)!).Contains("ban"))
                {
                    await SqlManager.ExecuteNonQuery(
                        $"UPDATE user_{this.Id}.activity SET steps = {steps} WHERE day = '{_date + " ban"}';");
                    throw new CustomError("ban");
                }
            }

            if(data.Count > 0)
            {
                float walkcoins = (float)(await SqlManager.Reader($"SELECT * FROM user_{Id}.user;"))[0]["walkcoins"];
                int todaySteps = (await SqlManager.Reader($"SELECT * FROM user_{Id}.activity WHERE day = '{date}';"))[0]["steps"];
                
                if (steps - todaySteps > 100)
                {
                    walkcoins += (steps - todaySteps) / 100;

                    string sql =
                        $"UPDATE user_{Id}.user SET walkcoins = {Math.Round(walkcoins, 2).ToString().Replace(',', '.')};";

                    await SqlManager.ExecuteNonQuery(sql);
                    await SqlManager.ExecuteNonQuery(
                        $"UPDATE user_{this.Id}.activity SET steps = {steps} WHERE day = '{date}';");
                }
            }
            else
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO user_{Id}.activity VALUES('{date}', {steps});");
                
                if (steps < 100) steps = 0;

                float walkcoins = (float)(await SqlManager.Reader($"SELECT * FROM user_{Id}.user;"))[0]["walkcoins"];
            
                walkcoins += (steps / 100);

                string sql = $"UPDATE user_{Id}.user SET walkcoins = {Math.Round(walkcoins, 2).ToString().Replace(',','.')};";
                await SqlManager.ExecuteNonQuery(sql);
            }
            if (WasActive == -1)
            {
                await SqlManager.ExecuteNonQuery($"UPDATE user_{Id}.user SET wasactive = {DateTime.Now.Hour}");
            }
            
            if(DateTime.Parse(date) == DateTime.Today)
            {
                await AddActivity(steps);
            }
            return true;
        }
        
        
        public async Task<List<Activity>> GetAllActivity()
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.activity;");

            List<Activity> activities = data.Select(item => new Activity(item["day"], item["steps"])).ToList();
            
            return activities;
        }
        
        public async Task<List<Activity>> GetLastWeekActivity(int dayCount)
        {
            if (dayCount < 1) return new();

            List<Activity> dateSample = new();
            
            for (var i = 0; i < dayCount; i++)
            {
                dateSample.Insert(0, new Activity(DateTime.Now.AddDays(-i).ToString("MM.dd.yyyy"), 0));
            }
            
            string sql =
                $"SELECT * FROM user_{this.Id}.activity WHERE day IN ({string.Join(",", dateSample.Select(item => $"'{item.x}'"))});";
            var data = await SqlManager.Reader(sql);
            int y = 0;
            foreach (var activity in data)
            {
                dateSample.First(item => item.x == (string) activity["day"]).y = (int) activity["steps"];
            }
            
            return dateSample;
        }
        
        public async Task<object> GetActivity(bool isGlobal)
        {
            if (isGlobal)
            {
                var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.activity;");

                var days =                 await GetLastWeekActivity(data.Count);
                int sum = 0;
                foreach (var item in days)
                {
                    sum += item.y;
                }

                return sum;
            }
            else
            {
                return (await GetActivityCurrentDay()).y;
            }
            
        }
        
        public async Task<Activity> GetActivityCurrentDay() //Async
        {
            var data = await SqlManager.Reader($"SELECT * FROM user_{this.Id}.activity WHERE day = '{_date}';");
            
            Activity currentDay = new();

            if (data.Count > 0)
            {
                currentDay = new Activity(data[0]["day"], data[0]["steps"]);
            }
            else
            {
                currentDay = new Activity(null, 0);
            }

            return currentDay;
        }
        
        
        //settings
        public async Task<object> Logout(string token)
        {
            if (!await CheckToken(token)) throw new CustomError("InvalidToken");

            await SqlManager.ExecuteNonQuery($"DELETE FROM user_{Id}.tokens WHERE token ='{token}';");

            return true;
        }
        public async Task<object> SetNewLogin(string? newLogin) //Async
        {
            var data = await SqlManager.Reader($"SELECT id, email, login FROM base.base WHERE login = '{newLogin}' OR email = '{newLogin}';");
            
            if (data.Count > 0)
            {
                throw new CustomError("LoginIsExist");
            }

            await SqlManager.ExecuteNonQuery($"UPDATE user_{this.Id}.user SET login = '{newLogin}';");

            await SqlManager.ExecuteNonQuery($"UPDATE base.base SET login = '{newLogin}' WHERE id = {this.Id};");

            return true;
        }
        public async Task<object> SetAccountPrivacy(int? accountPrivacy) //Async
        {
            await SqlManager.ExecuteNonQuery($"UPDATE user_{this.Id}.user SET accountprivacytype = '{accountPrivacy}';");

            return true;
        }

        public async Task<object> SetNewEmail(string? newEmail) //Async
        {
            var data = await SqlManager.Reader($"SELECT id, email, login FROM base.base WHERE login = '{newEmail}' OR email = '{newEmail}';");
            
            if (data.Count > 0)
            {
                throw new CustomError("EmailIsExist");
            }

            await SqlManager.ExecuteNonQuery($"UPDATE user_{this.Id}.user SET email = '{newEmail}';");
            await SqlManager.ExecuteNonQuery($"UPDATE base.base SET email = '{newEmail}' WHERE id = {this.Id};");
            
            return true;
        }

        public async Task<object> SetNewAvatar(string? newAvatar) //Async
        {
            try
            {
                await SqlManager.ExecuteNonQuery($"UPDATE user_{Id}.user SET avatar = true;");
                await File.WriteAllBytesAsync($"/home/ubuntu/api/avatars/{Id}.txt", Convert.FromBase64String(newAvatar!));
            }
            catch (Exception e)
            {
                await LoggerManager.WriteLog(e.Message);
                throw;
            }
            return true;
        }
        //SFTP

        public async Task<object> PayinForUkraine()
        {
            if (WalkCoins < 1000) throw new CustomError("UserHaveNotEnoughMoney");

            var data = await SqlManager.Reader($"SELECT * FROM base.ukraine WHERE id = {Id};");

            if (data.Count == 0)
            {
                await SqlManager.ExecuteNonQuery($"INSERT INTO base.ukraine VALUES({Id}, {0})");
            }
            else
            {
                await SqlManager.ExecuteNonQuery($"UPDATE base.ukraine SET count = {data[0]["id"] + 1}; WHERE id = {Id}");
            }

            await SqlManager.ExecuteNonQuery($"UPDATE user_{Id}.user SET walkcoins = {WalkCoins - 500}");
            
            return true;
        }
        
        //ranking
        public async Task<int> GetPlaceInRanking(bool isGloabal = false)
        {
            if(isGloabal)
            {
                var data = await SqlManager.Reader($"SELECT id FROM base.ranking order by steps DESC, id;");

                for (int i = 0; i < data.Count; i++)
                {
                    if(data[i]["id"] == Id) return i+1;
                }
            }
            else
            {

                var data = await SqlManager.Reader($"SELECT id FROM base.rankingglobal order by steps DESC, id;");

                for (int i = 0; i < data.Count; i++)
                {
                    if(data[i]["id"] == Id) return i+1;
                }
            }
            
            return -1;
        }
    }
}
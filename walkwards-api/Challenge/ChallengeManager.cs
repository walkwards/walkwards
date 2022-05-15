using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using walkwards_api.Notifications;
using walkwards_api.Sql;
using walkwards_api.UserManager;
using walkwards_api.Utilities;
using Activity = walkwards_api.structure.Activity;

namespace walkwards_api.Challenge
{
    public static class ChallengeManager
    {
        public static async Task<object> SetChallenge(int user1, int user2, int betValue, int dayCount)
        {
            Random rnd = new Random();
            int id = rnd.Next(100000, 999999);

            Challenge challenge = new Challenge(id, await UserMethod.GetUserData(user1, false, false), await UserMethod.GetUserData(user2, false, false), false, betValue, dayCount);

            var data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE sender = {challenge.User1.Id} AND recipient = {challenge.User2.Id} AND finished = false;");
            if (data.Count > 0) throw new CustomError("ChallengeIsExist");
            data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE sender = {challenge.User2.Id} AND recipient = {challenge.User1.Id}AND finished = false;");
            if (data.Count > 0) throw new CustomError("ChallengeIsExist");
            
            if (challenge.User1.WalkCoins < betValue) throw new CustomError("User1NotHaveEnoughtMoney");
            if (challenge.User2.WalkCoins < betValue) throw new CustomError("User2NotHaveEnoughtMoney");
            
            await SqlManager.ExecuteNonQuery(
                $"INSERT INTO base.challenge VALUES({challenge.Id}, {challenge.User1.Id}, {challenge.User2.Id}, false ,'', '', {betValue}, false, {dayCount}, 0, 0, 0, '{DateTime.Today.ToString("MM.dd.yyyy")}');");
            
            await SqlManager.ExecuteNonQuery(
                $"UPDATE user_{user1}.user SET walkcoins = {challenge.User1.WalkCoins - betValue};");
            
            await NotificationsManager.SendNotifications(NotificationType.send_challenge_request, "Rzucono Ci wyzwanie!",
                $"Użytkownik {(await UserMethod.GetUserData(user1, false, false)).Username} rzucił ci wyzwanie o wartości {betValue} walkCoins!",
                $"walkwards://showUser/{user1}", new[] {user2.ToString()}, id);
            
            return challenge;
        }
        
        public static async Task<object> GetActiveChallenge(int uid)
        {
            int stepsUser1 = 0;
            int stepsUser2 = 0;
            //0 - odebrano nie zaakceptowane (nie aktywne)
            //1 - wysłano nie zaakceptowano (nie aktywne)
            //2 - wysłano zaakceptowano (aktywne)
            //3 - odebrano zaakceptowano (aktywne)

            var data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE recipient = {uid} AND accepted = false AND finished = false");
            List<ChallengeStatus> requests = new List<ChallengeStatus>();
            foreach (var item in data)
            {
                requests.Add(new ChallengeStatus(
                    new Challenge(item["id"], await UserMethod.GetUserData(item["sender"], false, false),
                        await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                        item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]), 0, 0, 0));
            }

            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE sender = {uid} AND accepted = false AND finished = false");
            foreach (var item in data)
            {
                requests.Add(new ChallengeStatus(
                    new Challenge(item["id"], await UserMethod.GetUserData(item["sender"], false, false),
                        await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                        item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]), 1, 0, 0));
            }

            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE sender = {uid} AND finished = false AND accepted = true;");
            foreach (var item in data)
            {
                Challenge challenge = new Challenge(item["id"],
                    await UserMethod.GetUserData(item["sender"], false, false),
                    await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                    item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]);

                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(challenge.StartDate)).TotalDays;
                var activityListUser1 = await challenge.User1.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser1)
                {
                    stepsUser1 += activity.y;
                }
                var activityListUser2 = await challenge.User2.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser2)
                {
                    stepsUser2 += activity.y;
                }

                ChallengeStatus cs = new ChallengeStatus(challenge, 2, stepsUser1, stepsUser2);
                requests.Add(cs);
            }
            
            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE recipient = {uid} AND finished = false AND accepted = true;");
            foreach (var item in data)
            {
                Challenge challenge = new Challenge(item["id"],
                    await UserMethod.GetUserData(item["sender"], false, false),
                    await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                    item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]);

                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(challenge.StartDate)).TotalDays;
                var activityListUser1 = await challenge.User1.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser1)
                {
                    stepsUser1 += activity.y;
                }
                var activityListUser2 = await challenge.User2.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser2)
                {
                    stepsUser2 += activity.y;
                }

                ChallengeStatus cs = new ChallengeStatus(challenge, 3, stepsUser1, stepsUser2);
                requests.Add(cs);
            }

            requests = requests.OrderBy(item => DateTime.Parse(item.CreateDate)).ThenBy(item => item.Hour).ToList();

            return requests;
        }

        public static async Task<object> GetActiveBetweenUsersChallenge(int sid, int rid)
        {
            int stepsUser1 = 0;
            int stepsUser2 = 0;
            //0 - odebrano nie zaakceptowane (nie aktywne)
            //1 - wysłano nie zaakceptowano (nie aktywne)
            //2 - wysłano zaakceptowano (aktywne)
            //3 - odebrano zaakceptowano (aktywne)

            var data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE (recipient = {sid} AND sender = {rid}) AND accepted = false AND finished = false");
            List<ChallengeStatus> requests = new List<ChallengeStatus>();
            foreach (var item in data)
            {
                requests.Add(new ChallengeStatus(
                    new Challenge(item["id"], await UserMethod.GetUserData(item["sender"], false, false),
                        await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                        item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]), 0, 0, 0));
            }

            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE (recipient = {rid} AND sender = {sid}) AND accepted = false AND finished = false");
            foreach (var item in data)
            {
                requests.Add(new ChallengeStatus(
                    new Challenge(item["id"], await UserMethod.GetUserData(item["sender"], false, false),
                        await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                        item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]), 1, 0, 0));
            }

            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE (recipient = {sid} AND sender = {rid}) AND finished = false AND accepted = true;");
            foreach (var item in data)
            {
                Challenge challenge = new Challenge(item["id"],
                    await UserMethod.GetUserData(item["sender"], false, false),
                    await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                    item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]
                    );


                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(challenge.StartDate)).TotalDays;
                var activityListUser1 = await challenge.User2.GetLastWeekActivity(days+1); //I know this fuck

                foreach (var activity in activityListUser1)
                {
                    stepsUser1 += activity.y;
                }
                var activityListUser2 = await challenge.User1.GetLastWeekActivity(days+1); //I know this fuck

                foreach (var activity in activityListUser2)
                {
                    stepsUser2 += activity.y;
                }

                ChallengeStatus cs = new ChallengeStatus(challenge, 2, stepsUser1, stepsUser2);
                requests.Add(cs);
            }
            
            data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE (recipient = {rid} AND sender = {sid}) AND finished = false AND accepted = true;");
            foreach (var item in data)
            {
                Challenge challenge = new Challenge(item["id"],
                    await UserMethod.GetUserData(item["sender"], false, false),
                    await UserMethod.GetUserData(item["recipient"], false, false), item["enddata"],
                    item["startdata"], item["accepted"], item["betvalue"], item["daycount"], item["user1steps"], item["user2steps"], item["createddate"], item["hour"]);

                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(challenge.StartDate)).TotalDays;
                var activityListUser1 = await challenge.User2.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser1)
                {
                    stepsUser1 += activity.y;
                }
                var activityListUser2 = await challenge.User1.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser2)
                {
                    stepsUser2 += activity.y;
                }

                ChallengeStatus cs = new ChallengeStatus(challenge, 3, stepsUser1, stepsUser2);
                requests.Add(cs);
            }

            requests = requests.OrderBy(item => DateTime.Parse(item.CreateDate)).ThenBy(item => item.Hour).ToList();

            return requests;
        }
        
        public static async Task<object> AcceptOrCancelChallengeRequest(int cid, bool accepted)
        {
            Challenge c = await GetChallenge(cid);
            if (accepted)
            {
                await SqlManager.ExecuteNonQuery($"UPDATE base.challenge SET accepted = true WHERE id = {cid}");
                await SqlManager.ExecuteNonQuery($"UPDATE user_{c.User2.Id}.user SET walkcoins= {c.User2.WalkCoins - c.BetValue};");

                string endDate = DateTime.Today.AddDays(c.DayCount).ToString("MM.dd.yyyy");
                int hour = DateTime.Now.Hour;

                await SqlManager.ExecuteFromList(new()
                {
                    $"UPDATE base.challenge SET enddata = '{endDate}' WHERE id = {cid};",
                    $"UPDATE base.challenge SET hour = {hour} WHERE id = {cid};",
                    $"UPDATE base.challenge SET startdata = '{DateTime.Today.ToString("MM.dd.yyyy")}' WHERE id = {cid};",
                });

                
                string sql = $"DELETE FROM user_{c.User2.Id}.notifications WHERE objid = {cid} AND type = 4;";
                await SqlManager.ExecuteNonQuery(sql);
                
                await NotificationsManager.SendNotifications(NotificationType.accept_challenge_request, "Zaakceptowano Twoje wyzwanie",
                    $"Użytkownik {c.User2.Username} zaakceptował Twoje wyzwanie! Do dzieła!",
                    $"walkwards://showUser/{c.User2.Id}", new[] {c.User1.Id.ToString()}, c.Id);
            }
            else
            {
                await SqlManager.ExecuteNonQuery($"DELETE FROM base.challenge WHERE id = {cid}");
                await SqlManager.ExecuteNonQuery($"UPDATE user_{c.User1.Id}.user SET walkcoins = {c.User1.WalkCoins + c.BetValue};");

                string sql = $"DELETE FROM user_{c.User2.Id}.notifications WHERE objid = {cid} AND type = 4;";
                await SqlManager.ExecuteNonQuery(sql);
                
                await NotificationsManager.SendNotifications(NotificationType.reject_friend_request, "Odrzucono Twoje wyzwanie",
                    $"Użytkownik {c.User2.Username} odzrucił Twoje wyzwanie :(",
                    $"walkwards://showUser/{c.User2.Id}", new[] {c.User1.Id.ToString()}, c.Id);
            }
            return true;
        }

        public static async Task<Challenge> GetChallenge(int cid)
        {
            var data = await SqlManager.Reader(
                $"SELECT * FROM base.challenge WHERE id = {cid};");

            if (data.Count == 0) throw new CustomError("ChallengeIsNotExist");
            
            return new Challenge(data[0]["id"], await UserMethod.GetUserData(data[0]["sender"]), await UserMethod.GetUserData(data[0]["recipient"]), data[0]["enddata"], data[0]["startdata"], data[0]["accepted"], data[0]["betvalue"], data[0]["daycount"], data[0]["user1steps"], data[0]["user2steps"], data[0]["createddate"], data[0]["hour"]);
        }
        
        public static async Task<object> GiveUpChallenge(int cid, int id)
        {
            Challenge c = await GetChallenge(cid);

            // TODO: praodopodobnie zly adresat, byla 500 wiec nie przetestowane
            await NotificationsManager.SendNotifications(NotificationType.give_up_challenge, "Twoj przeciwnik poddał się!",
                $"Użytkownik {(await UserMethod.GetUserData(c.User2.Id, false, false)).Username} poddał się z wyzwaniu z Tobą. Własnie wygrałeś {c.BetValue} walkCoins!",
                $"walkwards://showUser/{c.User2.Id}", new[] {c.User1.Id.ToString()}, c.Id);

            int winner;

            if (id == c.User1.Id) winner = c.User2.Id;
            else winner = c.User1.Id;

            double walkcoinsWinner = (await SqlManager.Reader($"SELECT * FROM user_{winner}.user"))[0]["walkcoins"];
            
            await SqlManager.ExecuteNonQuery($"UPDATE user_{winner}.user SET walkcoins = {walkcoinsWinner + (c.BetValue * 2)};");
            
            await SqlManager.ExecuteNonQuery($"UPDATE base.challenge SET finished = true WHERE id = {cid};");

            return true;
        }

        public static async Task<object> EndChallenge()
        {
            string date = DateTime.Now.ToString("MM.dd.yyyy");
            var data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE enddata = '{date}' AND hour = {DateTime.Now.Hour};");

            if (data.Count == 0) return true;

            foreach (var item in data)
            {
                Challenge challenge = await GetChallenge(item["id"]);
                User u1 = await UserMethod.GetUserData(item["sender"], false, false);
                User u2 = await UserMethod.GetUserData(item["recipient"], false, false);

                string endDate = item["enddata"].ToString().Split('.')[1] + "." +
                                 item["enddata"].ToString().Split('.')[0] + "." +
                                 item["enddata"].ToString().Split('.')[2];
                
                string startDate = item["startdata"].ToString().Split('.')[1] + "." +
                                 item["startdata"].ToString().Split('.')[0] + "." +
                                 item["startdata"].ToString().Split('.')[2];


                int stepsSumUser1 = 0;
                int stepsSumUser2 = 0;

                int days = (int)(DateTime.Parse(DateTime.Today.ToString("MM.dd.yyyy")) - DateTime.Parse(challenge.StartDate)).TotalDays;
                var activityListUser1 = await challenge.User1.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser1)
                {
                    stepsSumUser1 += activity.y;
                }
                var activityListUser2 = await challenge.User2.GetLastWeekActivity(days+1);

                foreach (var activity in activityListUser2)
                {
                    stepsSumUser2 += activity.y;
                }

                if (stepsSumUser1 > stepsSumUser2)
                {
                    await SqlManager.ExecuteNonQuery($"UPDATE user_{u1.Id}.user SET walkcoins = {u1.WalkCoins + (challenge.BetValue * 2)};");
                }
                else
                {
                    await SqlManager.ExecuteNonQuery($"UPDATE user_{u2.Id}.user SET walkcoins = {u2.WalkCoins + (challenge.BetValue * 2)};");
                }

                Console.WriteLine(stepsSumUser1 + " " + stepsSumUser2);
                await SqlManager.ExecuteFromList(new(){
                    $"UPDATE base.challenge SET finished = true WHERE id = {item["id"]};",
                    $"UPDATE base.challenge SET user1steps = {stepsSumUser1} WHERE id = {item["id"]};",
                    $"UPDATE base.challenge SET user2steps = {stepsSumUser2} WHERE id = {item["id"]};"
                });
                
                await NotificationsManager.SendNotifications(NotificationType.challenge_end, "Wyzwanie się zakończyło",
                    $"Wyzwanie z użytkwnikiem {challenge.User2.Username} zakończone! Wejdź do aplikacji i zobacz kto zwyciężył!",
                    $"walkwards://showUser/{challenge.User1.Id}", new[] {challenge.User1.Id.ToString()}, challenge.Id);
                
                await NotificationsManager.SendNotifications(NotificationType.challenge_end, "Wyzwanie się zakończyło",
                    $"Wyzwanie z użytkwnikiem {challenge.User1.Username} zakończone! Wejdź do aplikacji i zobacz kto zwyciężył!",
                    $"walkwards://showUser/{challenge.User2.Id}", new[] {challenge.User2.Id.ToString()}, challenge.Id); 
            }

            return true;
        }

        public static async Task<object> GetFinishedChallenges(int sender)
        {
            List<Challenge> challenges = new();

            var data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE (sender = {sender} OR recipient = {sender}) AND finished = true;");
            foreach (var item in data)
            {
                challenges.Add(await GetChallenge(item["id"]));
            }

            return challenges;
        }
        
        public static async Task<object> ExpiryChallenge()
        {
            var data = await SqlManager.Reader($"SELECT * FROM base.challenge WHERE accepted = false");
            foreach (var item in data)
            {
                Challenge challenges = await GetChallenge(item["id"]);

                await AcceptOrCancelChallengeRequest(challenges.Id, false);
            }
            
            return true;
        }
    }
}
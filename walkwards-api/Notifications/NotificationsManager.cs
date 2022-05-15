using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using walkwards_api.Challenge;
using walkwards_api.Competition;
using walkwards_api.Shop;
using walkwards_api.Sql;
using walkwards_api.UserManager;

namespace walkwards_api.Notifications
{
    public static class NotificationsManager
    {
        private static string APP_ID = "c69fc758-02ee-48a2-b67f-6e4068537471";
        private static string API_KEY = "NmE3NWJlNjQtMDZmZC00MmFmLWJhNDgtNTc3MDJhOGU3ZmYz";

        private const string ApiUrlNotification = "https://onesignal.com/api/v1/notifications";
        private const string ApiUrlDevices = "https://onesignal.com/api/v1/players";


        public static async Task<object?> SendNotifications (NotificationType type, string title, string message, string appUrl, string[] ids, long objid)
        {
            Notifications notification = new (APP_ID, API_KEY, ApiUrlNotification);
            await SaveNotifications(type, title, message, ids, objid);
            
            return await notification.SendToSpecificDevices(title, message, appUrl, ids);
        }
        
        public static async Task<JObject> GetAllDevices (int offset = 0)
        {
            Devices devices = new (APP_ID, API_KEY, ApiUrlDevices);
            return await devices.GetDevices(offset);
        }

        public static async Task SaveNotifications(NotificationType type, string title, string message, string[] ids, long objid)
        {
            //respons: accept_friend_request  reject_friend_request accept_challenge_request reject_challenge_request
            
            foreach (var item in ids)
            {
                int id = int.Parse(item);
                try
                {
                    Random rand = new Random();
                    await SqlManager.ExecuteNonQuery($"INSERT INTO user_{id}.notifications (type, title, message, date, objid) VALUES({(int) type}, '{title}', '{message}', '{DateTime.Now}', {objid});");
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    await SqlManager.ExecuteNonQuery(
                        $"create sequence user_{id}.notifications_id_seq as integer; alter table user_{id}.notifications alter column id set default nextval('user_{id}.notifications_id_seq'::regclass); alter sequence user_{id}.notifications_id_seq owned by user_{id}.notifications.id;");
                }
            }
        }

        public static async Task<object> GetNotifications(int id, int page, bool active)
        {
            int objPerPage = 20;
            var data = await SqlManager.Reader($"SELECT * FROM user_{id}.notifications ORDER BY id DESC LIMIT {(page + 1) * objPerPage} ;");
            List<object> result = new List<object>();

            foreach (var item in data)
            {
                NotificationType type = (NotificationType) item["type"];

                User user = await UserMethod.GetUserData(id, false, false);
                    
                // if (type == NotificationType.send_friend_requst && (await user.GetFriendShip(item["objid"]) == 2))
                // {
                // }
                    
                //if (type == NotificationType.accept_challenge_request || type == NotificationType.reject_challenge_request)
                //{
                //    if (await ChallengeManager.GetActiveBetweenUsersChallenge(item["objid"], id) == 2 || await ChallengeManager.GetActiveBetweenUsersChallenge(item["objid"], id) == 3)
                //    {
                //        await SqlManager.ExecuteNonQuery($"DELETE FROM user_{id}.notifications WHERE id = {item["id"]};");
                //    }
                //}
                //
                if (active)
                {

                    object obj = null;

                    //try
                    //{
                        obj = await GetObject((NotificationType) item["type"], item["objid"]);
                    //}
                    //catch (Exception e){}

                    result.Add(new
                    {
                        type = item["type"], title = item["title"], message = item["message"], date = item["date"], id=item["id"],
                        content = obj, 
                    });
                }
                else
                {
                    
                    type = (NotificationType) item["type"];
                    
                    if (type == NotificationType.send_friend_requst || type == NotificationType.send_challenge_request || type == NotificationType.auction_outbid || type == NotificationType.invite_to_guild || type == NotificationType.join_request_guild)
                    {

                        object obj = null;

                        try
                        {
                           obj = await GetObject((NotificationType) item["type"], item["objid"]);
                        }
                        catch (Exception e)
                        {
                        }
                        
                        result.Add(new
                        {
                            type = item["type"], title = item["title"], message = item["message"], date = item["date"], id = item["id"],
                            content = obj
                        });
                    }
                }
            }

            return result;
        }

        private static async Task<object> GetObject(NotificationType obj, long id)
        {
            //respons: accept_friend_request  reject_friend_request accept_challenge_request reject_challenge_request
            switch (obj)
            {
                case NotificationType.accept_friend_request: return await UserMethod.GetUserData((int) id);
                case NotificationType.send_friend_requst: return await UserMethod.GetUserData((int) id);
                case NotificationType.reject_friend_request: return await UserMethod.GetUserData((int) id);
                case NotificationType.remove_friend: return await UserMethod.GetUserData((int) id);
                case NotificationType.send_challenge_request: return await ChallengeManager.GetChallenge((int) id);
                case NotificationType.accept_challenge_request: return await ChallengeManager.GetChallenge((int) id);
                case NotificationType.reject_challenge_request: return await ChallengeManager.GetChallenge((int) id);
                case NotificationType.challenge_end: return await ChallengeManager.GetChallenge((int) id);
                case NotificationType.competition_end: return await CompetitionManager.GetCompetition((int) id);
                case NotificationType.auction_start: return await ShopManager.GetProduct((int) id);
                case NotificationType.auction_end: return await ShopManager.GetProduct((int) id);
                case NotificationType.auction_outbid: return await ShopManager.GetProduct((int) id);
                case NotificationType.invite_to_guild: return await GuildManager.GetGuild((int) id);
                case NotificationType.accepted_invite_to_guild: return await GuildManager.GetGuild((int) id);
                case NotificationType.reject_invite_to_guild: return await GuildManager.GetGuild((int) id);
                case NotificationType.join_request_guild:
                {
                    Console.WriteLine(int.Parse(id.ToString().Substring(0, 6)));
                    Console.WriteLine(int.Parse(id.ToString().Substring(6, 6)));
                    return new
                    {
                        guild = await GuildManager.GetGuild(int.Parse(id.ToString().Substring(0, 6))),
                        sender = await UserMethod.GetUserData(int.Parse(id.ToString().Substring(6, 6)))
                    };
                }
                default: return new object();
                //563201 726299
                //
            }
        }
            

        public static async Task<object> SendDailyNotification()
        {
            var data = await SqlManager.Reader("SELECT * FROM base.base");

            string[] ids = new string[data.Count];


            if (DateTime.Now.Hour != 20)
            {
                int i = 0;
                foreach (var item in data)
                {
                    User user = await UserMethod.GetUserData(item["id"], false, false);
                    if (user.WasActive == -1)
                    {
                        ids[i] = item["id"].ToString();
                    }
                    else
                    {
                        ids[i] = 0.ToString();
                        //ids[i] = item["id"].ToString();
                    }

                    i++;
                }

                List<string> filterIds = new List<string>();

                foreach (var item in ids)
                {
                    if (item != "0") filterIds.Add(item);
                }
                return await SendNotifications(NotificationType.send_daily_raport, "Zobacz swoją aktywność", $"Pamiętaj jeżeli nie wejdziesz dzisiaj do aplikacji nie będziesz brany pod uwagę w rankingu!", $"walkwards://", filterIds.ToArray(), 0);

            }
            else
            {
                List<string> id = new();
                foreach (var item in data)
                {
                    id.Add(item["id"].ToString());
                }
                return await SendNotifications(NotificationType.send_daily_raport, "Zobacz swoją aktywność", $"Pamiętaj jeżeli nie wejdziesz dzisiaj do aplikacji nie będziesz brany pod uwagę w rankingu!", $"walkwards://", id.ToArray(), 0);

            }

        }
    }
}
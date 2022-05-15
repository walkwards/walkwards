using System.Dynamic;
using System.Net;
using System.Net.Mail;
using walkwards_api.Challenge;
using walkwards_api.Notifications;
using walkwards_api.Sql;
using walkwards_api.structure;
using walkwards_api.UserManager;

namespace walkwards_api.Utilities;

public static class EndpointDevelopedDaily
{
    public static async Task<object> DevelopedDaily()
    {
        await ChallengeManager.EndChallenge();
        await Competition.CompetitionManager.EndCompetition();
        await ChallengeManager.ExpiryChallenge();
        await StatsManager.WriteStats();
        await SqlManager.ExecuteNonQuery($"DELETE FROM base.ranking;");
        await SqlManager.ExecuteNonQuery($"DELETE FROM base.rankingglobal;");

        var ids = await SqlManager.Reader("SELECT * FROM base.base");
        foreach (var item in ids)
        {
            int steps = (await (await UserMethod.GetUserData(item["id"]) as User)?.GetActivityCurrentDay()!).y;
            int stepsAll = (await (await UserMethod.GetUserData(item["id"]) as User)?.GetAllActivity()!).Sum(activity=>steps);
            
            
            await SqlManager.ExecuteNonQuery($"INSERT INTO base.ranking VALUES({item["id"]}, {steps});");
                    
            await SqlManager.ExecuteNonQuery($"INSERT INTO base.rankingglobal VALUES({item["id"]}, {stepsAll});");
        }
        var data = await SqlManager.Reader("SELECT * FROM base.rankingglobal");
        foreach (var item in data)
        {
            int steps = 0;
            User user = await UserMethod.GetUserData(item["id"], false, false);
            foreach (var acti in await user.GetAllActivity())
            {
                steps += acti.y;
            }
            await SqlManager.ExecuteNonQuery($"UPDATE base.rankingglobal SET steps = {steps} WHERE id = {item["id"]};");
        }
        
        if (DateTime.Now.Hour == 0)
        {
            await StatsManager.WriteDailyStats();

            await SqlManager.ExecuteNonQuery($"DELETE FROM base.ranking;");
            await SqlManager.ExecuteNonQuery($"DELETE FROM base.rankingglobal;");

            ids = await SqlManager.Reader("SELECT * FROM base.base");
            foreach (var item in ids)
            {
                int steps = await (await UserMethod.GetUserData(item["id"])).GetActivityCurrentDay();
                int stepsAll= await (await UserMethod.GetUserData(item["id"])).GetAllActivity();
                    
                    await SqlManager.ExecuteNonQuery($"INSERT INTO base.ranking VALUES({item["id"]}, {steps});");
                    
                    await SqlManager.ExecuteNonQuery($"INSERT INTO base.rankingglobal VALUES({item["id"]}, {stepsAll});");
            }
            
            data = await SqlManager.Reader("SELECT * FROM base.base");

            foreach (var item in data)
            {
                await SqlManager.ExecuteNonQuery($"UPDATE user_{item["id"]}.user SET wasactive = -1");
            }
        }
        if (DateTime.Now.Hour == 20) await NotificationsManager.SendDailyNotification();
        if (DateTime.Now.Hour == 23)
        {
            foreach (var item in data)
            {
                if (item["steps"] > 40000)
                {
                    User user = await UserMethod.GetUserData(item["id"]);
                    using MailMessage msg = new();
                    msg.From = new MailAddress("walkwardsdev@gmail.com");
                    msg.To.Add(user.Email);
                    msg.Subject = "Resetowanie hasła - WalkWards";
                    msg.Body = "Wal się oszuscie";
                    msg.Priority = MailPriority.High;
                    msg.IsBodyHtml = true;

                    using SmtpClient client = new ();
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("walkwardsdev@gmail.com", "uK8ujw@vH@qh2e6g!V_phWu*P!"); //fajne hasło xD 0k...?
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network; 
                    
                    await SqlManager.ExecuteNonQuery(
                        $"UPDATE user_{item["id"]}.user SET walkcoins = {user.WalkCoins - item["steps"] / 100}");
                    await NotificationsManager.SendNotifications(NotificationType.send_daily_raport, "Ostrzeżenie", "Wykryto próbę oszustwa", "walkwards://", new string[] {item["id"]}, 0);
                    
                    await SqlManager.ExecuteNonQuery(
                        $"UPDATE user_{item["id"]}.activity SET day = '{DateTime.Now.ToString("MM.dd.yyyy") + " ban"}'");
                }
            }  
        }

        await LoggerManager.WriteLog("Dupny endpoint co godzine się wykonał :)))");
        
        return true;
    }
    
}
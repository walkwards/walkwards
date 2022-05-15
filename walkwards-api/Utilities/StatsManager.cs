using Newtonsoft.Json;
using walkwards_api.Shop;
using walkwards_api.Sql;
using walkwards_api.UserManager;

namespace walkwards_api.Utilities;

public static class StatsManager
{
    public static async Task<object> WriteStats()
    {
        var data = await SqlManager.Reader("SELECT * FROM base.base");
        int steps = 0;

        int activity = 0;

        foreach (var item in data)
        {
            User user = await UserMethod.GetUserData(item["id"], false, false);
            steps += (await user.GetActivityCurrentDay()).y;  
            if(user.WasActive == -1) continue;
            activity++;
        }

        string sql =
            $"INSERT INTO stats.stats VALUES('{DateTime.Now.ToString("g")}', {steps}, {activity + 1});";
        
        await SqlManager.ExecuteNonQuery(sql);

        return steps;
    }

    public static async Task WriteDailyStats()
    {
        List<User> users = new List<User>();
        var data = await SqlManager.Reader("SELECT * FROM base.base");

        int stepsum = 0;
        
        int i = 0;
        foreach (var item in data)
        {
            users.Add(await UserMethod.GetUserData(item["id"], false, false));

            users[i].Steps = (await users[i].GetLastWeekActivity(2))[0].y;
            stepsum += users[i].Steps;
            i++;
        }

        await SqlManager.ExecuteNonQuery(
            $"INSERT INTO stats.dailystats VALUES('{DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy")}', {stepsum}, {users.OrderByDescending(i => i.Steps).ToList()[0].Id}, 0, 0)");
    }
}
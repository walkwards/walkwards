using walkwards_api.UserManager;

namespace walkwards_api.structure
{
    public class Activity
    {
        public Activity(){}
        public Activity(string day, int steps)
        {
            x = day;
            y = steps;
        }

        public string x = null!;
        public int y;
    }
    
    public class UserActivity 
    {
        public UserActivity(Activity activity, User user)
        {
            Day = activity.x;
            Steps = activity.y;
            Id = user.Id;
            Username = user.Username;
            Email = user.Email;
            PlaceInRanging = user.PlaceInRanging;
            IsActivated = user.IsActivated;
            AccoutPrivacy = user.AccoutPrivacy;
            Avatar = user.Avatar;
            Walkcoins = user.WalkCoins;
            PlaceInRangingGloabal = user.PlaceInRangingGloabal;
            PlaceGuildInRanking = user.PlaceGuildInRanking;
            PlaceUserInGuildRanking = user.PlaceUserInGuildRanking;
        }
        public int Id;
        public string Day;
        public int Steps;
        
        public string? Username;
        public string? Email;
        
        public int PlaceInRanging = 0;
        public bool IsActivated;
        public PrivacyType? AccoutPrivacy;
        public bool Avatar;
        public float Walkcoins;
        
        public int PlaceInRangingGloabal       {get; set ;}
        public int PlaceUserInGuildRanking     {get; set ;}
        public int PlaceGuildInRanking         {get; set ;}

    }
    

}
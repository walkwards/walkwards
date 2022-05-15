using walkwards_api.Challenge;
using walkwards_api.UserManager;

namespace walkwards_api.Challenge
{
    public class Challenge
    {
        public int Id;
        public User User1;
        public User User2;
        public int BetValue;
        public string EndDate;
        public string StartDate;
        public bool IsAccepted;
        public string CreateDate;
        public int DayCount;
        public int User1steps;
        public int User2steps;
        public int Hour;
        public int Status;

        public Challenge(int id, User user1, User user2, string endDate, string startDate, bool isAccepted,
            int betValue, int dayCount, int user1Steps, int user2Steps, string createDate, int hour, int status = 0)
        {
            Id = id;
            User1 = user1;
            User2 = user2;
            EndDate = endDate;
            StartDate = startDate;
            IsAccepted = isAccepted;
            BetValue = betValue;
            DayCount = dayCount;
            User1steps = user1Steps;
            User2steps = user2Steps;
            CreateDate = createDate;
            Hour = hour;
            Status = status;
        }

        public Challenge(int id, User user1, User user2, bool isAccepted, int betValue, int dayCount)
        {
            Id = id;
            User1 = user1;
            User2 = user2;
            IsAccepted = isAccepted;
            BetValue = betValue;
            DayCount = dayCount;
        }
    }


    public class ChallengeStatus
    {
        public int Id;
        public User User1;
        public User User2;
        public int BetValue;
        public string EndDate;
        public string StartDate;
        public bool IsAcepted;
        public int Status;
        public string CreateDate;
        public int ActualStepsUser1;
        public int ActualStepsUser2;
        public int Hour;
        public int DayCount;


        public ChallengeStatus(Challenge c, int status, int actualStepsUser1, int actualStepsUser2)
        {
            Id = c.Id;
            User1 = c.User1;
            User2 = c.User2;
            EndDate = c.EndDate;
            StartDate = c.StartDate;
            IsAcepted = c.IsAccepted;
            BetValue = c.BetValue;
            CreateDate = c.CreateDate;
            ActualStepsUser1 = actualStepsUser1;
            ActualStepsUser2 = actualStepsUser2;
            Status = status;
            Hour = c.Hour;
            DayCount = c.DayCount;
        }
    }
}

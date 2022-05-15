using walkwards_api.UserManager;

namespace walkwards_api.Competition
{
    public class Competition
    {
        public readonly int Id;

        public string StartDate;
        public readonly string EndDate;
        public List<User> Useres;
        public readonly string Name;
        public readonly string CompanyName;
        public readonly bool IsPublic;
        public readonly string Ctoken;
        public readonly bool IsOfficial;
        public readonly string Avatar;
        public readonly string Description;
        public readonly int EntranceFee;
        public int DayCount;
        public bool Joined { get; set; }

        public Competition(int id, string startDate, string endDate, List<User> useres, string name, string companyName,
            bool isPublic, string ctoken, bool isOfficial, string avatar, string description, int entranceFee)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate;
            Useres = useres;
            Name = name;
            CompanyName = companyName;
            IsPublic = isPublic;
            Ctoken = ctoken;
            IsOfficial = isOfficial;
            Avatar = avatar;
            Description = description;
            EntranceFee = entranceFee;
        }

        public Competition(int id, string startDate, string endDate, string name, string companyName, bool isPublic)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate;
            Name = name;
            CompanyName = companyName;
            IsPublic = isPublic;
        }
    }
}
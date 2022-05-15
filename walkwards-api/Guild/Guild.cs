using walkwards_api.UserManager;

namespace walkwards_api.Competition
{
    public class Guild
    {
        public int Id;

        public List<User> Users;
        public string Name;
        public User Creator;
        public int StepSum = 0;
        public int WalkcoinsSum = 0;
        public bool CanMembersAddMembers;

        public Guild(int id, List<User> useres, string name, User creator, bool canMembersAddMembers)
        {
            Id = id;
            Users = useres;
            Name = name;
            Creator = creator;
            CanMembersAddMembers = canMembersAddMembers;
        }

        public Guild(int id, string name, User creator)
        {
            Id = id;
            Name = name;
            Creator = creator;
        }

        public int StepAvg { get; set; }
    }
}
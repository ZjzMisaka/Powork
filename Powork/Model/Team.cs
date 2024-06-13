namespace Powork.Model
{
    public class Team
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<User> MemberList { get; set; }
        public string LastModifiedTime { get; set; }
    }
}

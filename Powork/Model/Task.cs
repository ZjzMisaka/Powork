namespace Powork.Model
{
    public class Task
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int StartDay { get; set; }
        public int Days { get; set; }
        public string Note { get; set; }
    }
}

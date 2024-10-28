using Microsoft.Bot.Schema;

namespace Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Destination { get; set; }
    }
}
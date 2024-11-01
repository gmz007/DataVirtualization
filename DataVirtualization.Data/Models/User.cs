namespace DataVirtualization.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public string Status { get; set; } = "active";
        public int Points { get; set; } = 0;
        public string SubscriptionType { get; set; } = "free";
    }
}

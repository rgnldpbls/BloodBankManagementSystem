namespace BBMS.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}

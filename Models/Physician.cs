namespace BBMS.Models
{
    public class Physician
    {
        public int Id { get; set; }
        public required string LicenseNo { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public required string Region { get; set; }
        public required string ContactNo { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required DateOnly DateCreated { get; set; }
    }
}

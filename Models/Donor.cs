using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BBMS.Models
{
    public class Donor
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Sex { get; set; }
        public required string City { get; set; }
        public required string Region { get; set; }
        public required DateOnly Birthdate { get; set; }
        public required string ContactNo { get; set; }
        public required string Email { get; set; }
        public required string Password {  get; set; }
        public required DateOnly DateCreated { get; set; }
    }
}

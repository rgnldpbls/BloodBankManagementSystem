using BBMS.Models;

namespace BBMS.ViewModels
{
    public class BloodDonateIndexVM
    {
        public int Id { get; set; }
        public required int Age { get; set; }
        public required string BloodType { get; set; }
        public required int UnitNo { get; set; }
        public required string Status { get; set; }
        public required DateOnly DonateDate { get; set; }
        public required string DonatePlace { get; set; }
        public int DonorId { get; set; }
        public Donor? Donor { get; set; }
        public string DonorName { get; set; }
    }
}

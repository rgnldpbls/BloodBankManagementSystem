namespace BBMS.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public required string BloodType { get; set; }
        public required int Quantity { get; set; }
        public required DateOnly ExpiryDate { get; set; }
        public int RequestId { get; set; }
        public BloodRequest? Request { get; set; }
        public int DonorId { get; set; }
        public Donor? Donor { get; set; }
    }
}

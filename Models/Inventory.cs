namespace BBMS.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public string BloodType { get; set; }
        public int Quantity { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public DateOnly LastUpdated { get; set; }
    }
}

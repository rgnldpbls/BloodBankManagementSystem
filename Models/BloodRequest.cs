using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBMS.Models
{
    public class BloodRequest
    {
        public int Id { get; set; }
        public required string PatientName { get; set; }
        public required int PatientAge { get; set; }
        public required string RequestInfo { get; set; }
        public required string PatientHospital { get; set; }
        public required string BloodType {  get; set; }
        public required int UnitNo { get; set; }
        public required string Status { get; set; }
        public required DateOnly RequestDate { get; set; }
        public int PhysicianId { get; set; }
        public Physician? Physician {  get; set; }
    }
}

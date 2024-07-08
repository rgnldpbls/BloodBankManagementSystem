using BBMS.Data;
using BBMS.Models;
using Microsoft.EntityFrameworkCore;

namespace BBMS.Services
{
    public class InventoryService
    {
        private readonly BloodBankDBContext _context;
        public InventoryService(BloodBankDBContext context)
        {
            _context = context;
        }

        public async Task AcceptBloodDonationAsync(int id)
        {
            var bloodDonate = await _context.BloodDonate.FindAsync(id);
            if(bloodDonate == null)
            {
                throw new Exception("Blood Donation not found.");
            }
            if(bloodDonate.Status == "Accepted")
            {
                DateOnly expiryDate = bloodDonate.DonateDate.AddDays(42);

                var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.BloodType == bloodDonate.BloodType && i.ExpiryDate == expiryDate);

                if(inventory != null)
                {
                    inventory.Quantity += bloodDonate.UnitNo;
                    inventory.LastUpdated = DateOnly.FromDateTime(DateTime.Now);
                }
                else
                {
                    _context.Inventory.Add(new Inventory
                    {
                        BloodType = bloodDonate.BloodType,
                        Quantity = bloodDonate.UnitNo,
                        ExpiryDate = expiryDate,
                        LastUpdated = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task AcceptBloodRequestAsync(int id)
        {
            var bloodRequest = await _context.BloodRequest.FindAsync(id);
            if (bloodRequest == null)
            {
                throw new Exception("Blood Request not found.");
            }
            if(bloodRequest.Status == "Accepted")
            {
                int remainingQty = bloodRequest.UnitNo;

                var inventories = await _context.Inventory.Where(i => i.BloodType == bloodRequest.BloodType && i.ExpiryDate 
                >= DateOnly.FromDateTime(DateTime.Now)).OrderBy(i => i.ExpiryDate).ToListAsync();

                foreach(var inventory in inventories)
                {
                    if(remainingQty <= 0)
                    {
                        break;
                    }
                    if(inventory.Quantity >= remainingQty)
                    {
                        inventory.Quantity -= remainingQty;
                        inventory.LastUpdated = DateOnly.FromDateTime(DateTime.Now);
                        remainingQty = 0;
                    }
                    else
                    {
                        remainingQty -= inventory.Quantity;
                        inventory.Quantity = 0;
                        inventory.LastUpdated = DateOnly.FromDateTime(DateTime.Now);
                    }

                    if(inventory.Quantity == 0)
                    {
                        _context.Inventory.Remove(inventory);
                    }
                }
                if(remainingQty > 0)
                {
                    bloodRequest.Status = "Approved";
                    throw new Exception("Not enough blood available to fulfill the request.");
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BBMS.Data;
using BBMS.Models;
using Microsoft.AspNetCore.Authorization;
using BBMS.Services;
using BBMS.ViewModels;

namespace BBMS.Controllers
{
    public class BloodDonateController : Controller
    {
        private readonly BloodBankDBContext _context;
        private readonly AccountService _accountService;
        private readonly InventoryService _inventoryService;

        public BloodDonateController(BloodBankDBContext context, AccountService accountService, InventoryService inventoryService)
        {
            _context = context;
            _accountService = accountService;
            _inventoryService = inventoryService;
        }

        // GET: BloodDonate
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 4)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodDonations = _context.BloodDonate.Include(b => b.Donor)
                .Where(b => b.Status == "Accepted" || b.Status == "Rejected")
                .Select(b => new BloodDonateIndexVM
                {
                    Id = b.Id,
                    Age = b.Age,
                    BloodType = b.BloodType,
                    UnitNo = b.UnitNo,
                    Status = b.Status,
                    DonateDate = b.DonateDate,
                    DonatePlace = b.DonatePlace,
                    DonorId = b.DonorId,
                    DonorName = b.Donor.Name
                });
            var totalItems = await bloodDonations.CountAsync();
            var items = await bloodDonations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<BloodDonateIndexVM>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: BloodDonate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodDonate = await _context.BloodDonate
                .Include(b => b.Donor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodDonate == null)
            {
                return NotFound();
            }

            return View(bloodDonate);
        }

        // GET: BloodDonate/Create
        [Authorize(Roles = "Donor")]
        public IActionResult Create()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewBag.DonorId = Convert.ToInt32(accountId.Value);
                ViewData["AccountId"] = accountId.ToString();
            }
            return View();
        }

        // POST: BloodDonate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> Create([Bind("Id,Age,BloodType,UnitNo,Status,DonateDate,DonatePlace,DonorId")] BloodDonate bloodDonate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bloodDonate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DonateLogs));
            }
            return View(bloodDonate);
        }

        // GET: BloodDonate/Edit/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin,ValidatorAdmin,InventoryAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            else if (User.IsInRole("ValidatorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutValidator.cshtml";
            }else if (User.IsInRole("InventoryAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutInventoryAdmin.cshtml";
            }
            if (id == null)
            {
                return NotFound();
            }

            var bloodDonate = await _context.BloodDonate.FindAsync(id);
            if (bloodDonate == null)
            {
                return NotFound();
            }
            return View(bloodDonate);
        }

        // POST: BloodDonate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,DonorAdmin,ValidatorAdmin,InventoryAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Age,BloodType,UnitNo,Status,DonateDate,DonatePlace,DonorId")] BloodDonate bloodDonate)
        {
            if (id != bloodDonate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloodDonate);
                    await _inventoryService.AcceptBloodDonationAsync(id);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloodDonateExists(bloodDonate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if(User.IsInRole("SuperAdmin") || User.IsInRole("DonorAdmin")){
                    return RedirectToAction(nameof(PendingDonate));
                }else if (User.IsInRole("ValidatorAdmin")){
                    return RedirectToAction(nameof(ValidateDonate));
                }else if (User.IsInRole("InventoryAdmin")){
                    return RedirectToAction(nameof(ApproveDonate));
                }
            }
            return View(bloodDonate);
        }

        // GET: BloodDonate/Delete/5
        [Authorize(Roles = "SuperAdmin,Donor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            else if (User.IsInRole("Donor"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonor.cshtml";
            }
            if (id == null)
            {
                return NotFound();
            }

            var bloodDonate = await _context.BloodDonate
                .Include(b => b.Donor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodDonate == null)
            {
                return NotFound();
            }
            return View(bloodDonate);
        }

        // POST: BloodDonate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,DonorAdmin,Donor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloodDonate = await _context.BloodDonate.FindAsync(id);
            if (bloodDonate != null)
            {
                _context.BloodDonate.Remove(bloodDonate);
            }

            await _context.SaveChangesAsync();
            if(User.IsInRole("SuperAdmin") || User.IsInRole("DonorAdmin"))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(DonateLogs));
            }
        }

        private bool BloodDonateExists(int id)
        {
            return _context.BloodDonate.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> DonateLogs(int pageNumber = 1, int pageSize = 4)
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodDonate.Include(b => b.Donor).Where(b => b.DonorId == accountId.Value);
            var totalItems = await bloodBankDBContext.CountAsync();
            var items = await bloodBankDBContext.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<BloodDonate>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> UpdateDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodDonate = await _context.BloodDonate.FindAsync(id);
            if (bloodDonate == null)
            {
                return NotFound();
            }
            return View(bloodDonate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> UpdateDetails(int id, [Bind("Id,Age,BloodType,UnitNo,Status,DonateDate,DonatePlace,DonorId")] BloodDonate bloodDonate)
        {
            if (id != bloodDonate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloodDonate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloodDonateExists(bloodDonate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(DonateLogs));
            }
            return View(bloodDonate);
        }

        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> PendingDonate(int pageNumber = 1, int pageSize = 4)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodDonations = _context.BloodDonate.Include(b => b.Donor)
                .Where(b => b.Status == "Pending")
                .Select(b => new BloodDonateIndexVM
                {
                    Id = b.Id,
                    Age = b.Age,
                    BloodType = b.BloodType,
                    UnitNo = b.UnitNo,
                    Status = b.Status,
                    DonateDate = b.DonateDate,
                    DonatePlace = b.DonatePlace,
                    DonorId = b.DonorId,
                    DonorName = b.Donor.Name
                });
            var totalItems = await bloodDonations.CountAsync();
            var items = await bloodDonations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<BloodDonateIndexVM>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        [Authorize(Roles = "ValidatorAdmin")]
        public async Task<IActionResult> ValidateDonate(int pageNumber = 1, int pageSize = 4)
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodDonations = _context.BloodDonate.Include(b => b.Donor)
                .Where(b => b.Status == "Pre-Approved")
                .Select(b => new BloodDonateIndexVM
                {
                    Id = b.Id,
                    Age = b.Age,
                    BloodType = b.BloodType,
                    UnitNo = b.UnitNo,
                    Status = b.Status,
                    DonateDate = b.DonateDate,
                    DonatePlace = b.DonatePlace,
                    DonorId = b.DonorId,
                    DonorName = b.Donor.Name
                });
            var totalItems = await bloodDonations.CountAsync();
            var items = await bloodDonations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<BloodDonateIndexVM>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        [Authorize(Roles = "InventoryAdmin")]
        public async Task<IActionResult> ApproveDonate(int pageNumber = 1, int pageSize = 4)
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodDonations = _context.BloodDonate.Include(b => b.Donor)
                .Where(b => b.Status == "Approved")
                .Select(b => new BloodDonateIndexVM
                {
                    Id = b.Id,
                    Age = b.Age,
                    BloodType = b.BloodType,
                    UnitNo = b.UnitNo,
                    Status = b.Status,
                    DonateDate = b.DonateDate,
                    DonatePlace = b.DonatePlace,
                    DonorId = b.DonorId,
                    DonorName = b.Donor.Name
                });
            var totalItems = await bloodDonations.CountAsync();
            var items = await bloodDonations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<BloodDonateIndexVM>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }
    }
}

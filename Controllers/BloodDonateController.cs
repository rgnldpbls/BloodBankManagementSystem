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

        public BloodDonateController(BloodBankDBContext context, AccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        // GET: BloodDonate
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Index()
        {
            var bloodDonations = await _context.BloodDonate.Include(b => b.Donor).Select(b => new BloodDonateIndexVM
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
            }).Where(b => b.Status != "Pending").ToListAsync();
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
            return View(bloodDonations);
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
            /*ViewData["DonorId"] = new SelectList(_context.Donor, "Id", "Id");*/
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
                return RedirectToAction(nameof(Index));
            }
           /* ViewData["DonorId"] = new SelectList(_context.Donor, "Id", "Id", bloodDonate.DonorId);*/
            return View(bloodDonate);
        }

        // GET: BloodDonate/Edit/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Edit(int? id)
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
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            return View(bloodDonate);
        }

        // POST: BloodDonate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
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
                return RedirectToAction(nameof(Index));
            }
            return View(bloodDonate);
        }

        // GET: BloodDonate/Delete/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Delete(int? id)
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
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            return View(bloodDonate);
        }

        // POST: BloodDonate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloodDonate = await _context.BloodDonate.FindAsync(id);
            if (bloodDonate != null)
            {
                _context.BloodDonate.Remove(bloodDonate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BloodDonateExists(int id)
        {
            return _context.BloodDonate.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> DonateLogs()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodDonate.Include(b => b.Donor).Where(b => b.DonorId == accountId.Value);
            return View(await bloodBankDBContext.ToListAsync());
        }
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> PendingDonate()
        {
            var bloodDonations = await _context.BloodDonate.Include(b => b.Donor).Select(b => new BloodDonateIndexVM
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
            }).Where(b => b.Status == "Pending").ToListAsync();
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
            return View(bloodDonations);
        }
    }
}

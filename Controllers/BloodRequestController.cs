using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BBMS.Data;
using BBMS.Models;
using BBMS.Services;
using Microsoft.AspNetCore.Authorization;

namespace BBMS.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly BloodBankDBContext _context;
        private readonly AccountService _accountService;

        public BloodRequestController(BloodBankDBContext context, AccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        // GET: BloodRequest
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodRequest.Include(b => b.Physician).Where(b => b.Status == "Accepted" || b.Status == "Rejected");
            return View(await bloodBankDBContext.ToListAsync());
        }

        // GET: BloodRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodRequest = await _context.BloodRequest
                .Include(b => b.Physician)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodRequest == null)
            {
                return NotFound();
            }

            return View(bloodRequest);
        }

        // GET: BloodRequest/Create
        [Authorize(Roles = "Physician")]
        public IActionResult Create()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewBag.PhysicianId = Convert.ToInt32(accountId.Value);
                ViewData["AccountId"] = accountId.ToString();
            }
            return View();
        }

        // POST: BloodRequest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> Create([Bind("Id,PatientName,PatientAge,RequestInfo,PatientHospital,BloodType,UnitNo,Status,RequestDate,PhysicianId")] BloodRequest bloodRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bloodRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RequestLogs));
            }
            return View(bloodRequest);
        }

        // GET: BloodRequest/Edit/5
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin,ValidatorAdmin,InventoryAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }else if (User.IsInRole("ValidatorAdmin"))
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

            var bloodRequest = await _context.BloodRequest.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound();
            }
            return View(bloodRequest);
        }

        // POST: BloodRequest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin,ValidatorAdmin,InventoryAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientName,PatientAge,RequestInfo,PatientHospital,BloodType,UnitNo,Status,RequestDate,PhysicianId")] BloodRequest bloodRequest)
        {
            if (id != bloodRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloodRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloodRequestExists(bloodRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (User.IsInRole("SuperAdmin") || User.IsInRole("PhysicianAdmin"))
                {
                    return RedirectToAction(nameof(PendingRequest));
                } else if (User.IsInRole("ValidatorAdmin")) {
                    return RedirectToAction(nameof(ValidateRequest));
                } else if (User.IsInRole("InventoryAdmin")) {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(bloodRequest);
        }

        // GET: BloodRequest/Delete/5
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin,Physician")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            else if (User.IsInRole("Physician"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysician.cshtml";
            }
            if (id == null)
            {
                return NotFound();
            }

            var bloodRequest = await _context.BloodRequest
                .Include(b => b.Physician)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodRequest == null)
            {
                return NotFound();
            }

            return View(bloodRequest);
        }

        // POST: BloodRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin,Physician")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloodRequest = await _context.BloodRequest.FindAsync(id);
            if (bloodRequest != null)
            {
                _context.BloodRequest.Remove(bloodRequest);
            }

            await _context.SaveChangesAsync();
            if (User.IsInRole("SuperAdmin") || User.IsInRole("PhysicianAdmin"))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(RequestLogs));
            }
        }

        private bool BloodRequestExists(int id)
        {
            return _context.BloodRequest.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> RequestLogs()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodRequest.Include(b => b.Physician).Where(b => b.PhysicianId == accountId.Value);
            return View(await bloodBankDBContext.ToListAsync());
        }

        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> UpdateDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodRequest = await _context.BloodRequest.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound();
            }
            return View(bloodRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> UpdateDetails(int id, [Bind("Id,PatientName,PatientAge,RequestInfo,PatientHospital,BloodType,UnitNo,Status,RequestDate,PhysicianId")] BloodRequest bloodRequest)
        {
            if (id != bloodRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloodRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloodRequestExists(bloodRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(RequestLogs));
            }
            return View(bloodRequest);
        }

        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> PendingRequest()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodRequest.Include(b => b.Physician).Where(b => b.Status == "Pending");
            return View(await bloodBankDBContext.ToListAsync());
        }

        [Authorize(Roles = "ValidatorAdmin")]
        public async Task<IActionResult> ValidateRequest()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodRequest.Include(b => b.Physician).Where(b => b.Status == "Pre-Approved");
            return View(await bloodBankDBContext.ToListAsync());
        }

        [Authorize(Roles = "InventoryAdmin")]
        public async Task<IActionResult> ApproveRequest()
        {
            int? accountId = _accountService.GetAccountId();
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.ToString();
            }
            var bloodBankDBContext = _context.BloodRequest.Include(b => b.Physician).Where(b => b.Status == "Approved");
            return View(await bloodBankDBContext.ToListAsync());
        }
    }
}

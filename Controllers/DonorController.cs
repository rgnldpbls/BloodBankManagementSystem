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
using BBMS.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BBMS.Controllers
{
    public class DonorController : Controller
    {
        private readonly BloodBankDBContext _context;

        public DonorController(BloodBankDBContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var donor = await _context.Donor.SingleOrDefaultAsync(a => a.Email == model.Email);
                if (donor != null && PasswordHelper.VerifyPassword(model.Password, donor.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, donor.Email),
                        new Claim(ClaimTypes.Role, "Donor"),
                        new Claim("AccountId", donor.Id.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "Login");
                    var authProperties = new AuthenticationProperties();
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Profile", "Donor", new { id = donor.Id.ToString() });
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt!");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Donor");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Donor
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            return View(await _context.Donor.ToListAsync());
        }

        // GET: Donor/Details/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donor = await _context.Donor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donor == null)
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
            return View(donor);
        }

        // GET: Donor/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout2.cshtml"; ;
            }
            return View();
        }

        // POST: Donor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Id,Name,Sex,City,Region,Birthdate,ContactNo,Email,Password,DateCreated")] Donor donor)
        {
            if (ModelState.IsValid)
            {
                donor.Password = PasswordHelper.HashPassword(donor.Password);
                donor.DateCreated = DateOnly.FromDateTime(DateTime.Now);
                _context.Add(donor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(donor);
        }

        // GET: Donor/Edit/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donor = await _context.Donor.FindAsync(id);
            if (donor == null)
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
            return View(donor);
        }

        // POST: Donor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sex,City,Region,Birthdate,ContactNo,Email,Password,DateCreated")] Donor donor)
        {
            if (id != donor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    donor.Password = PasswordHelper.HashPassword(donor.Password);
                    donor.DateCreated = donor.DateCreated;
                    _context.Update(donor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonorExists(donor.Id))
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
            return View(donor);
        }

        // GET: Donor/Delete/5
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donor = await _context.Donor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donor == null)
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
            return View(donor);
        }

        // POST: Donor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,DonorAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donor = await _context.Donor.FindAsync(id);
            if (donor != null)
            {
                _context.Donor.Remove(donor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonorExists(int id)
        {
            return _context.Donor.Any(e => e.Id == id);
        }
        private string GetUserId()
        {
            return User.FindFirstValue("AccountId");
        }

        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> Profile(string? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var donor = await _context.Donor.FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if(donor == null)
            {
                return NotFound();
            }
            var curUserId = GetUserId();
            if (id != curUserId)
            {
                return Forbid(); // 403 Forbidden
            }
            return View(donor);
        }

        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> EditAccount(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var donor = await _context.Donor.FindAsync(id);
            if(donor == null)
            {
                return NotFound();
            }
            return View(donor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> EditAccount(int id, [Bind("Id,Name,Sex,City,Region,Birthdate,ContactNo,Email,Password,DateCreated")] Donor donor)
        {
            if(id != donor.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    donor.Password = PasswordHelper.HashPassword(donor.Password);
                    donor.DateCreated = donor.DateCreated;
                    _context.Update(donor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonorExists(donor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Profile", "Donor", new { id = donor.Id.ToString() });
            }
            return View(donor);
        }
    }
}

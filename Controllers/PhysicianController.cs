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
using System.Drawing;
using BBMS.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BBMS.Services;

namespace BBMS.Controllers
{
    public class PhysicianController : Controller
    {
        private readonly BloodBankDBContext _context;
        private readonly AccountService _accountService;

        public PhysicianController(BloodBankDBContext context, AccountService accountService)
        {
            _context = context;
            _accountService = accountService;
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
                var physician = await _context.Physician.SingleOrDefaultAsync(a => a.Email == model.Email);
                if (physician != null && PasswordHelper.VerifyPassword(model.Password, physician.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, physician.Email),
                        new Claim(ClaimTypes.Role, "Physician"),
                        new Claim("AccountId", physician.Id.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "Login");
                    var authProperties = new AuthenticationProperties();
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Profile", "Physician", new { id = physician.Id.ToString() });
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt!");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Physician");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Physician
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 4)
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
            var totalItems = await _context.Physician.CountAsync();
            var items = await _context.Physician.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new PaginatedViewModel<Physician>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Physician/Details/5
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            if (id == null)
            {
                return NotFound();
            }

            var physician = await _context.Physician
                .FirstOrDefaultAsync(m => m.Id == id);
            if (physician == null)
            {
                return NotFound();
            }
            return View(physician);
        }

        // GET: Physician/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout2.cshtml"; ;
            }
            ViewBag.DateNow = DateOnly.FromDateTime(DateTime.Now);
            return View();
        }

        // POST: Physician/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Id,LicenseNo,Name,City,Region,ContactNo,Email,Password,DateCreated")] Physician physician)
        {
            if (ModelState.IsValid)
            {
                physician.Password = PasswordHelper.HashPassword(physician.Password);
                _context.Add(physician);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(physician);
        }

        // GET: Physician/Edit/5
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            }
            else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            }
            if (id == null)
            {
                return NotFound();
            }

            var physician = await _context.Physician.FindAsync(id);
            if (physician == null)
            {
                return NotFound();
            }
            return View(physician);
        }

        // POST: Physician/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LicenseNo,Name,City,Region,ContactNo,Email,Password,DateCreated")] Physician physician)
        {
            if (id != physician.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    physician.Password = PasswordHelper.HashPassword(physician.Password);
                    _context.Update(physician);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhysicianExists(physician.Id))
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
            return View(physician);
        }

        // GET: Physician/Delete/5
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
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
            if (id == null)
            {
                return NotFound();
            }

            var physician = await _context.Physician
                .FirstOrDefaultAsync(m => m.Id == id);
            if (physician == null)
            {
                return NotFound();
            }
            return View(physician);
        }

        // POST: Physician/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,PhysicianAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var physician = await _context.Physician.FindAsync(id);
            if (physician != null)
            {
                _context.Physician.Remove(physician);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhysicianExists(int id)
        {
            return _context.Physician.Any(e => e.Id == id);
        }
        private string GetUserId()
        {
            return User.FindFirstValue("AccountId");
        }

        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> Profile(string? id)
        {
            var curUserId = GetUserId();
            if (id != curUserId)
            {
                return Forbid(); // 403 Forbidden
            }
            if (id == null)
            {
                return NotFound();
            }
            var physician = await _context.Physician.FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if(physician == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = curUserId;
            return View(physician);
        }

        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> EditAccount(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var physician = await _context.Physician.FindAsync(id);
            if (physician == null)
            {
                return NotFound();
            }
            return View(physician);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Physician")]
        public async Task<IActionResult> EditAccount(int id, [Bind("Id,LicenseNo,Name,City,Region,ContactNo,Email,Password,DateCreated")] Physician physician)
        {
            if(id != physician.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    physician.Password = PasswordHelper.HashPassword(physician.Password);
                    physician.DateCreated = physician.DateCreated;
                    _context.Update(physician);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhysicianExists(physician.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Profile", "Physician", new { id = physician.Id.ToString() });
            }
            return View(physician);
        }
    }
}

using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BBMS.Data;
using BBMS.Models;
using BBMS.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace BBMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly BloodBankDBContext _context;

        public AdminController(BloodBankDBContext context)
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Username == model.Username);
                if (admin != null && PasswordHelper.VerifyPassword(model.Password, admin.Password))
                {
                    switch (admin.Type)
                    {
                        case "SuperAdmin":
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, admin.Username),
                                new Claim(ClaimTypes.Role, "SuperAdmin"),
                                new Claim("AccountId", admin.Id.ToString())
                            };
                            var claimsIdentity = new ClaimsIdentity(claims, "Login");
                            var authProperties = new AuthenticationProperties();
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                            return RedirectToAction("Profile", "Admin", new { id = admin.Id.ToString() });
                        case "DonorAdmin":
                            var claims2 = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, admin.Username),
                                new Claim(ClaimTypes.Role, "DonorAdmin"),
                                new Claim("AccountId", admin.Id.ToString())
                            };
                            var claimsIdentity2 = new ClaimsIdentity(claims2, "Login");
                            var authProperties2 = new AuthenticationProperties();
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity2));
                            return RedirectToAction("Profile", "Admin", new { id = admin.Id.ToString() });
                        case "PhysicianAdmin":
                            var claims3 = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, admin.Username),
                                new Claim(ClaimTypes.Role, "PhysicianAdmin"),
                                new Claim("AccountId", admin.Id.ToString())
                            };
                            var claimsIdentity3 = new ClaimsIdentity(claims3, "Login");
                            var authProperties3 = new AuthenticationProperties();
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity3));
                            return RedirectToAction("Profile", "Admin", new { id = admin.Id.ToString() });
                        case "InventoryAdmin":
                            var claims4 = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, admin.Username),
                                new Claim(ClaimTypes.Role, "InventoryAdmin"),
                                new Claim("AccountId", admin.Id.ToString())
                            };
                            var claimsIdentity4 = new ClaimsIdentity(claims4, "Login");
                            var authProperties4 = new AuthenticationProperties();
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity4));
                            return RedirectToAction("Profile", "Admin", new { id = admin.Id.ToString() });
                        case "RequestValidatorAdmin":
                            var claims5 = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, admin.Username),
                                new Claim(ClaimTypes.Role, "RequestValidatorAdmin"),
                                new Claim("AccountId", admin.Id.ToString())
                            };
                            var claimsIdentity5 = new ClaimsIdentity(claims5, "Login");
                            var authProperties5 = new AuthenticationProperties();
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity5));
                            return RedirectToAction("Profile", "Admin", new { id = admin.Id.ToString() });
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt!");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Admin
        public async Task<IActionResult> Index()
        {
            return View(await _context.Admins.ToListAsync());
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Type,Username,Password")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.Password = PasswordHelper.HashPassword(admin.Password);
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        // GET: Admin/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Username,Password")] Admin admin)
        {
            if (id != admin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    admin.Password = PasswordHelper.HashPassword(admin.Password);
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id))
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
            return View(admin);
        }

        // GET: Admin/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            return User.FindFirstValue("AccountId");
        }

        [Authorize(Roles = "SuperAdmin,DonorAdmin,PhysicianAdmin,InventoryAdmin,RequestValidatorAdmin")]
        public async Task<IActionResult> Profile(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var admin = await _context.Admins.FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if (admin == null)
            {
                return NotFound();
            }
            if (User.IsInRole("SuperAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutSuperAdmin.cshtml";
            } else if (User.IsInRole("DonorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutDonorAdmin.cshtml";
            } else if (User.IsInRole("PhysicianAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutPhysicianAdmin.cshtml";
            } else if (User.IsInRole("InventoryAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutInventoryAdmin.cshtml";
            } else if (User.IsInRole("RequestValidatorAdmin"))
            {
                ViewData["Layout"] = "~/Views/Shared/_LayoutRequestV.cshtml";
            }
            var curUserId = GetUserId();
            if (id != curUserId)
            {
                return Forbid(); // 403 Forbidden
            }
            return View(admin);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BBMS.Models;

namespace BBMS.Data
{
    public class BloodBankDBContext : DbContext
    {
        public BloodBankDBContext (DbContextOptions<BloodBankDBContext> options)
            : base(options)
        {
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Donor> Donor { get; set; }
        public DbSet<Physician> Physician { get; set; }
        public DbSet<BloodDonate> BloodDonate { get; set; }
    }
}

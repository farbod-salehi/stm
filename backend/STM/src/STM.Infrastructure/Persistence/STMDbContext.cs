using STM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Infrastructure.Persistence
{
    public class STMDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }

        public STMDbContext(DbContextOptions<STMDbContext> options) : base(options) { }
    }
}

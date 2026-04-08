using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=aws-1-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.bpdsydscpveigfwrpbmo;Password=MBhTdJmhqrcSgLMQ;SslMode=Require");
            optionsBuilder.EnableSensitiveDataLogging();

            return new ApplicationDbContext(optionsBuilder.Options, null, null);
        }
    }
}

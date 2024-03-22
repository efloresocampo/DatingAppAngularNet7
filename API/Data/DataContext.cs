using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Data
{
    public class DataContext: DbContext
    {
        private readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration){
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlServer(_configuration.GetConnectionString("WebApiDatabase"), 
            sqlServer => sqlServer.UseDateOnlyTimeOnly());

        public DbSet<AppUser>? Users { get; set; }
    }
}
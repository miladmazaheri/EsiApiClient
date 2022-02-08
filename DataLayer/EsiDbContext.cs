using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class EsiDbContext : DbContext
    {
        public static string DbPath { get; private set; }

        public DbSet<Reservation> Reservations { get; set; }
        public EsiDbContext()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}esi.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Reservation>(r =>
            {
                r.HasKey(k => k.Id);
                r.HasIndex(k => new { k.Date, k.Cod_Meal, k.Reciver_Coupon_Id, k.Num_Ide });
               
            });
        }
    }
}

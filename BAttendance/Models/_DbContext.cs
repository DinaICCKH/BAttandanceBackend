using Microsoft.EntityFrameworkCore;

namespace BAttendance.Models
{
    public class _DbContext : DbContext
    {
        public _DbContext(DbContextOptions<_DbContext> options) : base(options) { }

        public DbSet<BranchViewModel> BranchViewModels { get; set; }
        public DbSet<LoginResult> LoginResults { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<SpResult> SpResults { get; set; }

        public DbSet<StaffFaceEntity> StaffFaceEntitys { get; set; }
        public DbSet<StaffEnableSettingResult> StaffEnableSettingResults { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchViewModel>().HasNoKey();
            modelBuilder.Entity<LoginResult>().HasNoKey();
            modelBuilder.Entity<SpResult>().HasNoKey();
            modelBuilder.Entity<StaffFaceEntity>().HasNoKey();
            modelBuilder.Entity<StaffEnableSettingResult>().HasNoKey();
            // REMOVED: modelBuilder.Entity<License>().HasNoKey(); -> License has [Key] LicenseId

            base.OnModelCreating(modelBuilder);
        }
    }
}
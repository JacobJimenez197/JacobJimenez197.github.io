using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Models
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationMaterial> ReservationMaterials { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasIndex(s => s.Code).IsUnique();
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasIndex(g => g.Code).IsUnique();
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.Property(m => m.Category)
                     .HasConversion<string>();
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Subject)
                      .WithMany(s => s.Reservations)
                      .HasForeignKey(r => r.SubjectId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Group)
                      .WithMany(g => g.Reservations)
                      .HasForeignKey(r => r.GroupId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(r => r.Status)
                      .HasConversion<string>();

                entity.HasCheckConstraint("CK_Reservation_Dates", "\"StartTime\" < \"EndTime\"");
            });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<ReservationMaterial>()
                .HasOne(rm => rm.Reservation)
                .WithMany(r => r.ReservationMaterials)
                .HasForeignKey(rm => rm.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservationMaterial>()
                .HasOne(rm => rm.Material)
                .WithMany()
                .HasForeignKey(rm => rm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Reservation)
                .WithMany(r => r.TeamMembers)
                .HasForeignKey(tm => tm.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany()
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
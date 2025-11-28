using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FootballFieldBooking_New.Models;

namespace FootballFieldBooking_New.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FieldType> FieldTypes { get; set; }
        public DbSet<FootballField> FootballFields { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Review> Reviews { get; set; }
     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FieldType configuration
            modelBuilder.Entity<FieldType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // FootballField configuration
            modelBuilder.Entity<FootballField>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.FieldType)
                    .WithMany(ft => ft.FootballFields)
                    .HasForeignKey(e => e.FieldTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TimeSlots)
                    .WithOne(ts => ts.FootballField)
                    .HasForeignKey(ts => ts.FootballFieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TimeSlot configuration
            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.FootballField)
                    .WithMany(f => f.TimeSlots)
                    .HasForeignKey(e => e.FootballFieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.FootballField)
                    .WithMany(f => f.Bookings)
                    .HasForeignKey(e => e.FootballFieldId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TimeSlot)
                    .WithMany(ts => ts.Bookings)
                    .HasForeignKey(e => e.TimeSlotId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Payment)
                    .WithOne(p => p.Booking)
                    .HasForeignKey<Payment>(p => p.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Booking)
                    .WithOne(b => b.Payment)
                    .HasForeignKey<Payment>(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserProfile configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<UserProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FootballField)
                    .WithMany(f => f.Reviews)
                    .HasForeignKey(e => e.FootballFieldId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Booking)
                    .WithOne()
                    .HasForeignKey<Review>(e => e.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Unique constraint: 1 user chỉ review 1 sân 1 lần
                entity.HasIndex(e => new { e.UserId, e.FootballFieldId })
                    .IsUnique();
            });
        }
        
    }
}
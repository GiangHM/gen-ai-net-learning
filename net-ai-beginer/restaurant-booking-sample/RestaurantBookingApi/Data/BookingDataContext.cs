using Microsoft.EntityFrameworkCore;

namespace RestaurantBookingApi.Data
{
    // TableEntities represents a table in the restaurant
    public class TableEntities
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }

        // Navigation property
        public ICollection<Booking> Bookings { get; set; }
    }

    // Booking represents a reservation
    public class Booking
    {
        public int Id { get; set; }
        public int TableEntitiesId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int NumberOfPeople { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        // Navigation property
        public TableEntities Table { get; set; }
    }

    // Define the BookingDataContext class that inherits from DbContext
    // This class is used to interact with the database for booking-related operations
    public class BookingDataContext : DbContext
    {
        public BookingDataContext(DbContextOptions<BookingDataContext> options) : base(options)
        {
        }

        public DbSet<TableEntities> Tables { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableEntities>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<TableEntities>()
                .HasMany(t => t.Bookings)
                .WithOne(b => b.Table)
                .HasForeignKey(b => b.TableEntitiesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<Booking>()
                .Property(b => b.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Booking>()
                .Property(b => b.CustomerPhone)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Booking>()
                .Property(b => b.ReservationTime)
                .IsRequired();

            modelBuilder.Entity<Booking>()
                .Property(b => b.NumberOfPeople)
                .IsRequired();

            
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed TableEntities
            modelBuilder.Entity<TableEntities>().HasData(
                new TableEntities { Id = 1, Number = 1, Capacity = 2, Description = "Table for couple, beautiful view" },
                new TableEntities { Id = 2, Number = 2, Capacity = 4, Description = "Table for friend, group 3-4 people, at the conner, quite place" },
                new TableEntities { Id = 3, Number = 3, Capacity = 6, Description = "Table for family, vip room" },
                new TableEntities { Id = 4, Number = 4, Capacity = 6, Description = "Table for family, at the center" },
                new TableEntities { Id = 5, Number = 5, Capacity = 6, Description = "Table for family, left side, sunset" }
            );

        }
    }
}

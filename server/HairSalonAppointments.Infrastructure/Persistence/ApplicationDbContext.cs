using Microsoft.EntityFrameworkCore;

namespace HairSalonAppointments.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();
    public DbSet<SuggestionEntity> Suggestions => Set<SuggestionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentEntity>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Service)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(32);

            e.Property(x => x.CreatedAt)
                .IsRequired();

            e.Property(x => x.Status).HasConversion<string>();

            e.Property(x => x.RowVersion).IsRowVersion();

            e.HasIndex(x => new { x.ResourceId, x.Start, x.End });
        });

        modelBuilder.Entity<SuggestionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.State).HasConversion<string>();
            e.Property(x => x.RequestedBy).HasConversion<string>();
        });
    }
}
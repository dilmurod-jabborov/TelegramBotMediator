using Microsoft.EntityFrameworkCore;
using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MessageRelayMap> MessageRelayMaps => Set<MessageRelayMap>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TelegramId).IsUnique();
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Address).IsRequired().HasMaxLength(500);
            entity.Property(x => x.IsBanned).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<MessageRelayMap>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ForwardedMessageId).IsUnique();
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TelegramId).IsUnique();
            entity.Property(x => x.TrackedMessageIdsJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });
    }
}

namespace HoneyWebPlatform.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using Models;

    public class BeekeeperEntityConfiguration : IEntityTypeConfiguration<Beekeeper>
    {
        public void Configure(EntityTypeBuilder<Beekeeper> builder)
        {
            // Configure UserId as Guid for PostgreSQL
            builder
                .Property(b => b.UserId)
                .HasColumnName("UserId")
                .HasColumnType("uuid");

            // Configure other properties if needed
            builder
                .Property(b => b.PhoneNumber)
                .HasMaxLength(15)
                .IsRequired();

            builder
                .Property(b => b.HiveFarmPicturePaths)
                .HasMaxLength(255)
                .IsRequired();
        }
    }
}

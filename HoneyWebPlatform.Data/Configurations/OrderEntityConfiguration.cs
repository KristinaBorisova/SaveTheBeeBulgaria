namespace HoneyWebPlatform.Data.Configurations
{
    using Models;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;


    public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Configure UserId for PostgreSQL compatibility
            builder
                .Property(o => o.UserId)
                .HasColumnName("UserId");

            // Relationships
            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Adjust the delete behavior as needed


            // Define the default value for OrderStatus
            builder.Property(o => o.Status)
                .HasDefaultValue(OrderStatus.Обработван);  
        }
    }
}
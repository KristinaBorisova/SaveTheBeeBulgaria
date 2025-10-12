namespace HoneyWebPlatform.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using Models;

    public class HoneyEntityConfiguration : IEntityTypeConfiguration<Honey>
    {
        public void Configure(EntityTypeBuilder<Honey> builder)
        {
            builder
                .Property(h => h.CreatedOn)
                .HasDefaultValueSql("NOW()");

            builder
                .Property(h => h.IsActive)
                .HasDefaultValue(true);

            builder
                .HasOne(h => h.Category)
                .WithMany(c => c.Honeys)
                .HasForeignKey(h => h.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(h => h.Beekeeper)
                .WithMany(a => a.OwnedHoney)
                .HasForeignKey(h => h.BeekeeperId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GenerateHoneys());
        }

        private Honey[] GenerateHoneys()
        {
            ICollection<Honey> honeys = new HashSet<Honey>();

            // Sample honey products with diverse Bulgarian honey types
            Honey honey1 = new Honey()
            {
                Title = "Premium Linden Honey from Vratsa",
                Origin = "Vratsa, Bulgaria",
                Description = "Exceptional quality linden honey collected from the pristine forests of Vratsa. This honey has a delicate floral aroma and smooth texture, perfect for tea and desserts.",
                ImageUrl = "https://beehoneyportal.com/wp-content/uploads/2014/10/burkan-s-med-3.jpg",
                Price = 15.50M,
                NetWeight = 500,
                YearMade = 2024,
                CategoryId = 1, // Linden
                BeekeeperId = Guid.Parse("7ADAF90E-FEC8-492E-8760-FE3190F1D689"),
            };
            honeys.Add(honey1);

            Honey honey2 = new Honey()
            {
                Title = "Organic Sunflower Honey",
                Origin = "Dobrich, Bulgaria",
                Description = "Pure organic sunflower honey from the sunflower fields of Dobrich. Rich in vitamins and minerals, this golden honey has a distinctive sunflower taste.",
                ImageUrl = "https://example.com/sunflower-honey.jpg",
                Price = 12.00M,
                NetWeight = 450,
                YearMade = 2024,
                CategoryId = 3, // Sunflower
                BeekeeperId = Guid.Parse("7ADAF90E-FEC8-492E-8760-FE3190F1D689"),
            };
            honeys.Add(honey2);

            Honey honey3 = new Honey()
            {
                Title = "Bio Acacia Honey",
                Origin = "Plovdiv, Bulgaria",
                Description = "Certified bio acacia honey from the acacia forests near Plovdiv. This light, crystal-clear honey is perfect for those who prefer mild sweetness.",
                ImageUrl = "https://example.com/acacia-honey.jpg",
                Price = 18.00M,
                NetWeight = 500,
                YearMade = 2024,
                CategoryId = 2, // Bio
                BeekeeperId = Guid.Parse("7ADAF90E-FEC8-492E-8760-FE3190F1D689"),
            };
            honeys.Add(honey3);

            Honey honey4 = new Honey()
            {
                Title = "Wildflower Bouquet Honey",
                Origin = "Rila Mountains, Bulgaria",
                Description = "Aromatic wildflower honey collected from the diverse flora of Rila Mountains. This multi-floral honey offers a complex taste profile with notes of various mountain flowers.",
                ImageUrl = "https://example.com/wildflower-honey.jpg",
                Price = 16.50M,
                NetWeight = 400,
                YearMade = 2024,
                CategoryId = 4, // Bouquet
                BeekeeperId = Guid.Parse("7ADAF90E-FEC8-492E-8760-FE3190F1D689"),
            };
            honeys.Add(honey4);

            Honey honey5 = new Honey()
            {
                Title = "Forest Honeydew Honey",
                Origin = "Stara Planina, Bulgaria",
                Description = "Dark, rich honeydew honey from the ancient forests of Stara Planina. This honey has a robust flavor and is rich in minerals and antioxidants.",
                ImageUrl = "https://example.com/honeydew-honey.jpg",
                Price = 20.00M,
                NetWeight = 350,
                YearMade = 2024,
                CategoryId = 5, // Honeydew
                BeekeeperId = Guid.Parse("7ADAF90E-FEC8-492E-8760-FE3190F1D689"),
            };
            honeys.Add(honey5);

            return honeys.ToArray();
        }
    }
}

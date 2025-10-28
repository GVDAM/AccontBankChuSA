using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Mappings
{
    public class CustomerMapping : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customer")
                .HasKey(x => x.Id)
                .HasName("pk_customer");

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Password)
                .HasColumnName("password")
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Role)
                .HasColumnName("role")
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("idx_email_customer");

        }
    }
}

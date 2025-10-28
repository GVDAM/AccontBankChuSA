using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Mappings
{
    public class AccountMapping : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("account")
                .HasKey(x => x.Id)
                .HasName("pk_account");

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Agency)
                .HasColumnName("agency")
                .IsRequired();

            builder.Property(x => x.Number)
                .HasColumnName("number")
                .IsRequired();

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired();

            builder.Property(x => x.Balance)
                .HasColumnName("balance")
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Account)
                .HasForeignKey(x => x.CustomerId)
                .HasConstraintName("fk_customer");


            builder.HasIndex(x => new { x.Agency, x.Number }).IsUnique().HasDatabaseName("idx_agency_number_account");
        }
    }
}

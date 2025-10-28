using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Mappings
{
    public class TransactionMapping : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transaction")
                .HasKey(x => x.Id)
                .HasName("pk_transaction");

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("ammout")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.SenderAccountId)
                .HasColumnName("sender_account_id")
                .IsRequired();

            builder.Property(x => x.ReceiverAccountId)
                .HasColumnName("receiver_account_id")
                .IsRequired();

            
            builder.HasOne(x => x.SenderAccount)
                .WithMany(x => x.TransactionsSender)
                .HasForeignKey(x => x.SenderAccountId)
                .HasConstraintName("fk_sender_account");

            builder.HasOne(x => x.ReceiverAccount)
                .WithMany(x => x.TransactionsReceiver)
                .HasForeignKey(x => x.ReceiverAccountId)
                .HasConstraintName("fk_receiver_account");

        }
    }
}

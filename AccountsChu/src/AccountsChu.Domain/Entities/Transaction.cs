namespace AccountsChu.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public int ReceiverAccountId { get; set; }
        public Account ReceiverAccount { get; set; }

        public int SenderAccountId { get; set; }
        public Account SenderAccount { get; set; }


        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

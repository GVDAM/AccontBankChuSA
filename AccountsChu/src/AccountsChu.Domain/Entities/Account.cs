namespace AccountsChu.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public Int16 Agency { get; set; }
        public Int32 Number { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Transaction> TransactionsSender { get; set; }
        public ICollection<Transaction> TransactionsReceiver { get; set; }


        public Account(Int16 agency, Int32 number, decimal balance, int customerId)
        {
            CustomerId = customerId;

            Agency = agency;
            Number = number;
            Balance = balance;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Account()
        {
            
        }

    }
}

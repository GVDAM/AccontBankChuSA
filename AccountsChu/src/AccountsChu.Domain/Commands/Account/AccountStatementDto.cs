namespace AccountsChu.Domain.Commands.Account
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public short Agency { get; set; }
        public int Number { get; set; }
        public decimal Balance { get; set; }

        public List<AccountStatementDto> Transactions { get; set; } = new List<AccountStatementDto>();
    }

    public class AccountStatementDto
    {
        public string BalanceInOut { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}

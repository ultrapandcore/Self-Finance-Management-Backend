namespace Finance.App.Domain.Models
{
    public class FinancialOperation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsIncome { get; set; }
        public bool Deleted { get; set; } = false;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

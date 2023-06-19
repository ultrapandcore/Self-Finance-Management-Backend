namespace Finance.App.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<FinancialOperation> Operations { get; set; }
        public bool Deleted { get; set; } = false;
    }
}

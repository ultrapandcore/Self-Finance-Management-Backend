using Finance.App.Domain.Models;

namespace Finance.App.Dtos
{
    public class OperationsByDateDto
    {
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public IEnumerable<FinancialOperationDto> Operations { get; set; }
    }
}
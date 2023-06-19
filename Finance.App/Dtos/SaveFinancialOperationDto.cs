using System.ComponentModel.DataAnnotations;

namespace Finance.App.Dtos
{
    public class SaveFinancialOperationDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsIncome { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}

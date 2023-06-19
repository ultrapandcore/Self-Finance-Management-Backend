using System.ComponentModel.DataAnnotations;

namespace Finance.App.Dtos
{
    public class SaveCategoryDto
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
    }
}

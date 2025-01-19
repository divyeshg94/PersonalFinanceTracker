using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Model
{
    public class FireInputModel
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid monthly expense.")]
        public decimal MonthlyExpense { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid current age.")]
        public int CurrentAge { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid retirement age.")]
        public int RetirementAge { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Please enter a valid inflation rate (0-100).")]
        public decimal InflationRate { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid desired Coast FIRE age.")]
        public int CoastFireAge { get; set; }
    }
}

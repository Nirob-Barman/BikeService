namespace BikeService.Application.DTOs.Payroll
{
    public class PayrollRecordFormDto
    {
        public int MechanicId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public string? Notes { get; set; }
    }
}

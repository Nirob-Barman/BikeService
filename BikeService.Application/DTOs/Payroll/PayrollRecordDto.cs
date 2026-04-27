using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Payroll
{
    public class PayrollRecordDto
    {
        public int Id { get; set; }
        public int MechanicId { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string PeriodLabel => $"{new DateTime(Year, Month, 1):MMMM yyyy}";
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay => BaseSalary + Bonus - Deductions;
        public PayrollStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities
{
    public class PayrollRecord : BaseEntity
    {
        public int MechanicId { get; set; }
        public Mechanic Mechanic { get; set; } = null!;

        public int Month { get; set; }   // 1–12
        public int Year { get; set; }

        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }

        public decimal NetPay => BaseSalary + Bonus - Deductions;

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
        public string? Notes { get; set; }
    }
}

namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report
{
    public class SalesSummaryRowDto
    {
        public string Dealer { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int TotalVehicles { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDebt { get; set; }
    }
}

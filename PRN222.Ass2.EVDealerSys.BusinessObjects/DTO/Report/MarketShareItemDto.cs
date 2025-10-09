namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report
{
    public class MarketShareItemDto
    {
        public string Dealer { get; set; } = "";
        public string Model { get; set; } = "";
        public string Color { get; set; } = "";
        public string Version { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}

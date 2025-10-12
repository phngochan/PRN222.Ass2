namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Customer;
public class CustomerItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string DisplayText => $"{Name} - {Phone}";
}

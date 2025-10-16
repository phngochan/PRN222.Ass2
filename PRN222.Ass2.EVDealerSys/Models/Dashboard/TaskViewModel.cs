namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

public class TaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}


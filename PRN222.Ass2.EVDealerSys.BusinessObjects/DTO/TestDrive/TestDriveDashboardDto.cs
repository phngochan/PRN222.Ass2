namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
public class TestDriveDashboardDto
{
    public int TodayTestDrivesCount { get; set; }
    public int PendingTestDrives { get; set; }
    public int CompletedTestDrives { get; set; }
    public int TotalTestDrives { get; set; }
    public List<TestDriveItemDto> UpcomingTestDrives { get; set; } = new List<TestDriveItemDto>();
    public List<TestDriveItemDto> TodayTestDrives { get; set; } = new List<TestDriveItemDto>();
}

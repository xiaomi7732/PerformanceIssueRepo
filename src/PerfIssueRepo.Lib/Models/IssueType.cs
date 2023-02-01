namespace PerfIssueRepo.Models;

public static class IssueType
{
    public const string Unknown = "U";
    public const string OnCPU = "C";
    public const string Memory = "M";
    public const string DiskIO = "D";
    public const string NetworkIO = "N";
    public const string Thread = "T";
    public const string OffCPU = "O";
}
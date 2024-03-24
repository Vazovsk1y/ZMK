namespace ZMK.Application.Contracts;

public record ExportToExcelProjectExecutionDTO(Guid ProjectId, ProjectExecutionReportTypes ReportType, string FilePath, Range? Range = null);

public record Range(DateTime From, DateTime To);

public enum ProjectExecutionReportTypes
{
    ByAreas,
    ByExecutors
}
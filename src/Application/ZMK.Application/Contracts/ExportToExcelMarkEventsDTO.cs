namespace ZMK.Application.Contracts;

public record ExportToExcelMarkEventsDTO(Guid MarkId, MarkEventsReportTypes ReportType, string FilePath);

public enum MarkEventsReportTypes
{
    Common,
    Modify,
    Complete
}
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Common;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Extensions;

public static class Mapper
{
    public static MarkCompleteEvent ToCompleteEvent(this Mark mark, AreaExecutionDTO dTO, DateTimeOffset createdDate, Guid creatorId)
    {
        var entity = new MarkCompleteEvent
        {
            MarkId = mark.Id,
            AreaId = dTO.AreaId,
            CompleteCount = dTO.Count,
            CreatedDate = createdDate,
            CreatorId = creatorId,
            EventType = EventType.Complete,
            Remark = dTO.Remark?.Trim(),
            MarkCode = mark.Code,
            MarkCount = mark.Count,
            MarkOrder = mark.Order,
            MarkTitle = mark.Title,
            MarkWeight = mark.Weight,
            CompleteDate = dTO.Date,
        };

        return entity;
    }
    
    public static MarkEventsReport<T> ToReport<T>(this Mark mark, IEnumerable<T> events)
    {
        return new MarkEventsReport<T>(
                        DateTime.Now,
                        mark.Title,
                        mark.Count,
                        mark.Code,
                        mark.Weight,
                        events);
    }

    public static ProjectExecutionReport<T> ToReport<T>(this Project project, ExportToExcelProjectExecutionDTO dTO, IEnumerable<T> items)
    {
        string reportTypeString = dTO.ReportType switch
        {
            ProjectExecutionReportTypes.ByAreas => "По участкам",
            ProjectExecutionReportTypes.ByExecutors => "По исполнителям",
            _ => throw new KeyNotFoundException(),
        };

        return new ProjectExecutionReport<T>(
            DateTime.Now, 
            project.FactoryNumber, 
            project.CreatedDate, 
            $"{project.Creator.UserName} - {project.Creator.Employee.FullName}",
            reportTypeString,
            dTO.Range is null ? "За весь период" : $"C: {dTO.Range.From:dd.MM.yyyy} По: {dTO.Range.To:dd.MM.yyyy}",
            items
            );
    }

    public static CompleteMarkEventRaw ToCompleteRaw(this MarkCompleteEvent @event)
    {
        return new CompleteMarkEventRaw(
            @event.CompleteDate.Date,
            @event.CompleteCount,
            @event.Area.Title,
            "Выполнено",
            string.Join(", ", @event.Executors.Select(e => e.Employee.FullName).ToList()),
            $"{@event.Creator.UserName}-{@event.Creator.Employee.FullName}",
            @event.Remark
            );
    }

    public static ModifyOrCreateMarkEventRaw ToModifyOrCreateRaw(this MarkEvent markEvent)
    {
        string eventType = markEvent.EventType switch { EventType.Create => "Создано", EventType.Modify => "Изменено", _ => throw new KeyNotFoundException() };
        return new ModifyOrCreateMarkEventRaw(
            markEvent.CreatedDate.DateTime,
            markEvent.MarkCount,
            markEvent.MarkCode,
            markEvent.MarkWeight,
            markEvent.MarkTitle,
            eventType,
            $"{markEvent.Creator.UserName}-{markEvent.Creator.Employee.FullName}",
            markEvent.Remark);
    }

    public static CommonMarkEventRaw ToCommonRaw(this MarkEvent markEvent)
    {
        string creator = $"{markEvent.Creator.UserName}-{markEvent.Creator.Employee.FullName}";
        switch (markEvent.EventType)
        {
            case EventType.Create:
                {
                    return new CommonMarkEventRaw(
                        markEvent.CreatedDate.DateTime,
                        markEvent.MarkCount,
                        $"{markEvent.MarkCode}-{markEvent.MarkTitle}",
                        "Создано",
                        creator,
                        markEvent.Remark);
                }
            case EventType.Modify:
                {
                    return new CommonMarkEventRaw(
                        markEvent.CreatedDate.DateTime,
                        markEvent.MarkCount,
                        $"{markEvent.MarkCode}-{markEvent.MarkTitle}",
                        "Изменено",
                        creator,
                        markEvent.Remark);
                }
            case EventType.Complete:
                {
                    var completeEvent = (MarkCompleteEvent)markEvent;
                    return new CommonMarkEventRaw(
                        completeEvent.CompleteDate.DateTime,
                        completeEvent.CompleteCount,
                        completeEvent.Area.Title,
                        "Выполнено",
                        creator,
                        markEvent.Remark);
                }
            default:
                throw new KeyNotFoundException();
        }
    }

    public static Project ToProject(this ProjectAddDTO dto, IClock clock, Guid creatorId)
    {
        return new Project
        {
            FactoryNumber = dto.FactoryNumber.Trim(),
            ContractNumber = dto.ContractNumber?.Trim(),
            Customer = dto.Customer?.Trim(),
            Vendor = dto.Vendor?.Trim(),
            Remark = dto.Remark?.Trim(),
            CreatedDate = clock.GetDateTimeOffsetUtcNow(),
            CreatorId = creatorId,
        };
    }

    public static void Update(this Project project, ProjectUpdateDTO dto, IClock clock)
    {
        project.FactoryNumber = dto.FactoryNumber.Trim();
        project.ContractNumber = dto.ContractNumber?.Trim();
        project.Customer = dto.Customer?.Trim();
        project.Vendor = dto.Vendor?.Trim();
        project.Remark = dto.Remark?.Trim();
        project.ModifiedDate = clock.GetDateTimeOffsetUtcNow();
    }

    public static ProjectSettings ToProjectSettings(this ProjectAddDTO dto, Guid projectId)
    {
        return new ProjectSettings
        {
            ProjectId = projectId,
            IsEditable = dto.IsEditable,
            AllowMarksDeleting = dto.AllowMarksDeleting,
            AllowMarksAdding = dto.AllowMarksAdding,
            AllowMarksModifying = dto.AllowMarksModifying,
            AreExecutorsRequired = dto.AreExecutorsRequired
        };
    }

    public static Mark ToMark(this MarkAddDTO markAddDTO)
    {
        return new Mark
        {
            ProjectId = markAddDTO.ProjectId,
            Code = markAddDTO.Code.Trim(),
            Title = markAddDTO.Title.Trim(),
            Order = markAddDTO.Order,
            Weight = markAddDTO.Weight,
            Count = markAddDTO.Count,
        };
    }

    public static void Update(this Mark mark, MarkUpdateDTO markUpdateDTO)
    {
        mark.Code = markUpdateDTO.Code.Trim();
        mark.Title = markUpdateDTO.Title.Trim();
        mark.Order = markUpdateDTO.Order;
        mark.Weight = markUpdateDTO.Weight;
        mark.Count = markUpdateDTO.Count;
    }
}
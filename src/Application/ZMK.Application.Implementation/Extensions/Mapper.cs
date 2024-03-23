using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Common;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Extensions;

public static class Mapper
{
    public static CompleteReportMarkEvent ToComplete(this MarkCompleteEvent @event)
    {
        return new CompleteReportMarkEvent(
            @event.CompleteDate.Date,
            @event.CompleteCount,
            @event.Area.Title,
            "Выполнено",
            string.Join(", ", @event.Executors.Select(e => e.Employee.FullName).ToList()),
            $"{@event.Creator.UserName}-{@event.Creator.Employee.FullName}",
            @event.Remark
            );
    }

    public static ModifyOrCreateReportMarkEvent ToModifyOrCreate(this MarkEvent markEvent)
    {
        string eventType = markEvent.EventType switch { EventType.Create => "Создано", EventType.Modify => "Изменено", _ => throw new KeyNotFoundException() };
        return new ModifyOrCreateReportMarkEvent(
            markEvent.CreatedDate.DateTime,
            markEvent.MarkCount,
            markEvent.MarkCode,
            markEvent.MarkWeight,
            markEvent.MarkTitle,
            eventType,
            $"{markEvent.Creator.UserName}-{markEvent.Creator.Employee.FullName}",
            markEvent.Remark);
    }

    public static CommonReportMarkEvent ToCommon(this MarkEvent markEvent)
    {
        string creator = $"{markEvent.Creator.UserName}-{markEvent.Creator.Employee.FullName}";
        switch (markEvent.EventType)
        {
            case EventType.Create:
                {
                    return new CommonReportMarkEvent(
                        markEvent.CreatedDate.DateTime,
                        markEvent.MarkCount,
                        $"{markEvent.MarkCode}-{markEvent.MarkTitle}",
                        "Создано",
                        creator,
                        markEvent.Remark);
                }
            case EventType.Modify:
                {
                    return new CommonReportMarkEvent(
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
                    return new CommonReportMarkEvent(
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

    public static Project ToEntity(this ProjectAddDTO dto, IClock clock, Guid creatorId)
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

    public static ProjectSettings ToSettingsEntity(this ProjectAddDTO dto, Guid projectId)
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

    public static Mark ToEntity(this MarkAddDTO markAddDTO)
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
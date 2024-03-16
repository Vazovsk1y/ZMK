using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Extensions;

public static class Mapper
{
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
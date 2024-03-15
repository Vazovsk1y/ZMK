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
        project.Settings.IsEditable = dto.IsEditable;
        project.Settings.AllowMarksDeleting = dto.AllowMarksDeleting;
        project.Settings.AllowMarksAdding = dto.AllowMarksAdding;
        project.Settings.AllowMarksModifying = dto.AllowMarksModifying;
        project.Settings.AreExecutorsRequired = dto.AreExecutorsRequired;
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
}
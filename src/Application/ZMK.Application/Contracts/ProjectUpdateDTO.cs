namespace ZMK.Application.Contracts;

public record ProjectUpdateDTO(
    Guid Id,
    string FactoryNumber,
    string? ContractNumber,
    string? Customer,
    string? Vendor,
    string? Remark,
    bool IsEditable,
    bool AllowMarksDeleting,
    bool AllowMarksAdding,
    bool AllowMarksModifying,
    bool AreExecutorsRequired);


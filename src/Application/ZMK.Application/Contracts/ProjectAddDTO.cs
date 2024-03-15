namespace ZMK.Application.Contracts;

public record ProjectAddDTO(
    string FactoryNumber, 
    string? ContractNumber, 
    string? Customer, 
    string? Vendor, 
    string? Remark,
    bool IsEditable,
    bool AllowMarksDeleting,
    bool AllowMarksAdding,
    bool AllowMarksModifying,
    bool AreExecutorsRequired,
    IEnumerable<Guid> Areas);


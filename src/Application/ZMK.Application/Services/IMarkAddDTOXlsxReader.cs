using ZMK.Application.Contracts;

namespace ZMK.Application.Services;

public interface IMarkAddDTOXlsxReader
{
    IEnumerable<MarkAddDTO> Read(string filePath, Guid ProjectId);
}
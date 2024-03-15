using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IMarkService
{
    Task<Result<Guid>> AddMarkAsync(MarkAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<Guid>>> AddFromXlsxAsync(string filePath, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(MarkUpdateDTO dTO, CancellationToken cancellationToken = default);
}
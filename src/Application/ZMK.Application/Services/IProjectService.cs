using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IProjectService
{
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> AddAsync(ProjectAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(ProjectUpdateDTO dTO, CancellationToken cancellationToken = default);
}
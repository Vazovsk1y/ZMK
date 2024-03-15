using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IEmployeeService
{
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> AddAsync(EmployeeAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(EmployeeUpdateDTO dTO, CancellationToken cancellationToken = default);
}
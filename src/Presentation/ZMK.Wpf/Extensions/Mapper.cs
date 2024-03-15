using ZMK.Domain.Entities;
using ZMK.Wpf.ViewModels;

namespace ZMK.Wpf.Extensions;

public static class Mapper
{
    public static CurrentSessionViewModel ToViewModel(this Session session)
    {
        return new CurrentSessionViewModel
        {
            Id = session.Id,
            User = session.User!.ToViewModel()
        };
    }

    public static UserViewModel ToPanelViewModel(this UserAddViewModel userAddViewModel, Guid id)
    {
        var entity = new UserViewModel
        {
            Id = id,
            Employee = userAddViewModel.SelectedEmployee,
            Role = userAddViewModel.SelectedRole,
            UserName = userAddViewModel.UserName,
        };

        entity.SaveState();
        return entity;
    }

    public static EmployeeViewModel ToPanelViewModel(this EmployeeAddViewModel employeeAddVm, Guid id)
    {
        var entity = new EmployeeViewModel
        {
            Id = id,
            FullName = employeeAddVm.FullName,
            Post = employeeAddVm.Post,
            Remark = employeeAddVm.Remark,
        };

        entity.SaveState();
        return entity;
    }

    public static EmployeeViewModel ToPanelViewModel(this Employee employee)
    {
        var entity = new EmployeeViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Post = employee.Post,
            Remark = employee.Remark,
        };

        entity.SaveState();
        return entity;
    }

    public static CurrentUserViewModel ToViewModel(this User user)
    {
        var role = user.Roles.SingleOrDefault() ?? throw new InvalidOperationException($"У {user.UserName} не определена роль.");

        return new CurrentUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Role = role.Role!.ToPanelViewModel(),
            Employee = user.Employee!.ToUsersPanelViewModel(),
        };
    }

    public static UserViewModel ToPanelViewModel(this User user)
    {
        var role = user.Roles.Select(e => e.Role).SingleOrDefault() ?? throw new InvalidOperationException($"У {user.UserName} не определена роль.");

        var entity = new UserViewModel
        {
            Id = user.Id,
            Role = role.ToPanelViewModel(),
            Employee = user.Employee!.ToUsersPanelViewModel(),
            UserName = user.UserName!,
        };

        entity.SaveState();
        return entity;
    }

    public static UserViewModel.RoleViewModel ToPanelViewModel(this Role role)
    {
        return new UserViewModel.RoleViewModel(role.Id, role.Name!);
    }

    public static UserViewModel.EmployeeViewModel ToUsersPanelViewModel(this Employee employee)
    {
        return new UserViewModel.EmployeeViewModel(employee.Id, employee.FullName);
    }
}

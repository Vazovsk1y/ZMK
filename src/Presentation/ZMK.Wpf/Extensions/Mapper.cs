using ZMK.Application.Contracts;
using ZMK.Domain.Entities;
using ZMK.Wpf.ViewModels;
using static ZMK.Wpf.ViewModels.ProjectViewModel;

namespace ZMK.Wpf.Extensions;

public static class Mapper
{
    public static ProjectViewModel ToViewModel(this Project project)
    {
        var entity = new ProjectViewModel
        {
            Id = project.Id,
            Creator = project.Creator is not null ? new CreatorViewModel(project.Creator.Id, project.Creator!.UserName!) : null,
            ClosingDate = project.ClosingDate,
            CreatedDate = project.CreatedDate,
            ModifiedDate = project.ModifiedDate,
            FactoryNumber = project.FactoryNumber,
            ContractNumber = project.ContractNumber,
            Customer = project.Customer,
            Vendor = project.Vendor,
            Remark = project.Remark,
            Settings = project.Settings.ToViewModel(project),
        };
        entity.Settings.SaveState();
        entity.SaveState();
        return entity;
    }

    public static ProjectUpdateDTO ToDTO(this ProjectViewModel viewModel)
    {
        return new ProjectUpdateDTO(
            viewModel.Id,
            viewModel.FactoryNumber,
            viewModel.ContractNumber,
            viewModel.Customer,
            viewModel.Vendor,
            viewModel.Remark,
            viewModel.Settings.IsEditable,
            viewModel.Settings.AllowMarksDeleting,
            viewModel.Settings.AllowMarksAdding,
            viewModel.Settings.AllowMarksModifying,
            viewModel.Settings.AreExecutorsRequired,
            viewModel.Settings.Areas.Select(area => area.Id)
        );
    }

    private static ProjectSettingsViewModel ToViewModel(this ProjectSettings settings, Project project)
    {
        var entity = new ProjectSettingsViewModel
        {
            ProjectId = settings.ProjectId,
            IsEditable = settings.IsEditable,
            AllowMarksDeleting = settings.AllowMarksDeleting,
            AllowMarksModifying = settings.AllowMarksModifying,
            AllowMarksAdding = settings.AllowMarksAdding,
            AreExecutorsRequired = settings.AreExecutorsRequired,
            Areas = new (project.Areas.Select(e => new ProjectSettingsViewModel.AreaViewModel(e.AreaId, e.Area.Title, e.Area.Order))),
        };

        entity.SaveState();
        return entity;
    }

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

using System.Globalization;
using ZMK.Application.Contracts;
using ZMK.Domain.Common;
using ZMK.Domain.Entities;
using ZMK.Wpf.ViewModels;
using ZMK.Wpf.ViewModels.User;
using static ZMK.Wpf.ViewModels.ProjectViewModel;

namespace ZMK.Wpf.Extensions;

public static class Mapper
{
    public static ExecutorInfo ToInfo(this Employee employee)
    {
        return new ExecutorInfo(employee.Id, string.IsNullOrWhiteSpace(employee.Post) ?
                employee.FullName
                :
                $"{employee.FullName} ({employee.Post})");
    }
    public static MarkEventsReportTypes ToReportType(this string type)
    {
        return type switch
        {
            MarkEventViewModel.CommonEventType => MarkEventsReportTypes.Common,
            MarkEventViewModel.ModifyEventType => MarkEventsReportTypes.Modify,
            MarkEventViewModel.CompleteEventType => MarkEventsReportTypes.Complete,
            _ => throw new KeyNotFoundException(),
        };
    }
    public static MarkEventViewModel ToViewModel(this MarkEvent @event)
    {
        string creatorInfo = $"{@event.Creator.UserName} - {@event.Creator.Employee.FullName}";
        string commonTitle = $"{@event.MarkCode} - {@event.MarkTitle}";

        switch (@event.EventType)
        {
            case EventType.Create:
                return new MarkCreateOrModifyEventViewModel 
                {
                    Id = @event.Id,
                    CreatorUserNameAndEmployeeFullName = creatorInfo,
                    MarkCount = @event.MarkCount,
                    Date = @event.CreatedDate.DateTime,
                    EventType = MarkEventViewModel.CreateEventType,
                    MarkTitle = @event.MarkTitle,
                    MarkCode = @event.MarkCode,
                    Remark = @event.Remark,
                    MarkOrder = @event.MarkOrder,
                    CommonTitle = commonTitle,
                    MarkWeight = @event.MarkWeight,
                };
            case EventType.Modify:
                return new MarkCreateOrModifyEventViewModel
                {
                    Id = @event.Id,
                    CreatorUserNameAndEmployeeFullName = creatorInfo,
                    MarkCount = @event.MarkCount,
                    Date = @event.CreatedDate.DateTime,
                    EventType = MarkEventViewModel.ModifyEventType,
                    MarkTitle = @event.MarkTitle,
                    MarkCode = @event.MarkCode,
                    Remark = @event.Remark,
                    MarkOrder = @event.MarkOrder,
                    CommonTitle = commonTitle,
                    MarkWeight = @event.MarkWeight,
                };
            case EventType.Complete:
                {
                    var completeEvent = (MarkCompleteEvent)@event;
                    return completeEvent.ToViewModel();
                }
            default: throw new KeyNotFoundException();
        }
    }
    public static MarkCompleteEventViewModel ToViewModel(this MarkCompleteEvent @event)
    {
        var entity = new MarkCompleteEventViewModel()
        {
            Id = @event.Id,
            Date = @event.CompleteDate.Date,
            Area = new(@event.Area.Id, @event.Area.Title),
            MarkCount = @event.CompleteCount,
            CreatorUserNameAndEmployeeFullName = $"{@event.Creator.UserName} - {@event.Creator.Employee.FullName}",
            Remark = @event.Remark,
            CommonTitle = @event.Area.Title,
            EventType = MarkEventViewModel.CompleteEventType,
        };

        entity.Executors.AddRange(@event.Executors.Select(e => new ExecutorInfo(e.EmployeeId,string.IsNullOrWhiteSpace(e.Employee.Post) ? e.Employee.FullName : $"{e.Employee.FullName} ({e.Employee.Post})")));
        entity.SaveState();
        return entity;
    }
    public static double? ParseInDifferentCultures(this string number)
    {
        if (!double.TryParse(number, NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
            //Then try in US english
            !double.TryParse(number, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
            //Then in neutral language
            !double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return null;
        }

        return result;
    }
    public static AreaViewModel ToViewModel(this Area area)
    {
        var viewModel = new AreaViewModel
        {
            Id = area.Id,
            Title = area.Title,
            Order = area.Order,
            Remark = area.Remark
        };

        viewModel.SaveState();

        return viewModel;
    }
    public static MarkViewModel ToViewModel(this Mark mark)
    {
        var entity = new MarkViewModel
        {
            Id = mark.Id,
            Code = mark.Code,
            Title = mark.Title,
            Order = mark.Order,
            Weight = mark.Weight,
            Count = mark.Count,
            ProjectId = mark.ProjectId,
        };

        entity.SaveState();
        return entity;
    }

    public static ProjectViewModel ToViewModel(this Project project)
    {
        var entity = new ProjectViewModel
        {
            Id = project.Id,
            Creator = project.Creator is not null ? new CreatorInfo(project.Creator.Id, project.Creator!.UserName!) : null,
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
            viewModel.Remark
        );
    }

    public static ProjectSettingsUpdateDTO ToUpdateDTO(this ProjectSettingsViewModel viewModel)
    {
        return new ProjectSettingsUpdateDTO(
            viewModel.ProjectId,
            viewModel.IsEditable,
            viewModel.AllowMarksDeleting,
            viewModel.AllowMarksAdding,
            viewModel.AllowMarksModifying,
            viewModel.AreExecutorsRequired,
            viewModel.Areas.Select(area => area.Id)
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
            Areas = new (project.Areas.Select(e => new ProjectSettingsViewModel.ProjectAreaViewModel(e.AreaId, e.Area.Title, e.Area.Order)).ToList()),
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

    public static UserViewModel.RoleInfo ToPanelViewModel(this Role role)
    {
        return new UserViewModel.RoleInfo(role.Id, role.Name!);
    }

    public static UserViewModel.EmployeeInfo ToUsersPanelViewModel(this Employee employee)
    {
        return new UserViewModel.EmployeeInfo(employee.Id, employee.FullName);
    }
}

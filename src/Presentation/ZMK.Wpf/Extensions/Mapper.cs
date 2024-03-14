using System.Collections.ObjectModel;
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

    public static CurrentUserViewModel ToViewModel(this User user)
    {
        return new CurrentUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Roles = new ObservableCollection<string>(user.Roles.Select(r => r.Role!.Name)!),
            Employee = user.Employee!.ToViewModel()
        };
    }

    public static CurrentEmployeeViewModel ToViewModel(this Employee employee)
    {

        return new CurrentEmployeeViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName
        };
    }
}

using ZMK.Domain.Shared;

namespace ZMK.Wpf.Extensions;

public static class CollectionsEx
{
    public static void AddRange<T>(this IList<T> values, IEnumerable<T> valuesToAdd)
    {
        foreach (var item in valuesToAdd)
        {
            values.Add(item);
        }
    }

    public static string Display(this IEnumerable<Error> errors)
    {
        return string.Join(Environment.NewLine, errors.Select(e => e.Text));
    }

    public static void DisplayUpdateResultMessageBox(this IEnumerable<Result> results)
    {
        var failedResults = results.Where(e => e.IsFailure);
        var successCount = results.Count() - failedResults.Count();

        string message = failedResults.Count() == 0 ? $"Успешно обновлено {successCount} записей." : $"Успешно обновлено {successCount} записей.\nОшибки:\n{failedResults.SelectMany(e => e.Errors).Display()}";
        MessageBoxHelper.ShowInfoBox(message);
    }
}
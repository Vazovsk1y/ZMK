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
}
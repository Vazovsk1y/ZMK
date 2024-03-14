using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ZMK.Wpf.Extensions;

public static class TaskEx
{
    public static void NoAwait(this Task? task)
    {
        task?.ContinueWith(NoAwaitContinuation,
          TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion);
    }

    private static readonly Action<Task> NoAwaitContinuation = t =>
    {
        if (t.Exception is not null)
        {
            using var scope = App.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            logger.LogError(t.Exception, "Что-то пошло не так.");
        }
    };
}
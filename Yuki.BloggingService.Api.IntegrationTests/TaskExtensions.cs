namespace Yuki.BloggingService.Api.IntegrationTests;

internal static class TaskExtensions
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMilliseconds = 5000)
    {
        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        var delayTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
        var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
        if (completed != task) throw new TimeoutException("Timed out waiting for event to be published.");
        await cts.CancelAsync();
        return await task.ConfigureAwait(false);

    }
}
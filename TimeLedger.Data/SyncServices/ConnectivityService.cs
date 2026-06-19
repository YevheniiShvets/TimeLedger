using Microsoft.Data.SqlClient;
using TimeLedger.Core.Interfaces.Synchronisation;

namespace TimeLedger.Infrastructure.SyncServices;

public class ConnectivityService(string connectionString) : IConnectivityService
{
    private bool _lastResult;
    private DateTime _lastCheck = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);

    public bool IsAvailable()
    {
        if (DateTime.UtcNow - _lastCheck < CacheDuration)
            return _lastResult;

        try
        {
            var task = Task.Run(async () =>
            {
                await using var connection = new SqlConnection(connectionString);
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
                await connection.OpenAsync(cts.Token);
                return true;
            });

            _lastResult = task.Wait(TimeSpan.FromSeconds(1)) && task.Result;
        }
        catch
        {
            _lastResult = false;
        }

        _lastCheck = DateTime.UtcNow;
        return _lastResult;
    }
}
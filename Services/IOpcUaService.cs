namespace OpcUaWorkerService.Services;

public interface IOpcUaService
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<T?> ReadVariableAsync<T>(string nodeId, CancellationToken cancellationToken = default);
    Task WriteVariableAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default);
}
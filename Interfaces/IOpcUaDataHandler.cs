namespace OpcUaWorkerService.Interfaces;

public interface IOpcUaDataHandler
{
    Task HandleReadDataAsync(string variableName, string nodeId, object? value, CancellationToken cancellationToken);
    Task HandleWriteDataAsync(string variableName, string nodeId, object value, CancellationToken cancellationToken);
    Task OnConnectedAsync(CancellationToken cancellationToken);
    Task OnDisconnectedAsync(CancellationToken cancellationToken);
    Task OnErrorAsync(Exception exception, CancellationToken cancellationToken);
}

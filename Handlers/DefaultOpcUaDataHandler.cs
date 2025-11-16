using Microsoft.Extensions.Logging;
using OpcUaWorkerService.Interfaces;

namespace OpcUaWorkerService.Handlers;

public class DefaultOpcUaDataHandler : IOpcUaDataHandler
{
    private readonly ILogger<DefaultOpcUaDataHandler> _logger;

    public DefaultOpcUaDataHandler(ILogger<DefaultOpcUaDataHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleReadDataAsync(string variableName, string nodeId, object? value, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Read {VariableName} ({NodeId}): {Value}", variableName, nodeId, value);
        return Task.CompletedTask;
    }

    public Task HandleWriteDataAsync(string variableName, string nodeId, object value, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Wrote {VariableName} ({NodeId}): {Value}", variableName, nodeId, value);
        return Task.CompletedTask;
    }

    public Task OnConnectedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OPC UA connection established");
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OPC UA connection closed");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "OPC UA error occurred");
        return Task.CompletedTask;
    }
}

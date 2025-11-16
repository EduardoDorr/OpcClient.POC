using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using Opc.Ua;

using OpcUaWorkerService.Configuration;
using OpcUaWorkerService.Interfaces;
using OpcUaWorkerService.Services;

namespace OpcUaWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpcUaService _opcUaService;
    private readonly IOpcUaDataHandler _dataHandler;
    private readonly OpcUaVariablesOptions _variablesOptions;

    public Worker(
        ILogger<Worker> logger,
        IOpcUaService opcUaService,
        IOpcUaDataHandler dataHandler,
        IOptions<OpcUaVariablesOptions> variablesOptions)
    {
        _logger = logger;
        _opcUaService = opcUaService;
        _dataHandler = dataHandler;
        _variablesOptions = variablesOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OPC UA Worker Service starting...");

        try
        {
            await _opcUaService.ConnectAsync(stoppingToken);
            await _dataHandler.OnConnectedAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_opcUaService.IsConnected)
                {
                    await ProcessVariablesAsync(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("OPC UA is not connected.");

                    await Task.Delay(5000, stoppingToken);

                    try
                    {
                        await _opcUaService.ConnectAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to reconnect");
                        await _dataHandler.OnErrorAsync(ex, stoppingToken);
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Worker");
            await _dataHandler.OnErrorAsync(ex, stoppingToken);
        }
        finally
        {
            await _opcUaService.DisconnectAsync(stoppingToken);
            await _dataHandler.OnDisconnectedAsync(stoppingToken);
        }
    }

    private async Task ProcessVariablesAsync(CancellationToken stoppingToken)
    {
        foreach (var variable in _variablesOptions.Read)
        {
            try
            {
                var value = await _opcUaService.ReadVariableAsync<bool>(variable.NodeId, stoppingToken);
                _logger.LogInformation("Read {VariableName} ({NodeId}): {Value}", variable.Name, variable.NodeId, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read variable {VariableName}", variable.Name);
            }
        }

        foreach (var variable in _variablesOptions.Write)
        {
            try
            {
                var valueToWrite = true; // variable.DefaultValue ?? "DefaultValue";
                await _opcUaService.WriteVariableAsync(variable.NodeId, valueToWrite, stoppingToken);
                _logger.LogInformation("Wrote {VariableName} ({NodeId}): {Value}", variable.Name, variable.NodeId, valueToWrite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write variable {VariableName}", variable.Name);
            }
        }
    }
}
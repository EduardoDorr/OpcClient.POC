using Microsoft.Extensions.Options;

using Opc.Ua;
using Opc.Ua.Client;

using OpcUaWorkerService.Configuration;

namespace OpcUaWorkerService.Services;

public class OpcUaContext
{
    public Session? Session { get; set; }
    public ApplicationConfiguration? ApplicationConfig { get; set; }
    public OpcUaConnectionOptions ConnectionOptions { get; }

    public OpcUaContext(
        IOptions<OpcUaConnectionOptions> connectionOptions)
    {
        ConnectionOptions = connectionOptions.Value;
    }
}
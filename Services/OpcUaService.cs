using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using System.Security.Cryptography.X509Certificates;

namespace OpcUaWorkerService.Services;

public class OpcUaService : IOpcUaService
{
    private readonly OpcUaContext _context;
    private readonly ILogger<OpcUaService> _logger;

    public bool IsConnected => _context?.Session?.Connected ?? false;

    public OpcUaService(
        OpcUaContext context,
        ILogger<OpcUaService> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to OPC UA server at {EndpointUrl}", _context.ConnectionOptions.EndpointUrl);

        try
        {
            var application = new ApplicationInstance
            {
                ApplicationName = _context.ConnectionOptions.ApplicationName,
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "OpcUaWorkerService"
            };

            var config = await application.Build(
                _context.ConnectionOptions.ApplicationUri,
                _context.ConnectionOptions.ApplicationName)
                .AsClient()
                .AddSecurityConfigurationStores(
                    subjectName: _context.ConnectionOptions.ApplicationName,
                    appRoot: "OPC Foundation/CertificateStores/MachineDefault",
                    trustedRoot: "OPC Foundation/CertificateStores/UA Applications",
                    issuerRoot: "OPC Foundation/CertificateStores/UA Certificate Authorities",
                    rejectedRoot: "OPC Foundation/CertificateStores/RejectedCertificates")
                .CreateAsync();

            _context.ApplicationConfig = config;

            if (_context.ConnectionOptions.UseSecurity)
            {
                if (!string.IsNullOrEmpty(_context.ConnectionOptions.Security.CertificatePath))
                {
                    _logger.LogInformation("Loading certificate from {CertificatePath}", _context.ConnectionOptions.Security.CertificatePath);

                    var certificate =
                        new X509Certificate2(
                            _context.ConnectionOptions.Security.CertificatePath,
                            _context.ConnectionOptions.Security.CertificatePassword,
                            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                    config.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier(certificate);
                    config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(certificate);

                    _logger.LogInformation("Certificate loaded successfully");
                }
                else
                {
                    bool haveAppCertificate = config.SecurityConfiguration.ApplicationCertificate.Certificate != null;

                    if (!haveAppCertificate)
                    {
                        _logger.LogWarning("No certificate path provided, creating self-signed certificate");
                        await application.CheckApplicationInstanceCertificatesAsync(true, 0, cancellationToken);
                    }

                    if (config.SecurityConfiguration.ApplicationCertificate.Certificate != null)
                    {
                        config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(
                            config.SecurityConfiguration.ApplicationCertificate.Certificate);
                    }
                }

                config.SecurityConfiguration.AutoAcceptUntrustedCertificates =
                    _context.ConnectionOptions.Security.AutoAcceptUntrustedCertificates;
            }

            await config.ValidateAsync(ApplicationType.Client, cancellationToken);

            var endpointDescription =
                await CoreClientUtils.SelectEndpointAsync(
                    config,
                    _context.ConnectionOptions.EndpointUrl,
                    _context.ConnectionOptions.UseSecurity,
                    cancellationToken);

            var endpointConfiguration = EndpointConfiguration.Create(config);
            var configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            var session =
                await Session.CreateAsync(
                    config,
                    null,
                    configuredEndpoint,
                    false,
                    false,
                    _context.ConnectionOptions.ApplicationName,
                    (uint)_context.ConnectionOptions.SessionTimeout,
                    new UserIdentity(new AnonymousIdentityToken()),
                    null,
                    cancellationToken);

            _context.Session = session;

            _logger.LogInformation("Successfully connected to OPC UA server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to OPC UA server");
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disconnecting from OPC UA server...");

        try
        {
            if (_context.Session != null)
            {
                await _context.Session.CloseAsync(cancellationToken);
                _context.Session.Dispose();
                _context.Session = null;
            }

            _logger.LogInformation("Successfully disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
        }

        await Task.CompletedTask;
    }

    public async Task<T?> ReadVariableAsync<T>(string nodeId, CancellationToken cancellationToken = default)
    {
        if (_context.Session == null)
            throw new InvalidOperationException("Session is not available");

        try
        {
            var node = new NodeId(nodeId);
            var value = await _context.Session.ReadValueAsync(node, cancellationToken);

            _logger.LogDebug("Read value from {NodeId}: {Value}", nodeId, value.Value);

            return value.GetValueOrDefault<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read variable {NodeId}", nodeId);
            throw;
        }
    }

    public async Task WriteVariableAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default)
    {
        if (_context.Session == null)
            throw new InvalidOperationException("Session is not available");

        try
        {
            var node = new NodeId(nodeId);
            var writeValue = new WriteValue
            {
                NodeId = node,
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(value))
            };

            var writeValueCollection =
                new WriteValueCollection { writeValue };

            var response = await _context.Session
                .WriteAsync(
                    null,
                    writeValueCollection,
                    cancellationToken);

            if (StatusCode.IsBad(response.Results[0]))
                throw new ServiceResultException(response.Results[0]);

            _logger.LogDebug("Wrote value to {NodeId}: {Value}", nodeId, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write variable {NodeId}", nodeId);
            throw;
        }
    }
}
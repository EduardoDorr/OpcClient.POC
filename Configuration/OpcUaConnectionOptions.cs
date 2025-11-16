namespace OpcUaWorkerService.Configuration;

public class OpcUaConnectionOptions
{
    public const string SectionName = "OpcUa:Connection";

    public string EndpointUrl { get; set; } = string.Empty;
    public bool UseSecurity { get; set; } = false;
    public SecurityOptions Security { get; set; } = new();
    public int SessionTimeout { get; set; } = 60000;
    public string ApplicationName { get; set; } = "OpcUaWorkerService";
    public string ApplicationUri { get; set; } = "urn:localhost:OpcUaWorkerService";
}

public class SecurityOptions
{
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public bool AutoAcceptUntrustedCertificates { get; set; } = false;
}

using OpcUaWorkerService;
using OpcUaWorkerService.Configuration;
using OpcUaWorkerService.Services;
using OpcUaWorkerService.Interfaces;
using OpcUaWorkerService.Handlers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OpcUaConnectionOptions>(
    builder.Configuration.GetSection(OpcUaConnectionOptions.SectionName));

builder.Services.Configure<OpcUaVariablesOptions>(
    builder.Configuration.GetSection(OpcUaVariablesOptions.SectionName));

builder.Services.AddSingleton<OpcUaContext>();
builder.Services.AddSingleton<IOpcUaService, OpcUaService>();
builder.Services.AddSingleton<IOpcUaDataHandler, DefaultOpcUaDataHandler>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
# OPC UA Worker Service

Worker Service .NET 8 moderno para comunicação OPC UA com arquitetura extensível baseada no padrão State.

## 🚀 Características

- ✅ **Configuração via IOptions**: Todas as configurações em `appsettings.json`
- ✅ **Suporte a Certificados**: Conexões seguras e não-seguras
- ✅ **Extensível**: Arquitetura preparada para adicionar lógicas customizadas
- ✅ **Reconexão Automática**: Retry automático em caso de falhas
- ✅ **Dependency Injection**: DI configurado para fácil extensão
- ✅ **Logging Estruturado**: Logs detalhados para debugging

## 🔧 Configuração

### 1. Configurar Servidor OPC UA

Edite `appsettings.json`:

```json
{
  "OpcUa": {
    "Connection": {
      "EndpointUrl": "opc.tcp://seu-servidor:4840",
      "UseSecurity": false,
      "SessionTimeout": 60000
    }
  }
}
```

### 2. Configurar Variáveis

Adicione as variáveis que deseja ler/escrever:

```json
{
  "OpcUa": {
    "Variables": {
      "Read": [
        {
          "Name": "CycleStart",
          "NodeId": "ns=3;i=2000",
          "DataType": "Double"
        },
        {
          "Name": "CycleStop",
          "NodeId": "ns=3;i=2001",
          "DataType": "Double",
          "DefaultValue": 25.0
        }
      ],
      "Write": [
        {
          "Name": "ToWrite",
          "NodeId": "ns=3;i=2002",
          "DataType": "Boolean",
          "DefaultValue": "true"
        }
      ]
    }
  }
}
```

## ▶️ Executar

```bash
dotnet run
```

## 🏗️ Estrutura do Projeto

```
OpcUaWorkerService/
├── Configuration/          # Configuração fortemente tipada
│   ├── OpcUaConnectionOptions.cs
│   └── OpcUaVariablesOptions.cs
├── Services/              # Serviço OPC UA
│   ├── IOpcUaService.cs
│   ├── OpcUaService.cs
│   └── OpcUaContext.cs
├── Interfaces/            # Interfaces para extensão
│   └── IOpcUaDataHandler.cs
├── Handlers/              # Handlers customizados
│   └── DefaultOpcUaDataHandler.cs
├── Worker.cs             # Worker principal
└── Program.cs            # Configuração DI
```

## 🔌 Adicionar Lógica Customizada

### 1. Criar Handler Customizado

```csharp
using OpcUaWorkerService.Interfaces;

namespace OpcUaWorkerService.Handlers;

public class MyCustomHandler : IOpcUaDataHandler
{
    private readonly ILogger<MyCustomHandler> _logger;

    public MyCustomHandler(ILogger<MyCustomHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleReadDataAsync(string variableName, string nodeId, object? value, CancellationToken cancellationToken)
    {
        // Sua lógica aqui
        _logger.LogInformation("Valor lido: {Variable} = {Value}", variableName, value);
        
        // Exemplo: salvar em banco de dados, enviar para API, etc.
        
        return Task.CompletedTask;
    }

    public Task HandleWriteDataAsync(string variableName, string nodeId, object value, CancellationToken cancellationToken)
    {
        // Sua lógica aqui
        return Task.CompletedTask;
    }

    public Task OnConnectedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Conectado ao servidor OPC UA!");
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Desconectado do servidor OPC UA");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Erro OPC UA");
        return Task.CompletedTask;
    }
}
```

### 2. Registrar no DI

Em `Program.cs`:

```csharp
// Substituir:
builder.Services.AddSingleton<IOpcUaDataHandler, DefaultOpcUaDataHandler>();

// Por:
builder.Services.AddSingleton<IOpcUaDataHandler, MyCustomHandler>();
```

## 🔐 Conexões Seguras

Para usar certificados:

```json
{
  "OpcUa": {
    "Connection": {
      "EndpointUrl": "opc.tcp://servidor:4840",
      "UseSecurity": true,
      "Security": {
        "CertificatePath": "/path/to/certificate.pfx",
        "CertificatePassword": "senha",
        "AutoAcceptUntrustedCertificates": false
      }
    }
  }
}
```

## 📊 Logs

O serviço gera logs estruturados:

```
info: OpcUaWorkerService.States.OpcUaStateContext[0]
      Transitioning from Disconnected to Connecting
info: OpcUaWorkerService.States.ConnectingState[0]
      Connecting to OPC UA server at opc.tcp://localhost:4840
info: OpcUaWorkerService.States.OpcUaStateContext[0]
      Transitioning from Connecting to Connected
```

## 🧪 Testar com Servidor OPC UA Local

Para testar, você pode usar:

1. **Prosys OPC UA Simulation Server** (gratuito)
2. **UAExpert** como cliente de teste
3. **Open62541** para servidor customizado

## 📦 Dependências

- `OPCFoundation.NetStandard.Opc.Ua` - Cliente OPC UA oficial
- `Microsoft.Extensions.Hosting` - Worker Service framework
- `Microsoft.Extensions.Configuration` - Configuração
- `Microsoft.Extensions.Options` - IOptions pattern
- `Microsoft.Extensions.DependencyInjection` - DI

## 🔄 Próximas Funcionalidades

- [ ] Sistema de subscrições (monitoring)
- [ ] Suporte a múltiplas conexões
- [ ] Gerenciamento automático de certificados
- [ ] Retry com backoff exponencial
- [ ] Métricas e health checks
- [ ] Plugins customizados

## 📝 Notas

- O serviço tenta reconectar automaticamente a cada 5 segundos em caso de falha
- Certificados self-signed são gerados automaticamente quando necessário
- O timeout de sessão padrão é 60 segundos
- O Worker processa variáveis a cada 10 segundos quando conectado

## 🆘 Troubleshooting

### Erro "BadNotConnected"

Este erro é normal quando não há um servidor OPC UA disponível. O serviço tentará reconectar automaticamente.

### Certificado Inválido

Se usar segurança, certifique-se de que:
- O certificado está no formato correto (.pfx ou .p12)
- A senha está correta
- `AutoAcceptUntrustedCertificates` está `true` para testes

### Variável não encontrada

Verifique se o `NodeId` está correto no formato:
- `i=2259` (identificador numérico)
- `ns=2;s=Variable` (string com namespace)
- `ns=2;i=1001` (numérico com namespace)

## 📄 Licença

Este projeto é fornecido como está, sem garantias.

---

**Desenvolvido com .NET 8 e OPC Foundation SDK**

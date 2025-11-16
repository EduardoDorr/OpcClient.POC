# OPC UA Worker Service

## Visão Geral
Worker Service .NET 8 moderno para comunicação OPC UA com arquitetura extensível e gerenciamento de estado. O serviço suporta conexões seguras (com certificado) e não-seguras, permitindo leitura e escrita de variáveis OPC UA configuradas via `appsettings.json` usando o padrão IOptions.

## Objetivos do Projeto
- Comunicação OPC UA com suporte a certificados e sem certificados
- Gerenciamento de estados da conexão usando padrão State
- Configuração via `appsettings.json` com IOptions
- Arquitetura extensível para adicionar lógicas customizadas
- Operações de leitura e escrita de variáveis OPC UA

## Estado Atual
✅ Projeto criado com .NET 8 Worker Service
✅ Padrão State implementado (Disconnected, Connecting, Connected, Error)
✅ Serviço OPC UA com suporte a certificados opcional
✅ Configuração via appsettings.json e IOptions
✅ Operações de leitura e escrita implementadas
✅ Dependency Injection configurado
✅ Arquitetura extensível com interfaces e handlers

## Estrutura do Projeto

```
OpcUaWorkerService/
├── Configuration/          # Classes de configuração fortemente tipadas
│   ├── OpcUaConnectionOptions.cs
│   └── OpcUaVariablesOptions.cs
├── States/                # Implementação do padrão State
│   ├── IOpcUaConnectionState.cs
│   ├── OpcUaStateContext.cs
│   ├── DisconnectedState.cs
│   ├── ConnectingState.cs
│   ├── ConnectedState.cs
│   └── ErrorState.cs
├── Services/              # Serviços OPC UA
│   ├── IOpcUaService.cs
│   └── OpcUaService.cs
├── Interfaces/            # Interfaces para extensibilidade
│   └── IOpcUaDataHandler.cs
├── Handlers/              # Handlers customizados
│   └── DefaultOpcUaDataHandler.cs
├── Worker.cs             # Worker principal
├── Program.cs            # Configuração DI e host
└── appsettings.json      # Configurações
```

## Padrão State

O serviço implementa o padrão State para gerenciar conexões OPC UA:

- **Disconnected**: Estado inicial, sem conexão
- **Connecting**: Tentando estabelecer conexão
- **Connected**: Conectado e pronto para operações
- **Error**: Erro ocorreu, permite recuperação

## Configuração

### appsettings.json

```json
{
  "OpcUa": {
    "Connection": {
      "EndpointUrl": "opc.tcp://localhost:4840",
      "UseSecurity": false,
      "Security": {
        "CertificatePath": "",
        "CertificatePassword": "",
        "AutoAcceptUntrustedCertificates": true
      },
      "SessionTimeout": 60000,
      "ApplicationName": "OpcUaWorkerService",
      "ApplicationUri": "urn:localhost:OpcUaWorkerService"
    },
    "Variables": {
      "Read": [
        {
          "Name": "ServerStatus",
          "NodeId": "i=2259",
          "DataType": "String"
        }
      ],
      "Write": [
        {
          "Name": "CustomVariable",
          "NodeId": "ns=2;s=CustomVariable",
          "DataType": "String",
          "DefaultValue": "HelloWorld"
        }
      ]
    }
  }
}
```

## Como Usar

### 1. Configurar Endpoint OPC UA
Edite `appsettings.json` e configure o `EndpointUrl` do servidor OPC UA.

### 2. Configurar Variáveis
Adicione as variáveis que deseja ler/escrever na seção `Variables`.

### 3. Executar
```bash
dotnet run
```

## Extensibilidade

### Adicionar Lógica Customizada

1. **Criar um Handler Customizado**

```csharp
public class MyCustomHandler : IOpcUaDataHandler
{
    public Task HandleReadDataAsync(string variableName, string nodeId, object? value, CancellationToken cancellationToken)
    {
        // Sua lógica aqui
        return Task.CompletedTask;
    }
    
    // Implementar outros métodos...
}
```

2. **Registrar no DI** (Program.cs)

```csharp
builder.Services.AddSingleton<IOpcUaDataHandler, MyCustomHandler>();
```

### Adicionar Novo Estado

1. Implementar `IOpcUaConnectionState`
2. Definir transições no método correspondente
3. Usar em `OpcUaStateContext`

## Dependências

- .NET 8.0
- OPCFoundation.NetStandard.Opc.Ua
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Options
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## Próximos Passos

- Implementar sistema de subscrições para monitoramento em tempo real
- Adicionar gerenciamento automático de certificados
- Criar estratégias de retry e reconexão automática
- Suportar múltiplas conexões OPC UA simultâneas
- Implementar eventos e callbacks para mudanças de estado
- Criar sistema de plugins para lógicas customizadas

## Arquitetura

O projeto usa:
- **Padrão State**: Para gerenciar estados da conexão OPC UA
- **Dependency Injection**: Para configuração e extensibilidade
- **IOptions Pattern**: Para configuração fortemente tipada
- **Interface Segregation**: Para permitir extensões customizadas
- **SOLID Principles**: Para código limpo e manutenível

## Notas Técnicas

- O serviço tenta reconectar automaticamente em caso de falha
- Suporta conexões seguras (com certificado) e não-seguras
- Auto-geração de certificados self-signed quando necessário
- Logging estruturado para debugging
- Tratamento de erros robusto com transições de estado

## Desenvolvedor

Para adicionar sua própria lógica de negócio:
1. Implemente `IOpcUaDataHandler` com sua lógica
2. Registre no container DI em `Program.cs`
3. O Worker chamará seus handlers automaticamente

---

**Última Atualização**: 2025-11-04
**Versão .NET**: 8.0
**Status**: ✅ Funcional

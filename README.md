# GroupMeBotPPE

A serverless GroupMe chat bot built with Azure Functions (.NET 10, isolated worker model). The bot listens for incoming GroupMe webhook messages and responds with GIFs or canned text responses based on message content.

## Architecture

The solution uses a clean architecture layout without a separate Domain layer for now:

- **Presentation** - Azure Functions HTTP trigger entry point and composition root. Receives webhook callbacks from GroupMe and delegates to the Application layer.
- **Application** - Application logic. Contains bot services, message parsing, application models, and utilities.
- **Infrastructure** - External service integrations and their dependency injection registration.
- **Application.UnitTest** - Unit tests using MSTest and Moq.

## Bot Commands

| Trigger | Example | Behavior |
|---------|---------|----------|
| `Gif:<query>` | `Gif: funny cat` | Searches Giphy and posts the top result to the chat |
| `bot message` | `hey bot message me` | Posts a random canned response (personalized per user) |
| `bot analysis` | `bot analysis` | Placeholder for future analysis features |

The bot ignores its own messages to avoid infinite loops.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Configuration

The following settings are required. Store secrets in user secrets for local development
or environment variables/application settings in deployed environments; do not commit them
to `appsettings.json`.

| Key | Description |
|-----|-------------|
| `GroupMePostUri` | GroupMe bot post API endpoint (e.g. `https://api.groupme.com/v3/bots/post`) |
| `GroupMeBotId` | Your GroupMe bot ID |
| `GroupMeAccessToken` | GroupMe access token used to retrieve recent conversation history |
| `GiphyBotId` | Your Giphy API key |
| `Anthropic:ApiKey` | Anthropic API key used to generate achievements (`Anthropic__ApiKey` as an environment variable) |

For Azure Key Vault references, use `Anthropic-ApiKey` as the Key Vault secret name
because Key Vault secret names cannot contain underscores. Create a Function App application
setting named `Anthropic__ApiKey` whose value references that Key Vault secret. The .NET
environment-variable provider maps the double underscore to `Anthropic:ApiKey`.

### Setting up user secrets (local development)

```bash
cd Presentation
dotnet user-secrets set "GroupMePostUri" "https://api.groupme.com/v3/bots/post"
dotnet user-secrets set "GroupMeBotId" "your-bot-id"
dotnet user-secrets set "GroupMeAccessToken" "your-groupme-access-token"
dotnet user-secrets set "GiphyBotId" "your-giphy-api-key"
dotnet user-secrets set "Anthropic:ApiKey" "your-anthropic-api-key"
```

## Building and Running

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run locally
cd Presentation
func start

# Run tests
dotnet test
```

## Project Structure

```
GroupMeBotPPE/
├── Presentation/
│   ├── BasicResponse.cs        # Azure Function HTTP trigger
│   ├── Program.cs              # Application entry point and DI setup
│   ├── appsettings.json        # App configuration
│   └── host.json               # Azure Functions host configuration
├── Application/
│   ├── BotService/
│   │   ├── MessageBot.cs       # Canned text response bot
│   │   ├── GifBot.cs           # Giphy search bot
│   │   └── AnalysisBot.cs      # Placeholder analysis bot
│   ├── MessageService/
│   │   ├── MessageIncoming.cs  # Incoming webhook parser and router
│   │   └── MessageOutgoing.cs  # Posts responses to GroupMe API
│   ├── Entities/
│   │   ├── MessageItem.cs      # GroupMe message data model
│   │   └── CreateBotPostRequest.cs
│   └── Utilities/
│       ├── BotPostConfiguration.cs
│       ├── GiphyBotPostConfig.cs
│       └── JsonSerializer.cs
├── Infrastructure/
│   ├── Ai/                      # AI provider abstractions and clients
│   └── DependencyInjection/     # Infrastructure service registration
├── Application.UnitTest/
│   ├── BotService/
│   │   └── MessageBotUnitTest.cs
│   └── Presentation/
│       └── StartupTests.cs
├── GroupMeBot.sln
├── LICENSE
└── README.md
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

# GroupMeBotPPE

A serverless GroupMe chat bot built with Azure Functions (.NET 10, isolated worker model). The bot listens for incoming GroupMe webhook messages and responds with GIFs or canned text responses based on message content.

## Architecture

The solution is organized into three projects:

- **Controller** - Azure Functions HTTP trigger entry point. Receives webhook callbacks from GroupMe and delegates to the Model layer.
- **Model** - Core business logic. Contains bot services, message parsing, entity models, and utilities.
- **Model.UnitTest** - Unit tests using MSTest and Moq.

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

The following settings are required and can be provided via `appsettings.json`, user secrets, or environment variables:

| Key | Description |
|-----|-------------|
| `GroupMePostUri` | GroupMe bot post API endpoint (e.g. `https://api.groupme.com/v3/bots/post`) |
| `GroupMeBotId` | Your GroupMe bot ID |
| `GiphyBotId` | Your Giphy API key |

### Setting up user secrets (local development)

```bash
cd Controller
dotnet user-secrets set "GroupMePostUri" "https://api.groupme.com/v3/bots/post"
dotnet user-secrets set "GroupMeBotId" "your-bot-id"
dotnet user-secrets set "GiphyBotId" "your-giphy-api-key"
```

## Building and Running

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run locally
cd Controller
func start

# Run tests
dotnet test
```

## Project Structure

```
GroupMeBotPPE/
├── Controller/
│   ├── BasicResponse.cs        # Azure Function HTTP trigger
│   ├── Program.cs              # Application entry point and DI setup
│   ├── appsettings.json        # App configuration
│   └── host.json               # Azure Functions host configuration
├── Model/
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
├── Model.UnitTest/
│   ├── BotService/
│   │   └── MessageBotUnitTest.cs
│   └── Controller/
│       └── StartupTests.cs
├── GroupMeBot.sln
├── LICENSE
└── README.md
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

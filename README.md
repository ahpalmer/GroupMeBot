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
| `bot achievement` | `bot achievement` | Generates a Dungeon Crawler Carl style achievement about the sender, plus an image |

The bot ignores its own messages to avoid infinite loops. Achievements also fire at
random on ordinary messages.

## Achievement images

Every achievement is followed a few seconds later by a generated image: a Dungeon
Crawler Carl style achievement card with a caricature of the crawler in question.

Because generation takes tens of seconds and GroupMe retries slow webhook callbacks,
the work is split across two functions:

1. `BasicResponse` (HTTP trigger) posts the achievement text and enqueues an
   `AchievementImageRequest` on the `achievement-images` storage queue, then returns.
2. `AchievementImageWorker` (queue trigger) fetches the crawler's reference photo,
   calls Gemini, uploads the result to GroupMe's image service, and posts it as an
   attachment.

Failures degrade quietly — a missing reference photo produces a generic crawler
figure, and a refusal or upload failure leaves the achievement as text-only. Set
`Achievement:ImagesEnabled` to `false` to turn images off without a redeploy.

### Reference photos

Likeness comes from one reference headshot per member, stored in a **private** blob
container rather than in this repository, which is public. Blobs are named after the
member's GroupMe user id.

```bash
az storage container create --name achievement-people --account-name <acct> --public-access off

# one clear, front-facing, well-lit headshot each, ~1024px, JPEG
for id in 4635437 20597076 7663415 11900950; do
  az storage blob upload --container-name achievement-people \
    --file ./$id.jpg --name $id.jpg --account-name <acct>
done
```

Reference quality is the single biggest factor in whether the caricature is
recognizable — a sharp single-subject headshot beats a group photo by a wide margin.
Members without a photo still get an achievement image, just with a generic figure.

Map user ids to display names under `AchievementPhotos:People` in `appsettings.json`.

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
| `Anthropic:ApiKey` | Anthropic API key used to generate achievement text (`Anthropic__ApiKey` as an environment variable) |
| `Google:ApiKey` | Google AI Studio API key used to generate achievement images (`Google__ApiKey` as an environment variable) |
| `AzureWebJobsStorage` | Functions host storage account; also backs the image queue and the reference photo container |

These optional settings have defaults in `appsettings.json`:

| Key | Default | Description |
|-----|---------|-------------|
| `Google:DefaultImageModel` | `gemini-3-pro-image` | Image model. The Pro tier renders the in-image achievement title noticeably better than the Flash tiers; drop to `gemini-3.1-flash-image` to trade that for cost and latency |
| `Google:ImageSize` | `1K` | Resolution tier |
| `Achievement:ImagesEnabled` | `true` | Kill switch for achievement image generation |
| `AchievementPhotos:ContainerName` | `achievement-people` | Private blob container holding reference headshots |
| `AchievementPhotos:People` | four entries | GroupMe user id to display name |

For Azure Key Vault references, use `Anthropic-ApiKey` and `Google-ApiKey` as the Key
Vault secret names because Key Vault secret names cannot contain underscores. Create
Function App application settings named `Anthropic__ApiKey` and `Google__ApiKey` whose
values reference those Key Vault secrets. The .NET environment-variable provider maps
the double underscore to `Anthropic:ApiKey` / `Google:ApiKey`.

### Setting up user secrets (local development)

```bash
cd Presentation
dotnet user-secrets set "GroupMePostUri" "https://api.groupme.com/v3/bots/post"
dotnet user-secrets set "GroupMeBotId" "your-bot-id"
dotnet user-secrets set "GroupMeAccessToken" "your-groupme-access-token"
dotnet user-secrets set "GiphyBotId" "your-giphy-api-key"
dotnet user-secrets set "Anthropic:ApiKey" "your-anthropic-api-key"
dotnet user-secrets set "Google:ApiKey" "your-google-ai-studio-api-key"
```

Running locally also needs [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
for the image queue and the photo container (`AzureWebJobsStorage` defaults to
`UseDevelopmentStorage=true`).

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
│   ├── BasicResponse.cs             # Azure Function HTTP trigger
│   ├── AchievementImageWorker.cs    # Queue-triggered image generation worker
│   ├── Program.cs                   # Application entry point and DI setup
│   ├── appsettings.json             # App configuration
│   └── host.json                    # Azure Functions host configuration
├── Application/
│   ├── BotService/
│   │   ├── MessageBot.cs            # Canned text response bot
│   │   ├── GifBot.cs                # Giphy search bot
│   │   ├── AnalysisBot.cs           # Placeholder analysis bot
│   │   ├── AchievementBot.cs        # Generates achievement text
│   │   └── AchievementImageBot.cs   # Generates and posts the achievement image
│   ├── MessageService/
│   │   ├── MessageIncoming.cs       # Incoming webhook parser and router
│   │   ├── MessageOutgoing.cs       # Posts responses to GroupMe API
│   │   ├── GroupMeMessageHistory.cs # Reads recent conversation history
│   │   ├── GroupMeImageUploader.cs  # Uploads images to image.groupme.com
│   │   └── StorageAchievementImageQueue.cs
│   ├── Entities/
│   │   ├── MessageItem.cs           # GroupMe message data model
│   │   ├── CreateBotPostRequest.cs
│   │   ├── Attachment.cs
│   │   └── AchievementImageRequest.cs
│   └── Utilities/
│       ├── BotPostConfiguration.cs
│       ├── GiphyBotPostConfig.cs
│       └── JsonSerializer.cs
├── Infrastructure/
│   ├── Ai/                          # AI provider abstractions and clients
│   │   ├── Anthropic/               # Claude, for achievement text
│   │   └── Google/                  # Gemini, for achievement images
│   ├── Storage/                     # Blob-backed reference photo store
│   └── DependencyInjection/         # Infrastructure service registration
├── Application.UnitTest/
│   ├── BotService/
│   │   ├── MessageBotUnitTest.cs
│   │   ├── AchievementBotUnitTest.cs
│   │   └── AchievementImageBotUnitTest.cs
│   └── Presentation/
│       ├── StartupTests.cs
│       ├── MessageIncomingTests.cs
│       ├── GroupMeMessageHistoryTests.cs
│       └── GroupMeImageUploaderTests.cs
├── GroupMeBot.sln
├── LICENSE
└── README.md
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

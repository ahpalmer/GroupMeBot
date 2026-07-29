using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using GroupMeBot.Application;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;

namespace GroupMeBot.Application;

public class MessageBot : IMessageBot
{
    private IMessageOutgoing _messageOutgoing;
    private IBotPostConfiguration _botPostConfiguration;
    private ILogger _log;

    private static readonly Regex _botCannedResponseRegex = new Regex(@"((?i)(?=.*\bbot\b)(?=.*\bmessage\b)(?=.*\bresponse\b)(?-i))");

    public MessageBot(IMessageOutgoing messageOutgoing,
        IBotPostConfiguration botPostConfiguration,
        ILogger<MessageBot> log)
    {
        _messageOutgoing = messageOutgoing;
        _botPostConfiguration = botPostConfiguration;
        _log = log;
    }

    public async Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message)
    {
        try
        {
            _log.LogInformation("MessageBot-HandleIncomingTextAsync method start");
            if (message.Text == null)
            {
                _log.LogWarning("MessageBot-message.text is null");
                return HttpStatusCode.BadRequest;
            }

            // Todo: Make this into a bitwise operator (?)
            if (message.UserId == "4635437") return await ChooseUniqueUserTextAsync("Andrew");
            else if (message.UserId == "20597076") return await ChooseUniqueUserTextAsync("Logan");
            else if (message.UserId == "7663415") return await ChooseUniqueUserTextAsync("Sean");
            else if (message.UserId == "11900950") return await ChooseUniqueUserTextAsync("Jordan");
            else if (message.UserId == "84706251") return await ChooseUniqueUserTextAsync("Hayden");
            else return await ChooseUniqueUserTextAsync();
        }
        catch (Exception ex)
        {
            _log.LogError($"MessageBot-HandleIncomingTextAsync method failed, {ex}");
            return HttpStatusCode.BadRequest;
        }
    }

    public async Task<HttpStatusCode> ChooseUniqueUserTextAsync(string user = "")
    {
        try
        {
            _log.LogInformation($"MessageBot-ChooseUniqueUserTextAsync method start for user: {user}");

            string response = RetrieveRandomResponse(user);

            _log.LogInformation($"MessageBot-Response for user: {user} retrieved: {response}");

            return await _messageOutgoing.PostAsync($"{response}", _botPostConfiguration.BotId);
        }
        catch(Exception ex)
        {
            _log.LogError($"MessageBot-ChooseUniqueUserTextAsync method failed, {ex}");
            return HttpStatusCode.BadRequest;
        }
    }

    public string RetrieveRandomResponse(string person)
    {
        try
        {
            _log.LogInformation("RetrieveRandomResponse");

            List<string> responses = RetrieveSpecificPersonResponse(person);
            int random = RandomNumberGenerator.GetInt32(0, responses.Count);
            return responses[random].ToString();
        }
        catch (Exception ex)
        {
            _log.LogError($"MessageBot-RetrieveRandomResponse method failed, {ex}");
            return "RetrieveRandomResponse-Bad Return";
        }
    }

    public List<string> RetrieveSpecificPersonResponse(string person)
    {
        switch (person)
        {
            case "Andrew":
                return new List<string>
                {
                    "I agree completely!",
                    "You are as correct as you are handsome",
                    "I agree with you 100%",
                    "Gosh you're so smart",
                    "HELP I'M TRAPPED IN A FACTORY THAT MAKES COMPLIMENTS, FUCK YOU ANDREW, FUCK Y",
                    "You're an awesome friend.",
                    "You have a great sense of humor!",
                    "You is strong, you is smart, you is important",
                    "You are inspiring",
                    "You are brave and courageous."
                };
            case "Logan":
                return new List<string>
                {
                    "No.",
                    "Fuck you",
                    "Not if you were the last guy on earth",
                    "You have a face for radio",
                    "I smell something burning.  Are you trying to think again?",
                    "You are the Michael Jordan of assholes",
                    "Don't worry, the first 31 years of childhood are the hardest"
                };
            case "Sean":
                return new List<string>
                {
                    "...okay but only if you were the LAST guy on earth",
                    "WHAT IN TARNATION",
                    "I smell something burning.  Are you trying to think again?",
                    "I disagree with everything you say",
                    "I'm sorry, I can't hear you over the sound of how awesome I am.",
                    "BRO, DO YOU EVEN LIFT?"
                };
            case "Jordan":
                return new List<string>
                {
                    "WHAT IN TARNATION",
                    "You have a lovely family and a big dumb brain!",
                    "All signs point to no",
                    "Absolutely No!",
                    "Andrew is the best",
                    "Dumb"
                };
            case "Hayden":
                return new List<string>
                {
                    "All signs point to no",
                    "Absolutely No!",
                    "Andrew is the best",
                    "Ask again later",
                    "You have a face for radio",
                    "You'd think the Secret Service woulda taught you to be less ugly"
                };
            case "Compliments":
                return new List<string>
                {
                    "You are as correct as you are handsome",
                    "I agree with you 100%",
                    "Gosh you're so smart",
                    "HELP I'M TRAPPED IN A FACTORY THAT MAKES COMPLIMENTS, FUCK YOU ANDREW, FUCK Y",
                    "You're an awesome friend.",
                    "You have a great sense of humor!",
                    "You is strong, you is smart, you is important",
                    "You are inspiring",
                    "You are brave and courageous."
                };
            case "Insults":
                return new List<string>
                {
                    "You have a face for radio",
                    "I smell something burning.  Are you trying to think again?",
                    "You are the Michael Jordan of assholes",
                    "Don't worry, the first 31 years of childhood are the hardest",
                    "Not if you were the last guy on earth"
                };
            default:
                return new List<string>
                {
                    "This is an anonymous response",
                    "Yes",
                    "Sure",
                    "All signs point to no"
                };
        }
    }
}

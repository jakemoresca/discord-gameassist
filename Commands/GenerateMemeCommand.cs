using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GenerateMemeCommand : ISlashCommand
{
    private readonly IConfigurationRoot _config;

    public GenerateMemeCommand(IConfigurationRoot config)
    {
        _config = config;
    }

    public string CommandName => SlashCommands.GenerateMeme;

    public async Task Handle(SocketSlashCommand command)
    {
        var prompt = command.Data.Options.First().Value.ToString() ?? "Any";

        var apiKey = _config.GetRequiredSection("Settings")["OpenAPIKey"] ?? string.Empty;
        var generatedText = await GenerateMeme(prompt, apiKey);

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(command.User)
            .WithTitle($"A user asks me: \"{prompt}\"")
            .WithCurrentTimestamp();

        if (generatedText.Length > 0)
        {
            embedBuilder.WithImageUrl(generatedText)
                .WithColor(Color.Green);
        }
        else
        {
            embedBuilder.WithDescription($"Catastrophic! I cannot present.")
                .WithColor(Color.Red);
        }

        await command.FollowupAsync(embed: embedBuilder.Build());
    }

    async Task<string> GenerateMeme(string prompt, string apiKey)
    {
        var url = "https://api.openai.com/v1/chat/completions";

        var memeRules = $@"Prompt: Create a new meme about ""{prompt}"". Only output the URL in this format: https://api.memegen.link/images/<MEME KEY THAT BEST FITS THE MEME>/<THE TEXT OF THE MEME HERE FORMATTED FOR A URL><'/' AND OPTIONAL BOTTOM TEXT FORMATTED FOR URL>.jpg
Meme key - you can use any of them:
Use ds meme key for memes that presents a choice but both are negative, first choice on first text and second choice on second text.
Use leo meme key for memes that don't care.
Use oag meme key for memes that has stalker vibes.
Use headaches meme key for memes that is stress inducing, only generate first text.
Use fine meme key for memes that is catastrophic but not minding the issue.
Use harold meme key for memes that hiding the pain.
Use agnes meme key for memes that is obviously a lie.
Use spiderman meme key for caughting each other.
Use aag meme key for unexplained things, only generate first text.
Use afraid meme key for questioning a norm but you did not know the reason.
Use ams meme key for awkward moments.
Use wddth meme key for expressing we don't do that here.
Use kombucha meme key for trying for the first time.
Use bilbo meme key for questioning yourself.
Use disastergirl meme key for implying you cause a disaster.
Use dwight meme key for correcting a popular belief.
Use exit meme key for disapproving something on first text and prefer the second text.
Use grave meme key for second text expressing enjoyment on the death of first text.
Use money meme key for approving of something.
There is no chatgpt meme key. Do no put meme key that is not listed here. Do not use the following characters (?). Use _ instead of space.";

        var messages = new[] { new { role = "system", content = memeRules } };
        var data = new { model = "gpt-3.5-turbo", max_tokens = 150, temperature = 0.7, messages };
        var json = JsonConvert.SerializeObject(data);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
            {
                request.Content = stringContent;

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseMessage = (JsonConvert.DeserializeObject<JObject>(responseBody))?["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;

                Console.WriteLine($"Generated image: {responseMessage}");

                return responseMessage;
            }
        }
    }

}
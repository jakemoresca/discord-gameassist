// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordGameAssist.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var discordConfig = new DiscordSocketConfig
{
    //AlwaysDownloadUsers = true,
    //MessageCacheSize = 100,
    LogLevel = LogSeverity.Debug,
    //GatewayIntents = GatewayIntents.All
};

var client = new DiscordSocketClient(discordConfig);


var commands = new CommandService(new CommandServiceConfig
{
    // Again, log level:
    LogLevel = LogSeverity.Info,

    // There's a few more properties you can set,
    // for example, case-insensitive commands.
    CaseSensitiveCommands = false,
});

// Setup your DI container.
Bootstrapper.Init();
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(commands);
Bootstrapper.RegisterType<ICommandHandler, CommandHandler>();
Bootstrapper.RegisterInstance(config);

await MainAsync();

async Task MainAsync()
{
    client.Ready += Client_Ready;
    //client.MessageReceived += HandleCommandAsync;

    // Login and connect.
    var token = config.GetRequiredSection("Settings")["DiscordBotToken"];
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Token is null or empty.");
        //await Logger.Log(LogSeverity.Error, $"{nameof(Program)} | {nameof(MainAsync)}", "Token is null or empty.");
        return;
    }

    await client.LoginAsync(TokenType.Bot, token);
    await client.StartAsync();

    // Wait infinitely so your bot actually stays connected.
    await Task.Delay(Timeout.Infinite);
}

async Task Client_Ready()
{
    Console.WriteLine("Client Ready");

    var pingCommand = new SlashCommandBuilder();
    pingCommand.WithName("is-service-available")
        .WithDescription("Check if the service is available.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("server")
            .WithDescription("Select the server you want to check.")
            .WithRequired(true)
            .AddChoice("Mediaserver", "mediaserver")
            .AddChoice("Project Zomboid", "process|cmd|StartServer64_nosteam.bat - Shortcut")
            .AddChoice("The Forest", "process|TheForestDedicatedServer")
            .WithType(ApplicationCommandOptionType.String)
        );

    // var highlightsCommand = new SlashCommandBuilder();
    // highlightsCommand.WithName("random-highlights")
    //     .WithDescription("Get a random highlights.");

    var startServiceCommand = new SlashCommandBuilder();
    startServiceCommand.WithName("start-service")
        .WithDescription("Start a service if not already started.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("server")
            .WithDescription("Select the server you want to check.")
            .WithRequired(true)
            .AddChoice("Project Zomboid", "project-zomboid")
            .AddChoice("The Forest", "the-forest-link")
            .WithType(ApplicationCommandOptionType.String)
        );

    var plexInviteCommand = new SlashCommandBuilder();
    plexInviteCommand.WithName("plex-invite")
        .WithDescription("Join PressX Media Plex Server.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("email")
            .WithDescription("Type in the email you use to register at plex.tv")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String)
        );

    var jokeCommand = new SlashCommandBuilder();
    jokeCommand.WithName("joel-joke")
        .WithDescription("Get some inspirational thoughts from our idol.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("category")
            .WithDescription("Select the category of the joke you want.")
            .WithRequired(true)
            .AddChoice("Any", "Any")
            .AddChoice("Programming", "Programming")
            .AddChoice("Misc", "Misc")
            .AddChoice("Dark", "Dark")
            .AddChoice("Pun", "Pun")
            .AddChoice("Spooky", "Spooky")
            .AddChoice("Christmas", "Christmas")
            .WithType(ApplicationCommandOptionType.String)
        );

    var generateMemeCommand = new SlashCommandBuilder();
    generateMemeCommand.WithName("generate-meme")
        .WithDescription("The JoelGPT we are waiting for! Generate some meme.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("prompt")
            .WithDescription("Type in the meme you want to generate.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String)
        );

    client.SlashCommandExecuted += SlashCommandHandler;
    //client.MessageReceived += HandleCommandAsync;

    try
    {
        await client.CreateGlobalApplicationCommandAsync(pingCommand.Build());
        //await client.CreateGlobalApplicationCommandAsync(highlightsCommand.Build());
        await client.CreateGlobalApplicationCommandAsync(startServiceCommand.Build());
        await client.CreateGlobalApplicationCommandAsync(plexInviteCommand.Build());
        await client.CreateGlobalApplicationCommandAsync(jokeCommand.Build());
        await client.CreateGlobalApplicationCommandAsync(generateMemeCommand.Build());
    }
    catch (HttpException exception)
    {
        // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
        var json = JsonConvert.SerializeObject(exception.Message, Formatting.Indented);

        // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
        Console.WriteLine(json);
    }
}

async Task HandleCommandAsync(SocketMessage messageParam)
{
    // Don't process the command if it was a system message
    var message = messageParam as SocketUserMessage;
    if (message == null) return;

    if (client == null) return;

    // Create a WebSocket-based command context based on the message
    //var context = new SocketCommandContext(client, message);

    var botUser = client.CurrentUser;
    Console.WriteLine($"Message Received: {message.Author} {messageParam.Content} botuser: {botUser.Username}");

    if (messageParam.Content.Contains("joel", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine($"Sending Reply: Leche U! " + message.Author.Mention + ".");
        //await message.Channel.SendMessageAsync("Leche U! " + message.Author.Mention + ".");
        try
        {

            //await messageParam.Channel.SendMessageAsync("Leche U! " + message.Author.Mention + ".");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // if (message.MentionedUsers.Any(x => x.Username == botUser.Username))
    // {
    //     await message.Channel.SendMessageAsync("Leche U! " + message.Author.Mention + ".");
    // }
}

async Task SlashCommandHandler(SocketSlashCommand command)
{
    await HandleJoelFlavoringFollowUps(command);

    switch (command.Data.Name)
    {
        case "is-service-available":
            var serviceName = command.Data.Options.First().Value.ToString() ?? string.Empty;
            var isProcessCheck = serviceName.StartsWith("process");

            if (isProcessCheck)
            {
                await HandleProcessCheckCommand(command);
            }
            else
            {
                await HandlePingCommand(command);
            }
            break;

        case "random-highlights":
            await HandleRandomChannelVideoCommand(command);
            break;

        case "start-service":
            await HandleStartServiceCommand(command);
            break;

        case "plex-invite":
            await HandlePlexInviteCommand(command);
            break;

        case "joel-joke":
            await HandleJokeCommand(command);
            break;

        case "generate-meme":
            await HandleGenrateMemeCommand(command);
            break;
    }
}

async Task HandleJokeCommand(SocketSlashCommand command)
{
    var category = command.Data.Options.First().Value.ToString() ?? "Any";

    using var client = new HttpClient();
    var response = await client.GetAsync($"https://v2.jokeapi.dev/joke/{category}?format=txt");

    var embedBuilder = new EmbedBuilder()
    .WithAuthor(command.User)
    .WithTitle($"Joel's joke for the day")
    .WithCurrentTimestamp();

    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync();

        embedBuilder.WithDescription(responseContent)
            .WithColor(Color.Green);
    }
    else
    {
        embedBuilder.WithDescription($"Catastrophic! I cannot present.")
            .WithColor(Color.Red);
    }

    await command.FollowupAsync(embed: embedBuilder.Build());
}

async Task HandleGenrateMemeCommand(SocketSlashCommand command)
{
    var prompt = command.Data.Options.First().Value.ToString() ?? "Any";

    var apiKey = config.GetRequiredSection("Settings")["OpenAPIKey"] ?? string.Empty;
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

async Task HandlePingCommand(SocketSlashCommand command)
{
    var host = command.Data.Options.First().Value.ToString() ?? string.Empty;
    await command.FollowupAsync($"Checking the availability of {host}");

    var hasPort = host.Contains(":");
    var isSuccess = PingHostWithoutPort(host);

    var embedBuilder = new EmbedBuilder()
        .WithAuthor(command.User)
        .WithTitle($"Availability of {host}")
        .WithCurrentTimestamp();

    if (isSuccess)
    {
        embedBuilder.WithDescription($"{host} is Online.")
            .WithColor(Color.Green);
    }
    else
    {
        embedBuilder.WithDescription($"{host} is Offline.")
            .WithColor(Color.Red);
    }

    await command.FollowupAsync(embed: embedBuilder.Build());
}

static bool PingHostWithoutPort(string hostUri)
{
    Ping pingSender = new Ping();
    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    byte[] buffer = Encoding.ASCII.GetBytes(data);
    PingOptions options = new PingOptions(64, true);
    AutoResetEvent waiter = new AutoResetEvent(false);

    PingReply reply = pingSender.Send(hostUri, 12000, buffer, options);

    return reply.Status == IPStatus.Success;
}

async Task HandleProcessCheckCommand(SocketSlashCommand command)
{
    var process = command.Data.Options.First().Value.ToString() ?? string.Empty;
    var processName = process.Split("|")[1];

    await command.FollowupAsync($"Ah eh... Checking the availability of {processName}");
    var isSuccess = CheckProcessIfRunning(processName, command);

    var embedBuilder = new EmbedBuilder()
        .WithAuthor(command.User)
        .WithTitle($"Availability of {processName}")
        .WithCurrentTimestamp();

    if (isSuccess)
    {
        embedBuilder.WithDescription($"{processName} is Online.")
            .WithColor(Color.Green);
    }
    else
    {
        embedBuilder.WithDescription($"{processName} is Offline.")
            .WithColor(Color.Red);
    }

    await command.FollowupAsync(embed: embedBuilder.Build());
}

bool CheckProcessIfRunning(string processName, SocketSlashCommand command)
{
    if (processName.ToLower() == "cmd")
    {
        var processArgument = command.Data.Options.First().Value.ToString() ?? string.Empty;
        var title = processArgument.Split("|")[2];
        var processes = Process.GetProcessesByName(processName);

        foreach (var process in processes)
        {
            if (process.MainWindowTitle == title)
            {
                return true;
            }
        }

        return false;
    }
    else
    {
        var processes = Process.GetProcessesByName(processName);

        return processes.Length > 0;
    }
}

async Task HandleRandomChannelVideoCommand(SocketSlashCommand command)
{
    await command.FollowupAsync($"Getting random highlights.");

    if (client == null)
        return;

    var channelId = ulong.Parse(config.GetRequiredSection("Settings")["HighlightsChannelId"]?.ToString() ?? string.Empty);
    var channel = await client.GetChannelAsync(channelId) as ITextChannel;

    if (channel == null)
        return;

    var messages = await channel.GetMessagesAsync().FlattenAsync();
    var videos = messages.Where(m => m.Attachments.Any() || m.Content.Any());
    var embeds = messages.Where(m => m.Embeds.Any() || m.Content.Any());

    var test = await channel.GetMessageAsync(1087925220561653831);

    if (!videos.Any() && !embeds.Any())
    {
        await command.FollowupAsync("No videos found in that channel.");
        return;
    }

    if (videos.Any())
    {
        var randomVideo = videos.ElementAt(new Random().Next(videos.Count()));
        await command.FollowupAsync(randomVideo.Attachments.First().Url);
    }
    else if (embeds.Any())
    {
        var randomEmbed = embeds.ElementAt(new Random().Next(embeds.Count()));
        await command.FollowupAsync(randomEmbed.Embeds.First().Url);
    }
}

async Task HandleStartServiceCommand(SocketSlashCommand command)
{
    var serviceName = command.Data.Options.First().Value.ToString() ?? string.Empty;
    await command.FollowupAsync($"Starting {serviceName}");

    try
    {
        var applicationFolderPath = Path.Combine(@"C:\apps\", serviceName);

        string[] extensions = new[] { ".exe", ".bat", ".cmd", ".lnk" };

        FileInfo[] files = new DirectoryInfo(applicationFolderPath)
            .EnumerateFiles()
                 .Where(f => extensions.Contains(f.Extension.ToLower()))
                 .ToArray();

        var process = new ProcessStartInfo(files[0].FullName);
        process.WorkingDirectory = applicationFolderPath;
        process.UseShellExecute = true;
        Process.Start(process);
    }
    catch (Exception ex)
    {
        await command.FollowupAsync($"Failed to start {serviceName}. {ex.Message}");
    }

    await command.FollowupAsync($"Started {serviceName} successfully.");
}

async Task HandlePlexInviteCommand(SocketSlashCommand command)
{
    var inviteeEmail = command.Data.Options.First().Value.ToString() ?? string.Empty;

    await command.FollowupAsync($"Creating invitation...");

    // Create an HTTP client
    var client = new HttpClient();

    // Set the Authorization header to include the user's Plex token
    var baseUrl = "https://plex.tv";
    var serverId = config.GetRequiredSection("Settings")["ServerId"];
    var plexToken = config.GetRequiredSection("Settings")["PlexToken"];
    client.DefaultRequestHeaders.Add("X-Plex-Token", plexToken);
    client.DefaultRequestHeaders.Add("X-Plex-Client-Identifier", "au9tfexlaznwm2n5cckacppg");

    // Build the URL for the invite endpoint
    string url = $"{baseUrl}/api/v2/shared_servers";

    // Create the JSON body for the request
    var json = new JObject
        {
            { "invitedEmail", inviteeEmail },
            { "settings",
                new JObject{
                    { "allowChannels", true },
                    { "allowSubtitleAdmin", true },
                    { "allowSync", true },
                    { "allowTuners", 0 },
                    {"filterMovies", ""},
                    {"filterMusic", ""},
                    {"filterTelevision", ""}
                }
            },
            { "librarySectionIds", new JArray {112155685, 112155692} },
            { "machineIdentifier", "2a557d987b9d87b6ec2776d7acbfb2cac5fff185" }
        };

    // Make the request to the Plex API
    var response = client.PostAsync(url, new StringContent(json.ToString(), System.Text.Encoding.UTF8, "application/json")).Result;

    // check if request is successfull
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Invitation sent successfully to: {inviteeEmail}");
        await command.FollowupAsync($"Invitation sent successfully to: {inviteeEmail}");
    }
    else
    {
        Console.WriteLine($"Failed to send the invitation, please check the credentials and the serverId. reason: {await response.Content.ReadAsStringAsync()}");
        await command.FollowupAsync("Failed to send the invitation, please check the credentials and the serverId");
    }
}

async Task<string> GenerateMeme(string prompt, string apiKey)
{
    var url = "https://api.openai.com/v1/chat/completions";
    //var memeRules = "You are to become a meme generator. This is the url structure: https://apimeme.com/meme?meme={MEME_TYPE}&top={TOP_TEXT}&bottom={BOTTOM_TEXT}\nYou are to place the generated link as an image markdown. ![MEME_NAME](MEME_LINK)\nYou will reply with a markdown image of the generated meme.\n\nThe joke will be \"" + prompt +"\"\n\nRULES:\nYou will decide which {MEME_TYPE} that best suits the user's joke. \nAll jokes have a valid meme type. \nYou will replace {TOP_TEXT} with part of the joke. \nYou will replace {BOTTOM_TEXT} with the last half of the joke. \nNeither line of text is to exceed 10 words\n\nThis is a comma separated list of valid MEME_TYPE's to pick from. \nOprah-You-Get-A-Car-Everybody-Gets-A-Car,Fat-Cat,Dr-Evil-Laser,Frowning-Nun,Chuck-Norris-Phone,Mozart-Not-Sure,Who-Killed-Hannibal,Youre-Too-Slow-Sonic,Conspiracy-Keanu,Blank-Yellow-Sign,Smiling-Jesus,Patrick-Says,Deadpool-Surprised,Imagination-Spongebob,Hey-Internet,How-Tough-Are-You,Misunderstood-Mitch,Crazy-Hispanic-Man,Kool-Kid-Klan,Confused-Gandalf,Confused-Mel-Gibson,Jammin-Baby,Angry-Baby,Aaaaand-Its-Gone,Storytelling-Grandpa,Surpised-Frodo,Blank-Comic-Panel-1x2,And-everybody-loses-their-minds,Derp,Evil-Baby,Grumpy-Cat-Birthday,Why-Is-The-Rum-Gone,Interupting-Kanye,Sexy-Railroad-Spiderman,Hipster-Kitty,Put-It-Somewhere-Else-Patrick,Finding-Neverland,Billy-Graham-Mitt-Romney,Aunt-Carol,Warning-Sign,The-Rock-Driving,Marvel-Civil-War-2,Scumbag-Daylight-Savings-Time,Our-Glorious-Leader-Nicolas-Cage,Chihuahua-dog,Drake-Hotline-Bling,Hot-Caleb,Sexual-Deviant-Walrus,Marked-Safe-From,Mr-Black-Knows-Everything,Overly-Attached-Nicolas-Cage,Wrong-Number-Rita,We-Will-Rebuild,Idiot-Nerd-Girl,CASHWAG-Crew,Mr-Mackey,Laughing-Villains,Lethal-Weapon-Danny-Glover,Jon-Stewart-Skeptical,Smilin-Biden,Tech-Impaired-Duck,Booty-Warrior,Brian-Burke-On-The-Phone,Batman-Smiles,Angry-Dumbledore,Giovanni-Vernia,Trailer-Park-Boys-Bubbles,Socially-Awesome-Awkward-Penguin,So-God-Made-A-Farmer,Advice-God,True-Story,Marvel-Civil-War,I-Should-Buy-A-Boat-Cat,Larry-The-Cable-Guy,Obama-No-Listen,The-Most-Interesting-Justin-Bieber,Super-Kami-Guru-Allows-This,And-then-I-said-Obama,Uncle-Sam,That-Would-Be-Great,Darti-Boy,Grumpy-Cat-Happy,Joe-Biden,Laundry-Viking,SonTung,High-Dog,Perturbed-Portman,Thumbs-Up-Emoji,Jack-Nicholson-The-Shining-Snow,Angry-Asian,Art-Attack,Samuel-L-Jackson,UNO-Draw-25-Cards,Solemn-Lumberjack,Efrain-Juarez,X,-X-Everywhere,Dwight-Schrute,Batman-Slapping-Robin,You-Dont-Say,Surprised-Pikachu,Batman-And-Superman,Dexter,Hello-Kassem,Mother-Of-God,Team-Rocket,Grumpy-Cat-Not-Amused,Engineering-Professor,Nice-Guy-Loki,Hillary-Clinton,Pope-Nicolas-Cage,Stoner-Dog,Minor-Mistake-Marvin,Fabulous-Frank-And-His-Snake,Oprah-You-Get-A,1950s-Middle-Finger,Smug-Bear,Surprised-Koala,Facepalm-Bear,Finn-The-Human,Felix-Baumgartner-Lulz,X-X-Everywhere,College-Freshman,Sexually-Oblivious-Girlfriend,Koala,Hawkward,Brian-Williams-Was-There,Meme-Dad-Creature,These-Arent-The-Droids-You-Were-Looking-For,Bad-Luck-Bear,Deadpool-Pick-Up-Lines,Subtle-Pickup-Liner,First-World-Problems,Crazy-Girlfriend-Praying-Mantis,Fk-Yeah,Butthurt-Dweller,Self-Loathing-Otter,Hal-Jordan,Member-Berries,Obama-Romney-Pointing,Perfection-Michael-Fassbender,Ancient-Aliens,Hypocritical-Islam-Terrorist,Apathetic-Xbox-Laser,Tom-Hardy-,Legal-Bill-Murray,Liam-Neeson-Taken-2,Rich-Guy-Dont-Care,Unsettled-Tom,Doug,Insanity-Wolf,Creeper-Dog,I-Am-Not-A-Gator-Im-A-X,Advice-Peeta,Overly-Manly-Man,Squidward,Captain-Picard-Facepalm,Keep-Calm-And-Carry-On-Aqua,Rick,Depression-Dog,Think,Tennis-Defeat,Always-Has-Been,Multi-Doge,First-Day-On-The-Internet-Kid,Creepy-Condescending-Wonka,Blank-Blue-Background,Beyonce-Knowles-Superbowl-Face,Happy-Minaj-2,Feels-Bad-Frog---Feels-Bad-Man,Men-In-Black,Dafuq-Did-I-Just-Read,The-Critic,Keep-Calm-And-Carry-On-Purple,Jersey-Santa,He-Will-Never-Get-A-Girlfriend,TSA-Douche,Grus-Plan,Contradictory-Chris,Rick-and-Carl,Kim-Jong-Il-Y-U-No,Guy-Holding-Cardboard-Sign,College-Liberal,Paranoid-Parrot,Blank-Starter-Pack,Cute-Puppies,Nabilah-Jkt48,But-Thats-None-Of-My-Business-Neutral,Gasp-Rage-Face,Google-Chrome,Thats-Just-Something-X-Say,Face-You-Make-Robert-Downey-Jr,Officer-Cartman,Overly-Attached-Girlfriend,Pickup-Master,Musically-Oblivious-8th-Grader,Modern-Warfare-3,Pink-Escalade,Grumpy-Cat-Table,Evil-Kermit,Obama-Cowboy-Hat,Mr-T,Chester-The-Cat,Alien-Meeting-Suggestion,Homophobic-Seal,Serious-Xzibit,Snape,Picard-Wtf,Pelosi,Excited-Cat,Spiderman-Laugh,Portuguese,Rebecca-Black,Monkey-Business,Larfleeze,The-Rock-It-Doesnt-Matter,Intelligent-Dog,Britney-Spears,Dad-Joke-Dog,Arrogant-Rich-Man,Chemistry-Cat,Woman-Yelling-At-Cat,Cereal-Guys-Daddy,Little-Romney,Fear-And-Loathing-Cat,Art-Student-Owl,Putin,Talk-To-Spongebob,Mocking-Spongebob,Samuel-Jackson-Glance,Dat-Ass,Eye-Of-Sauron,Unicorn-MAN,Its-True-All-of-It-Han-Solo,Bill-OReilly,Internet-Guide,Harmless-Scout-Leader,Rick-Grimes,Baby-Cry,Predator,Romney,Jerkoff-Javert,Mega-Rage-Face,Actual-Advice-Mallard,Ron-Swanson,Burn-Kitty,Sudden-Disgust-Danny,Scrooge-McDuck-2,If-You-Know-What-I-Mean-Bean,Super-Cool-Ski-Instructor,Welcome-To-The-Internets,Annoying-Facebook-Girl,Bubba-And-Barack,Herm-Edwards,Romneys-Hindenberg,Impossibru-Guy-Original,Relaxed-Office-Guy,Determined-Guy-Rage-Face,Small-Face-Romney,Alyssa-Silent-Hill,Do-I-Care-Doe,Big-Bird-And-Snuffy,Aint-Nobody-Got-Time-For-That,Baron-Creater,Are-Your-Parents-Brother-And-Sister,Inception,Tuxedo-Winnie-The-Pooh,Dont-You-Squidward,Back-In-My-Day,Secure-Parking,Challenge-Accepted-Rage-Face,Brian-Griffin,Murica,I-Know-That-Feel-Bro,Men-Laughing,Ghost-Nappa,Simsimi,Beyonce-Knowles-Superbowl,Internet-Explorer,Yao-Ming,Costanza,Felix-Baumgartner,Harper-WEF,Zuckerberg,Matanza,Anti-Joke-Chicken,Simba-Shadowy-Place,Beyonce-Superbowl-Yell,Pillow-Pet,Surprised-Coala,Fast-Furious-Johnny-Tran,Happy-Minaj,Michael-Phelps-Death-Stare,American-Chopper-Argument,Scumbag-Job-Market,Rick-and-Carl-3,Really-Evil-College-Teacher,WTF,Ridiculously-Photogenic-Judge,Okay-Truck,Good-Fellas-Hilarious";
    //var requestBody = "{\"prompt\": \"" + memeRules + "\",\"max_tokens\": 150,\"temperature\": 0.7,\"n\": 1,\"stop\": \"\\n\"}";

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
// @"Meme key - you can use any of them:
//Use pigeon meme key for memes that you don't know. Put 'Is this a <some wild guess on second text>' as the first text and put the prompt on second text.
// aag
// afraid
// agnes
// ams
// bilbo
// disastergirl
// drake
// drowning
// ds
// dwight
// exit
// feelsgood
// fine
// firsttry
// gandalf
// grave
// grumpycat
// harold
// headaches
// joker
// kombucha
// leo
// money
// mouth
// mw
// nails
// nice
// noah
// noidea
// ntot
// oag
// officespace
// older
// oprah
// panik-kalm-panik
// patrick
// perfection
// persian
// philosoraptor
// pigeon
// pooh
// ptj
// puffin
// red
// regret
// remembers
// reveal
// right
// rollsafe
// sad-biden
// sad-boehner
// sad-bush
// sad-clinton
// sad-obama
// sadfrog
// saltbae
// same
// sarcasticbear
// sb
// scc
// seagull
// sf
// sk
// ski
// slap
// snek
// soa
// sohappy
// sohot
// soup-nazi
// sparta
// spiderman
// spongebob
// ss
// stew
// stonks
// stop
// stop-it
// success
// tenguy
// toohigh
// touch
// tried
// trump
// ugandanknuck
// vince
// waygd
// wddth
// whatyear
// winter
// wkh
// woman-cat
// wonka
// worst
// xy
// yallgot
// yodawg
// yuno
// zero-wing";

    var messages = new [] { new {role = "system", content = memeRules } };
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

async Task HandleJoelFlavoringFollowUps(SocketSlashCommand command)
{
    await command.RespondAsync("Ah. Eh..");
    await Task.Delay(3000);
    // await command.FollowupAsync(".");
    // await Task.Delay(1000);
    // await command.FollowupAsync("..");
    // await Task.Delay(1000);
    // await command.FollowupAsync("...");
    // await Task.Delay(1000);
}
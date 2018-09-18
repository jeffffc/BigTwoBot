using BigTwoBot;
using BigTwoBot.Models;
using BigTwoBot.Models.Information;
using BigTwoBot.Models.Requests;
using ConsoleTables;
using NamedPipeWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotApi;
using TelegramBotApi.Types;

namespace BigTwoBotNode
{
    class Program
    {
        // console window
        static List<string> upperArea = new List<string>();
        static List<string> lowerArea = new List<string>();
        static int areaHeights = 0;

        internal static Locale English;
        public static Dictionary<string, Locale> Langs;

        public static DateTime Startup;
        public static string Id { get; } = Guid.NewGuid().ToString("N");
        public static List<BigTwo> Games = new List<BigTwo>();
        public static bool ShuttingDown = false;

        public static NamedPipeClient<ControlNodeMessage> NodeClient = new NamedPipeClient<ControlNodeMessage>("BigTwoControl");

        static void Main(string[] args)
        {
            Startup = DateTime.Now;
            Bot.Api = new TelegramBot(Constants.GetBotToken("BotToken"));
            Bot.Me = Bot.Api.GetMeAsync().Result;

            English = Helpers.ReadEnglish();
            Langs = Helpers.ReadLanguageFiles();

            Console.Title = $"BigTwoBot [NODE] - {{{Id}}} - Connected to {Bot.Me.FirstName} (@{Bot.Me.Username} | {Bot.Me.Id})";
            Console.WindowHeight += 20;
            areaHeights = (Console.WindowHeight - 2) / 2;


            NodeClient.Disconnected += (sender) => Environment.Exit(0);
            NodeClient.Error += (e) => Console.WriteLine(e);
            NodeClient.ServerMessage += (sender, message) =>
            {
                if (message.Request.Type == RequestType.Ping) NodeClient.PushMessage(new ControlNodeMessage(new RefreshInfo(), Id));
                else if (message.NodeId == Id) ReceivedRequest(message.Request);
            };
            new Thread(UpdateConsole).Start();

            WriteToLower("Starting pipe client...");
            NodeClient.Start();
            WriteToLower("Waiting for connection with control...");
            NodeClient.WaitForConnection();
            WriteToLower("Connected!");
            // NodeClient.PushMessage(new ControlNodeMessage(new RefreshInfo(), Id));

            while (true)
            {
                if (ShuttingDown && Games.Count == 0)
                {
                    Environment.Exit(0);
                }
                Thread.Sleep(1000);
            }

        }

        static void ReceivedRequest(IRequest request)
        {
            RequestType type = request.Type;
            Message message = request.Message != null ? JsonConvert.DeserializeObject<Message>(request.Message) : null;
            User user = request.User != null ? JsonConvert.DeserializeObject<User>(request.User) : null;
            CallbackQuery query = request.Query != null ? JsonConvert.DeserializeObject<CallbackQuery>(request.Query) : null;
            /*
            Console.ForegroundColor = ConsoleColor.Green;
            if (Type == "ChoiceRequest") Console.WriteLine($"IN:  ChoiceRequest - {req.Choice}");
            else Console.WriteLine($"IN:  {Type}");
            */

            BigTwo g;
            switch (type)
            {
                case RequestType.StartGame:
                    if (!ShuttingDown)
                    {
                        g = new BigTwo(message.Chat.Id, user, message.Chat.Title, message.Chat.Username);
                        Games.Add(g);
                    }
                    break;
                case RequestType.JoinGame:
                    g = Games.FirstOrDefault(x => x.ChatId == message.Chat.Id);
                    g?.AddPlayer(user);
                    break;
                case RequestType.FleeGame:
                    g = Games.FirstOrDefault(x => x.ChatId == message.Chat.Id);
                    g?.RemovePlayer(user);
                    break;
                case RequestType.Choice:
                    var r = (ChoiceRequest)request;
                    g = Games.FirstOrDefault(x => x.Id == r.GameId);
                    var u = JsonConvert.DeserializeObject<User>(r.User);
                    var q = JsonConvert.DeserializeObject<CallbackQuery>(r.Query);
                    var p = g?.Players.FirstOrDefault(x => x.TelegramId == u.Id);
                    if (p != null)
                    {
                        g.HandleQuery(q, r.Args);
                    }
                    break;
                case RequestType.Extend:
                    g = Games.FirstOrDefault(x => x.ChatId == message.Chat.Id);
                    g?.ExtendTimer(message);
                    break;
                case RequestType.ForceStart:
                    g = Games.FirstOrDefault(x => x.ChatId == message.Chat.Id);
                    g?.ForceStart();
                    break;
                case RequestType.KillGame:
                    g = Games.FirstOrDefault(x => x.ChatId == message.Chat.Id);
                    g?.KillGame();
                    break;
                case RequestType.ChangeStatus:
                    var req = (ChangeStatusRequest)request;
                    if (req.ShutDown != null) ShuttingDown = req.ShutDown ?? false;
                    if (message != null) Bot.Edit(message.Chat.Id, message.MessageId, $"{message}\n\nNode {Id} stopped accepting games and will shut down.");
                    SendInfo(new StatusChangedInfo(ShuttingDown));
                    if (ShuttingDown && Games.Count == 0) Environment.Exit(0);
                    break;
                default:
                    break;
            }
        }

        public static void SendInfo(IInfo info)
        {
            NodeClient.PushMessage(new ControlNodeMessage(info, Id));
            WriteToLower($"OUT: {info.Type}");
            Thread.Sleep(1000);
        }

        // console window
        private static void DrawScreen()
        {
            Console.Clear();

            // Draw the area divider
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, areaHeights);
                Console.Write('=');
            }

            int currentLine = areaHeights - 1;

            for (int i = 0; i < upperArea.Count; i++)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(upperArea[i]);
            }

            currentLine = (areaHeights * 2);
            for (int i = 0; i < lowerArea.Count; i++)
            {
                Console.SetCursorPosition(0, currentLine - (i + 1));
                Console.WriteLine(lowerArea[i]);
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write("> ");
        }

        private static void UpdateConsole()
        {
            while (true)
            {
                var Uptime = DateTime.Now - Startup;
                string msg = $"Startup Time: {Startup.ToString()}";
                msg += Environment.NewLine + $"Uptime: {Uptime.ToString()}";
                var games = Program.Games;
                int gameCount = games.Count();

                msg += Environment.NewLine + $"Number of Games: {gameCount.ToString()}";

                var table = new ConsoleTable("Game GUID", "ChatId", "Phase", "# of Players");
                foreach (BigTwo game in games)
                {
                    table.AddRow(game.Id.ToString(), game.ChatId.ToString(), game.Phase.ToString(), game.Players.Count().ToString());
                }
                msg += Environment.NewLine + Environment.NewLine + table.ToStringAlternative();

                WriteToUpper(msg);

                DrawScreen();
                Thread.Sleep(1000);
            }
        }

        private static void WriteToUpper(string line)
        {
            upperArea.Clear();
            upperArea.Add(line);
        }

        private static void WriteToLower(string line)
        {
            lowerArea.Insert(0, line);

            if (lowerArea.Count == areaHeights)
            {
                lowerArea.RemoveAt(areaHeights - 1);
            }
        }

    }
}

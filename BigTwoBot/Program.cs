using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi;
using System.Runtime.Caching;
using BigTwoBot.Handlers;
using BigTwoBot.Models;
using BigTwoBot.Models.Requests;
using System.Threading;
using ConsoleTables;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using NamedPipeWrapper;
using System.Diagnostics;
using BigTwoBot.Models.Information;

namespace BigTwoBot
{
    class Program
    {
        internal static Locale English;
        public static Dictionary<string, Locale> Langs;
        public static readonly MemoryCache AdminCache = new MemoryCache("GroupAdmins");
        public static bool MaintMode = false;
        public static DateTime Startup;

        public static NamedPipeServer<ControlNodeMessage> Control = new NamedPipeServer<ControlNodeMessage>("BigTwoControl");
        public static bool IsRefreshing = false;
        public static List<string> Refreshing = new List<string>();
        public static List<Node> Nodes = new List<Node>();
        public static bool UpdatingControl = false;


        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            // Load card Images
            /*
            for (int i = 1; i <= 104; i++)
                Constants.cardImages.Add(Image.FromFile(Path.Combine(Constants._imagePath, $"{i}.png")));
            */

            Bot.Api = new TelegramBot(Constants.GetBotToken("BotToken"));
            Bot.Me = Bot.Api.GetMeAsync().Result;

            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            Bot.Send(Constants.LogGroupId, $"Bot started! Version: {version.ToString()}");

            Console.Title = $"BigTwoBot - Connected to {Bot.Me.FirstName} (@{Bot.Me.Username} | {Bot.Me.Id}) - Version {version.ToString()}";

            foreach (var m in typeof(Commands).GetMethods())
            {
                foreach (var a in m.GetCustomAttributes(true))
                {
                    if (a is Attributes.Command cmd)
                    {
                        var method = m.CreateDelegate(typeof(Bot.CommandMethod)) as Bot.CommandMethod;
                        Bot.Commands.Add(new Models.Command(cmd.Trigger, cmd.AdminOnly, cmd.DevOnly, cmd.GroupOnly, method));
                    }
                }
            }

            foreach (var m in typeof(Callbacks).GetMethods())
            {
                foreach (var a in m.GetCustomAttributes(true))
                {
                    if (a is Attributes.Callback cb)
                    {
                        var method = m.CreateDelegate(typeof(Bot.CallbackMethod)) as Bot.CallbackMethod;
                        Bot.Callbacks.Add(new Models.Callback(cb.Trigger, cb.AdminOnly, cb.DevOnly, method));
                    }
                }
            }

            // control named pipe server
            Control.ClientDisconnected += (sender) => RefreshNodes();
            Control.ClientConnected += (sender) => RefreshNodes();
            Control.Error += (e) => Console.WriteLine(e);
            Control.ClientMessage += (sender, message) =>
            {
                try
                {
                    var info = message.Info;
                    var nodeId = message.NodeId;
                    if (info.Type == InfoType.Refresh) Refreshing.Add(nodeId);
                    else
                    {
                        var node = Nodes.FirstOrDefault(x => x.Id == nodeId);
                        if (node != null) node.ReceivedInfo(info);
                        else throw new Exception("Can't understand message from node");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
            Control.Start();
#if RELEASE
            NewNode();
#endif


            English = Helpers.ReadEnglish();
            Langs = Helpers.ReadLanguageFiles();

            var offset = Bot.Api.GetUpdates(offset: -1);
            if (offset.Any()) Bot.Api.GetUpdates(offset: offset.FirstOrDefault().Id);
            Handler.HandleUpdates(Bot.Api);
            Bot.Api.StartReceiving();
            Startup = DateTime.Now;
            new Thread(UpdateConsole).Start();
            Console.ReadLine();
            Bot.Api.StopReceiving();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            string message = Environment.NewLine + Environment.NewLine + exc.Message + Environment.NewLine + Environment.NewLine;
            string trace = exc.StackTrace;

            do
            {
                exc = exc.InnerException;
                if (exc == null) break;
                message += exc.Message + Environment.NewLine + Environment.NewLine;
            }
            while (true);

            message += trace;
            Bot.Send(Constants.LogGroupId, "<b>UNHANDELED EXCEPTION! BOT IS PROBABLY CRASHING!</b>" + message.FormatHTML());
            Thread.Sleep(5000); // Give the message time to be sent
        }

        private static void UpdateConsole()
        {
            Console.Clear();
            Console.WindowWidth += 25;
            DateTime LastErrorNotify = DateTime.MinValue;

            while (true)
            {
                /*
                Console.Clear();
                var Uptime = DateTime.Now - Startup;
                string msg = $"Startup Time: {Startup.ToString()}";
                msg += Environment.NewLine + $"Uptime: {Uptime.ToString()}";
                var games = Program.Games;
                int gameCount = games.Count();

                msg += Environment.NewLine + $"Number of Games: {gameCount.ToString()}";
                Console.WriteLine(msg);

                var table = new ConsoleTable("Game GUID", "ChatId", "Phase", "# of Players");
                foreach (BigTwo game in games)
                {
                    table.AddRow(game.Id.ToString(), game.ChatId.ToString(), game.Phase.ToString(), game.Players.Count().ToString());
                }
                table.Write(Format.Alternative);

                Thread.Sleep(2000);
                */
                try
                {
                    var table = new ConsoleTable("Node ID", "Started at", "GameNum", "PlayerNum", "ShuttingDown");

                    foreach (var n in Nodes)
                    {
                        table.AddRow(n.Id, n.StartTime.ToShortDateString() + " " + n.StartTime.ToShortTimeString(), n.Games.Count, n.Games.Sum(x => x.Players.Count), n.ShuttingDown);
                    }

                    Console.Clear();
                    Console.WriteLine();
                    if (MaintMode)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("     MAINTENANCE MODE ENABLED! NO GAMES CAN BE STARTED!");

                        if (UpdatingControl)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("    BOT IS ABOUT TO UPDATE AS SOON AS ALL GAMES STOPPED!");
                        }

                        Console.WriteLine();
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.WriteLine($"Started at: {Startup.ToShortDateString()} {Startup.ToLongTimeString()}");
                    Console.WriteLine($"Uptime: {(DateTime.Now - Startup).ToString("dd\\.hh\\:mm\\:ss")}");
                    Console.WriteLine();

                    table.Write(Format.Alternative);
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    if (DateTime.Now.AddMinutes(-30) > LastErrorNotify)
                    {
                        e.LogError();
                        LastErrorNotify = DateTime.UtcNow;
                    }
                }

            }
        }

        public static void NewNode()
        {
#if RELEASE
            var latest = Directory.GetDirectories(Path.Combine(Constants.GetBasePath(), Constants.NodesFolder)).OrderByDescending(x => x).First();
            Process.Start(Path.Combine(latest, "BigTwoBotNode.exe"));
#endif
        }

        static void RefreshNodes()
        {
            if (IsRefreshing) return;
            IsRefreshing = true;
            Refreshing.Clear();
            Control.PushMessage(new ControlNodeMessage(new PingRequest()));
            Thread.Sleep(2000);
            string[] refreshing = new string[20];
            Refreshing.CopyTo(refreshing);
            List<string> toRemove = new List<string>();

            foreach (var node in Nodes.Where(x => !refreshing.Contains(x.Id))) toRemove.Add(node.Id);
            Nodes.RemoveAll(x => toRemove.Contains(x.Id));
            foreach (var node in refreshing.Where(x => x != null && !Nodes.Any(y => y.Id == x))) Nodes.Add(new Node(node));
            Refreshing.Clear();

            if (!UpdatingControl && !Nodes.Any(x => !x.ShuttingDown && x.Games.Count < Constants.NodeMaxGames))
            {
#if RELEASE
                NewNode();
#endif
            }
            else
            {
                var all = Nodes.Where(x => x.Games.Count == 0 && !x.ShuttingDown);
                if (all.Count() > 1)
                {
                    var nodes = all.Take(all.Count() - 1);
                    foreach (var n in nodes)
                        n?.SendRequest(new ChangeStatusRequest(shutDown: true, nodeId: n.Id));
                }
            }
            IsRefreshing = false;
        }

    }
}

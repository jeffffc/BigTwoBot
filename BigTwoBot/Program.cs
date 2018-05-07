﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Runtime.Caching;
using BigTwoBot.Handlers;
using BigTwoBot.Models;
using System.Threading;
using ConsoleTables;
using System.Xml.Linq;
using System.IO;
using System.Drawing;

namespace BigTwoBot
{
    class Program
    {
        internal static Locale English;
        public static Dictionary<string, Locale> Langs;
        public static readonly MemoryCache AdminCache = new MemoryCache("GroupAdmins");
        public static bool MaintMode = false;
        public static DateTime Startup;

        private static List<BigTwo> _Games = new List<BigTwo>();
        public static List<BigTwo> Games { get { return _Games; } set { _Games = Games; } }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            // Load card Images
            /*
            for (int i = 1; i <= 104; i++)
                Constants.cardImages.Add(Image.FromFile(Path.Combine(Constants._imagePath, $"{i}.png")));
            */

            Bot.Api = new TelegramBotClient(Constants.GetBotToken("BotToken"));
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

            English = Helpers.ReadEnglish();
            Langs = Helpers.ReadLanguageFiles();

            Bot.Api.GetUpdatesAsync(-1).Wait();
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
            while (true)
            {
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
            }
        }
    }
}

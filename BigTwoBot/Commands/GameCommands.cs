using Database;
using BigTwoBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;
using TelegramBotApi.Enums;
using static BigTwoBot.Helpers;
using BigTwoBot.Models.Requests;

namespace BigTwoBot
{
    public partial class Commands
    {
        [Command(Trigger = "startgame", GroupOnly = true)]
        public static void StartGame(Message msg, string[] args)
        {
            
            if (Program.MaintMode)
            {
                Bot.Send(msg.Chat.Id, GetTranslation("CantStartGameMaintenance", GetLanguage(msg.Chat.Id)));
                return;
            }

            // find a node that contains an existing game
            var node = Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id));
            if (node == null)
            {
                node = Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.Players.Contains(msg.From.Id)));
                if (node == null)
                {
                    node = Program.Nodes.Where(x => !x.ShuttingDown).OrderBy(x => x.Games.Count).FirstOrDefault();
                    if (node == null)
                    {
                        Bot.Send(msg.Chat.Id, "There are no nodes available!");
                        return;
                    }
                    // Bot.Api.SendChatActionAsync(msg.From.Id, ChatAction.Typing).Wait();
                    try
                    {
                        var t = Bot.Api.SendTextMessage(msg.From.Id, "test", disableNotification: true);
                        Bot.Api.DeleteMessage(msg.From.Id, t.MessageId);
                    }
                    catch (Exception)
                    {
                        // no he did not
                        Bot.Send(msg.Chat.Id, GetTranslation("NotStartedBot", GetLanguage(msg.From.Id), msg.From.GetName()), GenerateStartMe(msg.From.Id));
                        return;
                    }

                    node.SendRequest(new StartGameRequest(msg.From, msg, node.Id));
                }

            }
        }

        [Command(Trigger = "join", GroupOnly = true)]
        public static void JoinGame(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new JoinGameRequest(msg.From, msg));
        }

        [Command(Trigger = "flee", GroupOnly = true)]
        public static void FleeGame(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new FleeGameRequest(msg.From, msg));
        }

        [Command(Trigger = "forcestart", GroupOnly = true, AdminOnly = true)]
        public static void ForceStart(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new ForceStartRequest(msg.From, msg));
        }

        [Command(Trigger = "killgame", GroupOnly = true, DevOnly = true)]
        public static void KillGame(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new KillGameRequest(msg.From, msg));
        }

        [Command(Trigger = "nextgame", GroupOnly = true)]
        public static void NextGame(Message msg, string[] args)
        {
            if (msg.Chat.Type == ChatType.Private)
                return;
            var grpId = msg.Chat.Id;
            using (var db = new BigTwoDb())
            {
                var dbGrp = db.Groups.FirstOrDefault(x => x.GroupId == grpId);
                if (dbGrp != null)
                {
                    var notified = db.NotifyGames.FirstOrDefault(x => x.GroupId == grpId && x.UserId == msg.From.Id);
                    if (notified != null)
                    {
                        Bot.Send(msg.From.Id, GetTranslation("AlreadyInWaitingList", GetLanguage(msg.From.Id)));
                        return;
                    }
                    db.Database.ExecuteSqlCommand($"INSERT INTO NotifyGame VALUES ({msg.From.Id}, {msg.Chat.Id})");
                    db.SaveChanges();
                    Bot.Send(msg.From.Id, GetTranslation("NextGame", GetLanguage(msg.From.Id)));
                }
            }
        }

        [Command(Trigger = "extend", GroupOnly = true)]
        public static void ExtendTimer(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new ExtendRequest(msg.From, msg));
        }

        [Command(Trigger = "showdeck", GroupOnly = true)]
        public static void ShowDeck(Message msg, string[] args)
        {
            Program.Nodes.FirstOrDefault(x => x.Games.Any(y => y.GroupId == msg.Chat.Id))?.SendRequest(new ExtendRequest(msg.From, msg));
        }
    }
}

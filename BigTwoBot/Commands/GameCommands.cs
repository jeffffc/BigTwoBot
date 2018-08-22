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

namespace BigTwoBot
{
    public partial class Commands
    {
        [Command(Trigger = "startgame")]
        public static void StartGame(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                if (Program.MaintMode)
                {
                    Bot.Send(msg.Chat.Id, GetTranslation("CantStartGameMaintenance", GetLanguage(msg.Chat.Id)));
                    return;
                }

                // see if he started bot
                try
                {
                    // Bot.Api.SendChatActionAsync(msg.From.Id, ChatAction.Typing).Wait();
                    var t = Bot.Api.SendTextMessage(msg.From.Id, "test", disableNotification: true);
                    Bot.Api.DeleteMessage(msg.From.Id, t.MessageId);
                    Bot.AddGame(new BigTwo(msg.Chat.Id, msg.From, msg.Chat.Title, msg.Chat.Username));
                }
                catch (Exception e)
                {
                    // no he did not
                    Bot.Send(msg.Chat.Id, GetTranslation("NotStartedBot", GetLanguage(msg.From.Id), msg.From.GetName()), GenerateStartMe(msg.From.Id));
                }
            }
            else
            {
                game.HandleMessage(msg);
                // msg.Reply(GetTranslation("ExistingGame", GetLanguage(msg.Chat.Id)));
            }
        }

        [Command(Trigger = "test")]
        public static void Testing(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "join")]
        public static void JoinGame(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "flee")]
        public static void FleeGame(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "forcestart")]
        public static void ForceStart(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "killgame", DevOnly = true)]
        public static void KillGame(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "nextgame")]
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
                    else
                    {
                    }
                    db.Database.ExecuteSqlCommand($"INSERT INTO NotifyGame VALUES ({msg.From.Id}, {msg.Chat.Id})");
                    db.SaveChanges();
                    Bot.Send(msg.From.Id, GetTranslation("NextGame", GetLanguage(msg.From.Id)));
                }
            }
        }

        [Command(Trigger = "extend")]
        public static void ExtendTimer(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
               game.HandleMessage(msg);
            }
        }

        [Command(Trigger = "showdeck")]
        public static void ShowDeck(Message msg, string[] args)
        {
            BigTwo game = Bot.GetGameByChatId(msg.Chat.Id);
            if (game == null)
            {
                return;
            }
            else
            {
                game.HandleMessage(msg);
            }
        }
    }
}

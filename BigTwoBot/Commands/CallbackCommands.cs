using BigTwoBot.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotApi.Types;
using Database;
using BigTwoBot.Handlers;
using static BigTwoBot.Helpers;

namespace BigTwoBot
{
    public partial class Callbacks
    {
        [Callback(Trigger = "game")]
        public static void GameQuery(CallbackQuery query, string[] args)
        {
            var temp = args[1].Split('|');
            var gameId = temp[0];
            var playerId = temp[1];
            var playerChoice = temp[2];

            var game = Bot.GetGameByGuid(gameId);
            if (game != null)
            {
                game.HandleQuery(query, temp);
            }
            else
            {
                // should not happen
            }
        }

        [Callback(Trigger = "config")]
        public static void ConfigQuery(CallbackQuery query, string[] args)
        {
            var temp = args[1].Split('|');
            var chatId = long.Parse(temp[1]);
            if (temp[0] == "lang")
            {
                if (temp.Length == 2)
                {
                    var menu = Handler.GetConfigLangMenu(chatId);
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, GetTranslation(chatId > 0 ? "ChoosePMLanguage" : "ChooseLanguage", GetLanguage(chatId)), menu);
                }
                if (temp.Length > 2)
                {
                    var chosenLang = temp[2];
                    Handler.SetLanguage(chatId, chosenLang);
                    var menu = Handler.GetConfigMenu(chatId);
                    var toSend = GetTranslation("ReceivedButton", GetLanguage(chatId)) + Environment.NewLine + GetTranslation("WhatToDo", GetLanguage(chatId));
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, toSend, menu);
                }
            }
            else if (temp[0] == "choosetime")
            {
                if (temp.Length == 2)
                {
                    var menu = Handler.GetConfigChooseCardTimeMenu(chatId);
                    // should be group only
                    var group = Helpers.GetGroup(chatId);
                    var current = group.ChooseCardTime ?? Constants.ChooseCardTime;
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId,
                        GetTranslation("ConfigChooseCardTimeDetail", GetLanguage(chatId), current), menu);
                }
                if (temp.Length > 2)
                {
                    var chosen = int.Parse(temp[2]);
                    Handler.SetChooseCardTimeConfig(chatId, chosen);
                    var menu = Handler.GetConfigMenu(chatId);
                    var toSend = GetTranslation("ReceivedButton", GetLanguage(chatId)) + Environment.NewLine + GetTranslation("WhatToDo", GetLanguage(chatId));
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, toSend, menu);

                }
            }
            else if (temp[0] == "playchips")
            {
                if (temp.Length == 2)
                {
                    var menu = Handler.GetConfigPlayChipsMenu(chatId);
                    // should be group only
                    var group = Helpers.GetGroup(chatId);
                    var current = group.PlayChips ?? false;
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId,
                        GetTranslation("ConfigPlayChipsDetail", GetLanguage(chatId), current == true ? GetTranslation("ConfigYes", GetLanguage(chatId)) : GetTranslation("ConfigNo", GetLanguage(chatId))), menu);
                }
                if (temp.Length > 2)
                {
                    var chosen = temp[2] == "yes";
                    Handler.SetPlayChipsConfig(chatId, chosen);
                    var menu = Handler.GetConfigMenu(chatId);
                    var toSend = "";
                    if (!chosen)
                        toSend = GetTranslation("ReceivedButton", GetLanguage(chatId)) + Environment.NewLine + GetTranslation("WhatToDo", GetLanguage(chatId));
                    else
                    {
                        var group = Helpers.GetGroup(chatId);
                        var current = group.ChipsPerCard ?? Constants.ChipsPerCard;
                        toSend = GetTranslation("ConfigChipsAmountDetail", GetLanguage(chatId), current);
                        menu = Handler.GetChipsAmountMenu(chatId);
                    }
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, toSend, menu);

                }
            }
            else if (temp[0] == "chips")
            {
                if (temp.Length > 2)
                {
                    var chosen = int.Parse(temp[2]);
                    Handler.SetChipsAmountConfig(chatId, chosen);
                    var menu = Handler.GetConfigMenu(chatId);
                    var toSend = GetTranslation("ReceivedButton", GetLanguage(chatId)) + Environment.NewLine + GetTranslation("WhatToDo", GetLanguage(chatId));
                    Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, toSend, menu);

                }
            }
            else if (temp[0] == "done")
            {
                Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, GetTranslation("ConfigDone", GetLanguage(chatId)));
            }
            else if (temp[0] == "back")
            {
                Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, GetTranslation("WhatToDo", Handler.GetLanguage(chatId)), Handler.GetConfigMenu(chatId));
            }
            return;
        }
    }
}

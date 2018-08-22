using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Database;
using TelegramBotApi.Types;
using TelegramBotApi.Types.Markup;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace BigTwoBot.Handlers
{
    partial class Handler
    {
        public static void HandleQuery(CallbackQuery call)
        {
            if (call.Data != null)
            {
                var args = call.Data.Contains('|')
                ? new[] { call.Data.Split('|')[0], call.Data.Remove(0, call.Data.IndexOf('|') + 1) }
                : new[] { call.Data, null };

                var callback = Bot.Callbacks.FirstOrDefault(x => x.Trigger == args[0]);
                if (callback == null)
                {
                    // maybe it is from game
                    var g = Bot.GetGameByGuid(args[0]);
                    if (g != null)
                    {
                        var temp = $"game|{call.Data}";
                        args = new[] { temp.Split('|')[0], temp.Remove(0, temp.IndexOf('|') + 1) };
                        callback = Bot.Callbacks.FirstOrDefault(x => x.Trigger == args[0]);
                    }
                    if (callback == null)
                    {
                        BotMethods.AnswerCallback(call);
                        BotMethods.Edit("An error occured: No callback method found! The developer was informed!", call.Message);
                        BotMethods.Send(Constants.LogGroupId, $"Error occured! No callback method \"{args[0]}\" found! at CallbackHandler.cs, OnCallback()");
                        return;
                    }
                }

                if (callback.AdminOnly && !Helpers.IsGroupAdmin(call))
                {
                    BotMethods.AnswerCallback(call,  "You are not a group admin!", true);
                    return;
                }

                if (callback.DevOnly && !Constants.Dev.Contains(call.From.Id))
                {
                    call.Message.Reply("You aren't a bot dev!");
                    return;
                }

                callback.Method.Invoke(call, args);

                
            }
            else
            {
                //
            }
        }

        public static InlineKeyboardMarkup GetConfigMenu(long id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton> { };
            //base menu
            buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigChangeLanguage", GetLanguage(id))) { CallbackData = $"config|lang|{id}" });
            // for group only
            if (id < 0)
            {
                buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigChooseCardTime", GetLanguage(id))) { CallbackData = $"config|choosetime|{id}" });
                buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigPlayChips", GetLanguage(id))) { CallbackData = $"config|playchips|{id}" });
            }
            buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigDone", GetLanguage(id))) { CallbackData = $"config|done|{id}" });
            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                twoMenu.Add(new[] { buttons[i] });
            }

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }

        public static InlineKeyboardMarkup GetConfigLangMenu(long id, bool setlang = false)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            //base menu
            foreach (var lang in Program.Langs.Values)
                buttons.Add(new InlineKeyboardButton(lang.LanguageName) { CallbackData = !setlang ? $"config|lang|{id}|{lang.Language}" : $"setlang|lang|{id}|{lang.Language}" });
            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons.Count - 1 == i)
                {
                    twoMenu.Add(new[] { buttons[i] });
                }
                else
                    twoMenu.Add(new[] { buttons[i], buttons[i + 1] });
                i++;
            }
            if (!setlang)
                twoMenu.Add(new[] { new InlineKeyboardButton(GetTranslation("ConfigBack", GetLanguage(id))) { CallbackData = $"config|back|{id}" } });

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }

        public static InlineKeyboardMarkup GetGetLangMenu()
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            //base menu
            foreach (var lang in Program.Langs.Values)
                buttons.Add(new InlineKeyboardButton(lang.LanguageName) { CallbackData = $"getlang|get|{lang.Language}" });
            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons.Count - 1 == i)
                {
                    twoMenu.Add(new[] { buttons[i] });
                }
                else
                    twoMenu.Add(new[] { buttons[i], buttons[i + 1] });
                i++;
            }
            twoMenu.Add(new[] { new InlineKeyboardButton("Cancel") { CallbackData = $"getlang|cancel" } });

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }

        public static InlineKeyboardMarkup GetConfigChooseCardTimeMenu(long id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton> { };
            var times = new string[] { "30", "45", "60", "75", "90" };
            //base menu
            foreach (var t in times)
                buttons.Add(new InlineKeyboardButton(t) { CallbackData = $"config|choosetime|{id}|{t}" });

            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons.Count - 1 == i)
                {
                    twoMenu.Add(new[] { buttons[i] });
                }
                else
                    twoMenu.Add(new[] { buttons[i], buttons[i + 1] });
                i++;
            }
            twoMenu.Add(new[] { new InlineKeyboardButton(GetTranslation("ConfigBack", GetLanguage(id))) { CallbackData = $"config|back|{id}" } });

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }

        public static InlineKeyboardMarkup GetChipsAmountMenu(long id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton> { };
            var amount= new string[] { "5", "10", "20", "50", "100" };
            //base menu
            foreach (var t in amount)
                buttons.Add(new InlineKeyboardButton(t) { CallbackData = $"config|chips|{id}|{t}" });

            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons.Count - 1 == i)
                {
                    twoMenu.Add(new[] { buttons[i] });
                }
                else
                    twoMenu.Add(new[] { buttons[i], buttons[i + 1] });
                i++;
            }
            twoMenu.Add(new[] { new InlineKeyboardButton(GetTranslation("ConfigBack", GetLanguage(id))) { CallbackData = $"config|back|{id}" } });

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }

        public static InlineKeyboardMarkup GetConfigPlayChipsMenu(long id)
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton> { };
            //base menu
            buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigYes", GetLanguage(id))) { CallbackData = $"config|playchips|{id}|yes" });
            buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigNo", GetLanguage(id))) { CallbackData = $"config|playchips|{id}|no" });
            buttons.Add(new InlineKeyboardButton(GetTranslation("ConfigBack", GetLanguage(id))) { CallbackData = $"config|back|{id}" });
            var twoMenu = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < buttons.Count; i++)
            {
                twoMenu.Add(new[] { buttons[i] });
            }

            var menu = new InlineKeyboardMarkup(twoMenu.ToArray());
            return menu;
        }
    }
}

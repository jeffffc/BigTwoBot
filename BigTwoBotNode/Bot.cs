using BigTwoBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi;
using TelegramBotApi.Types;
using TelegramBotApi.Types.Markup;
using TelegramBotApi.Enums;
using TelegramBotApi.Types.Upload;
using TelegramBotApi.Types.Exceptions;
using BigTwoBot;

namespace BigTwoBotNode
{
    public class Bot
    {
        public static TelegramBot Api;
        public static User Me;

        //internal static HashSet<Models.Command> Commands = new HashSet<Models.Command>();
        //internal static HashSet<Models.Callback> Callbacks = new HashSet<Models.Callback>();
        //public delegate void CommandMethod(Message msg, string[] args);
        //public delegate void CallbackMethod(CallbackQuery query, string[] args);


        internal static Message Send(long chatId, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            return BotMethods.Send(chatId, text, replyMarkup, parseMode, disableWebPagePreview, disableNotification);
        }

        internal static Message SendSticker(long chatId, string fileId, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            return BotMethods.SendSticker(chatId, fileId, replyMarkup, disableNotification);
        }

        internal static Message SendSticker(long chatId, SendFile sticker, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            return BotMethods.SendSticker(chatId, sticker, replyMarkup, disableNotification);
        }

        internal static Message Edit(long chatId, int oldMessageId, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return BotMethods.Edit(chatId, oldMessageId, text, replyMarkup, parseMode, disableWebPagePreview, disableNotification);
            }
            catch (Exception ex)
            {
                ex.LogError();
                return null;
            }
        }

        #region Game related
        public static List<BigTwo> Games { get { return Program.Games; } }

        
        public static BigTwo GetGameByChatId(long chatId)
        {
            return Program.Games.FirstOrDefault(x => x.ChatId == chatId);
        }

        public static void AddGame(BigTwo game)
        {
            Program.Games.Add(game);
        }

        public static void RemoveGame(BigTwo game)
        {
            Program.Games.Remove(game);
        }

        public static void RemoveGame(long chatId)
        {
            RemoveGame(GetGameByChatId(chatId));
        }

        #endregion

    }

    public static class BotMethods
    {
        #region Messages
        public static Message Send(long chatId, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            return Bot.Api.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, disableNotification, 0, replyMarkup).Result;

        }

        public static Message Send(this Chat chat, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return Bot.Api.SendTextMessageAsync(chat.Id, text, parseMode, disableWebPagePreview, disableNotification, 0, replyMarkup).Result;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public static Message SendSticker(long chatId, string fileId, ReplyMarkupBase replyMarkup = null, bool disableNotification = false)
        {
            return Bot.Api.SendStickerAsync(chatId, new SendFileId(fileId), disableNotification, 0, replyMarkup).Result;
        }

        public static Message SendSticker(long chatId, SendFile sticker, ReplyMarkupBase replyMarkup = null, bool disableNotification = false)
        {
            return Bot.Api.SendStickerAsync(chatId, sticker, disableNotification, 0, replyMarkup).Result;
        }

        public static Message Reply(this Message m, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return Bot.Api.SendTextMessageAsync(m.Chat.Id, text, parseMode, disableWebPagePreview, disableNotification, m.MessageId, replyMarkup).Result;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public static Message Reply(long chatId, int oldMessageId, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return Bot.Api.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, disableNotification, oldMessageId, replyMarkup).Result;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public static Message ReplyNoQuote(this Message m, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return Bot.Api.SendTextMessageAsync(m.Chat.Id, text, parseMode, disableWebPagePreview, disableNotification, 0, replyMarkup).Result;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public static Message ReplyPM(this Message m, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                var r = Bot.Api.SendTextMessageAsync(m.From.Id, text, parseMode, disableWebPagePreview, disableNotification, 0, replyMarkup).Result;
                if (r == null)
                {
                    return m.Reply(Helpers.GetTranslation("NotStartedBot", Helpers.GetLanguage(m.From.Id)),
                        new ReplyMarkupMaker(ReplyMarkupMaker.ReplyMarkupType.Inline).AddRow().AddCallbackButton(
                            Helpers.GetTranslation("StartMe", Helpers.GetLanguage(m.From.Id)),
                            $"https://t.me/{Bot.Me.Username}", 0));
                }
                
                return m.Reply(Helpers.GetTranslation("SentPM", Helpers.GetLanguage(m.From.Id)));
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public static Message Edit(string text, Message msg, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true)
        {
            return Edit(msg.Chat.Id, msg.MessageId, text, replyMarkup, parseMode, disableWebPagePreview);
        }


        public static Message Edit(long chatId, int oldMessageId, string text, ReplyMarkupBase replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                var t = Bot.Api.EditMessageTextAsync(chatId, oldMessageId, text, parseMode, disableWebPagePreview, (InlineKeyboardMarkup)replyMarkup);
                t.Wait();
                return t.Result;
            }
            catch (ApiRequestException ex)
            {
                ex.LogError();
                return null;
            }
            catch (Exception e)
            {
                if (e is AggregateException Agg && Agg.InnerExceptions.Any(x => x.Message.ToLower().Contains("message is not modified")))
                {
                    /*
                    var m = "Messae not modified." + Environment.NewLine;
                    m += $"Chat: {chatId}" + Environment.NewLine;
                    m += $"Text: {text}" + Environment.NewLine;
                    m += $"Time: {DateTime.UtcNow.ToLongTimeString()} UTC";
                    Send(Constants.LogGroupId, m);
                    */
                    return null;
                }
                e.LogError();
                return null;
            }
        }

        public static Message SendDocument(long chatId, SendFile SendFile, string caption = null, ReplyMarkupBase replyMarkup = null, bool disableNotification = false, ParseMode parseMode = ParseMode.None)
        {
            try
            {
                return Bot.Api.SendDocumentAsync(chatId, SendFile, caption, parseMode, disableNotification, 0, replyMarkup).Result;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }
        #endregion

        #region Callbacks
        public static bool AnswerCallback(CallbackQuery query, string text = null, bool popup = false)
        {
            try
            {
                var t = Bot.Api.AnswerCallbackQueryAsync(query.Id, text, popup);
                t.Wait();
                return t.Result;            // Await this call in order to be sure it is sent in time
            }
            catch (Exception e)
            {
                e.LogError();
                return false;
            }
        }
        #endregion

    }
}

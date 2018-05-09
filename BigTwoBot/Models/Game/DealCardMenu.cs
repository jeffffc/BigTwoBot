using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using static BigTwoBot.Helpers;

namespace BigTwoBot.Models
{
    public class DealCardMenu
    {
        public InlineKeyboardMarkup MarkUp { get; set; }
        private Guid _GameId { get; set; }
        public string GameId { get { return _GameId.ToString(); } }
        public int TelegramId { get; set; }
        public long GroupId { get; set; }
        public string Language { get; set; }
        public BTPlayerHand Hand { get; set; }
        public MenuCategory CurrentCategory { get; set; } = MenuCategory.One;
        public List<int> ChosenIndexes = new List<int>();
        public List<int> LastValidIndexes = new List<int>();

        public DealCardMenu()
        {

        }

        public DealCardMenu(BTPlayer p, Guid gameId, long groupId)
        {
            _GameId = gameId;
            TelegramId = p.TelegramId;
            Hand = p.Hand;
            GroupId = groupId;
            Language = GetLanguage(GroupId);
            MarkUp = new InlineKeyboardMarkup();
            UpdateHand(Hand);
        }

        public void UpdateHand(BTPlayerHand hand)
        {
            Hand = hand;
            UpdateMenu();
        }

        public void UpdateMenu()
        {
            BTPlayerHand hand = Hand;
            var rows = new List<InlineKeyboardButton[]>();

            var cardButtons = new List<Tuple<string, string>>();
            foreach (BTCard card in hand)
            {
                if (ChosenIndexes.Contains(card.Index))
                    cardButtons.Add(new Tuple<string, string>(card.Selected(), $"{GameId}|{TelegramId}|card|{card.Index}"));
                else
                    cardButtons.Add(new Tuple<string, string>(card.GetCardName(), $"{GameId}|{TelegramId}|card|{card.Index}"));
            }

            
            var row = new List<InlineKeyboardButton>();
            for (int i = 0; i < cardButtons.Count; i += 5)
            {
                row.Clear();
                var subButtons = cardButtons.Skip(i).Take(5).ToList();
                foreach (var button in subButtons)
                    row.Add(new InlineKeyboardCallbackButton(button.Item1, button.Item2));
                rows.Add(row.ToArray());
            }

            var footer1 = new List<InlineKeyboardButton>();
            if (ChosenIndexes.Count > 0)
            {
                var chosen = Hand.Cards.Where(x => ChosenIndexes.Contains(x.Index)).ToList();
                var chosenText = chosen.Select(x => x.GetName()).Aggregate((x, y) => x + " " + y);
                var pokerType = chosen.CheckChosenCards();
                if (pokerType != null)
                {
                    // footer1.Add(new InlineKeyboardCallbackButton(pokerType.ToString() + chosenText, $"{GameId}|{TelegramId}|card|go|{ChosenIndexes.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}"));
                    footer1.Add(new InlineKeyboardCallbackButton(GetTranslation(pokerType.ToString(), Language) + chosenText, $"{GameId}|{TelegramId}|card|go"));
                    LastValidIndexes = ChosenIndexes;
                }
                else
                    footer1.Add(new InlineKeyboardCallbackButton(GetTranslation("Invalid", Language), $"{GameId}|{TelegramId}|card|invalid"));
            }
            else
                footer1.Add(new InlineKeyboardCallbackButton(GetTranslation("ChooseCardButton", Language), $"{GameId}|{TelegramId}|card|dummy"));
            var footer2 = new List<InlineKeyboardButton>();
            footer2.Add(new InlineKeyboardCallbackButton(GetTranslation("Pass", Language), $"{GameId}|{TelegramId}|card|skip"));
            footer2.Add(new InlineKeyboardCallbackButton(GetTranslation("Reset", Language), $"{GameId}|{TelegramId}|card|reset"));

            rows.Add(footer1.ToArray());
            rows.Add(footer2.ToArray());

            var m = new InlineKeyboardMarkup(rows.ToArray());
            MarkUp = m;
        }

        /*
        private string CurrentCategoryString(MenuCategory i)
        {
            var o = ((int)i).ToString();
            return CurrentCategory == i ? $"✔️ {o}" : o;
        }
        */

        
    }

    public enum MenuCategory
    {
        None,
        One,
        Two,
        Three,
        Four,
        Five
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types.Markup;
using static BigTwoBotNode.Helpers;

namespace BigTwoBotNode.Models
{
    public class DealCardMenu
    {
        public InlineKeyboardMarkup MarkUp { get; set; }
        private Guid _GameId { get; set; }
        public string GameId { get { return _GameId.ToString("N"); } }
        public int TelegramId { get; set; }
        public long GroupId { get; set; }
        public string Language { get; set; }
        public BTPlayerHand Hand { get; set; }
        public MenuCategory CurrentCategory { get; set; } = MenuCategory.One;
        public List<int> ChosenIndexes = new List<int>();
        public List<int> LastValidIndexes = new List<int>();
        public bool SortBySuit { get; set; } = false;

        public DealCardMenu()
        {

        }

        public DealCardMenu(BTPlayer p, string gameId, long groupId)
        {
            Guid.TryParse(gameId, out var guid);
            _GameId = guid;
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

            if (SortBySuit)
                hand.SortBySuit();
            else
                hand.SortByNumber();

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
                    row.Add(new InlineKeyboardButton(button.Item1) { CallbackData = button.Item2 });
                rows.Add(row.ToArray());
            }

            var footer1 = new List<InlineKeyboardButton>();
            if (ChosenIndexes.Count > 0)
            {
                var chosen = Hand.Cards.Where(x => ChosenIndexes.Contains(x.Index)).OrderBy(x => x.GameValue).ToList();
                var chosenText = chosen.Select(x => x.GetName()).Aggregate((x, y) => x + " " + y);
                var pokerType = chosen.CheckChosenCards();
                if (pokerType != null)
                {
                    // footer1.Add(new InlineKeyboardCallbackButton(pokerType.ToString() + chosenText, $"{GameId}|{TelegramId}|card|go|{ChosenIndexes.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}"));
                    footer1.Add(new InlineKeyboardButton(GetTranslation(pokerType.ToString(), Language) + chosenText) { CallbackData = $"{GameId}|{TelegramId}|card|go" });
                    LastValidIndexes = ChosenIndexes;
                }
                else
                    footer1.Add(new InlineKeyboardButton(GetTranslation("Invalid", Language)) { CallbackData = $"{GameId}|{TelegramId}|card|invalid" });
            }
            else
                footer1.Add(new InlineKeyboardButton(GetTranslation("ChooseCardButton", Language)){CallbackData = $"{GameId}|{TelegramId}|card|dummy" });
            var footer2 = new List<InlineKeyboardButton>();
            footer2.Add(new InlineKeyboardButton(GetTranslation("Pass", Language)) { CallbackData = $"{GameId}|{TelegramId}|card|skip" });
            footer2.Add(new InlineKeyboardButton(GetTranslation(!SortBySuit ? "SortBySuit" : "SortByNumber", Language)) { CallbackData = $"{GameId}|{TelegramId}|card|sort" });
            footer2.Add(new InlineKeyboardButton(GetTranslation("Reset", Language)) { CallbackData = $"{GameId}|{TelegramId}|card|reset" });

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BigTwoBot.Models
{
    public class BTPlayer
    {
        public BTPlayer(User u, int dbId)
        {
            this.Name = u.FirstName;
            this.Id = dbId;
            this.TelegramId = u.Id;
            this.Username = u.Username;
            this.Hand = new BTPlayerHand();
            this.DealCardMenu = new DealCardMenu();
        }

        public string Name { get; set; }

        public int Id { get; set; }

        public int TelegramId { get; set; }

        public string Username { get; set; }

        public bool FirstPlayer { get; set; } = false;

        public BTPlayerHand Hand { get; set; }

        public DealCardMenu DealCardMenu { get; set; }

        public int? Choice { get; set; } = null;
        public QuestionAsked CurrentQuestion { get; set; } = null;
        public string DeckText { get; set; }

        public void AddCard(BTCard BTCard)
        {
            Hand.AddCard(BTCard);
        }

        public bool CheckBadHand()
        {
            return !Hand.Any(x => x.GameValue >= 14) && Hand.Count(x => new int[] { 11, 12, 13 }.Contains(x.GameValue)) <= 2;
        }

        public void UseCards(List<BTCard> cards)
        {
            Hand.RemoveCards(cards);
            DealCardMenu.ChosenIndexes.Clear();
            DealCardMenu.LastValidIndexes.Clear();
            DealCardMenu.UpdateHand(Hand);
        }

        public void UseCards(BTCard card)
        {
            Hand.RemoveCards(new List<BTCard> { card });
            DealCardMenu.UpdateHand(Hand);
        }

        public void UpdateChosenIndexes(int index = 0, bool empty = false)
        {
            if (empty)
            {
                DealCardMenu.ChosenIndexes.Clear();
                DealCardMenu.LastValidIndexes.Clear();
            }
            else if (DealCardMenu.ChosenIndexes.Contains(index))
                DealCardMenu.ChosenIndexes.Remove(index);
            else
                DealCardMenu.ChosenIndexes.Add(index);
            DealCardMenu.UpdateMenu();
        }

        public int CardCount
        {
            get { return Hand.Count; }
        }

        public PlayedCards PlayTurn(PlayedCards currentMove)
        {
            var cardsToPlay = new PlayedCards(this);

            // display the cards to the player
            DisplayCardList();

            bool valid = false;
            do
            {

                var stillBuildingHand = true;

                do
                {
                    int selectedCardIndex = GetUserSelectedCardIndex();

                    if (selectedCardIndex >= 0)
                    {
                        BTCard selectedCard = Hand[selectedCardIndex];
                        cardsToPlay.AddCard(selectedCard);
                        valid = true;
                    }

                    if (!ContinueBuildingHandPrompt())
                    {
                        stillBuildingHand = false;
                    }
                } while (stillBuildingHand);


                try
                {
                    // validate the move
                    cardsToPlay.Validate(currentMove);
                }
                catch (Exception ex)
                {
                    cardsToPlay.Clear();
                    Console.WriteLine("Invalid move: " + ex.Message);
                    valid = false;
                }
            } while (valid == false);

            // play the cards);
            return cardsToPlay;
        }

        private static bool ContinueBuildingHandPrompt()
        {
            Console.WriteLine("Do you want to add another BTCard? (Y/N)");
            string result = Console.ReadLine();
            return string.Equals(result, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private int GetUserSelectedCardIndex()
        {
            // prompt for input
            Console.WriteLine("Please select a BTCard to play.");
            string rawInput = Console.ReadLine();

            int index;

            if (int.TryParse(rawInput, out index) && index >= 0 && index < Hand.Count)
            {
                if (index > 0)
                {
                    // transform to 0 based index
                    index = index - 1;
                }
                else
                {
                    index = -1;
                }
            }
            else
            {
                index = -1;
                Console.WriteLine("Invalid selection... expected a number between 1 and " + Hand.Count);
            }

            return index;
        }

        private void DisplayCardList()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Hand.Count; i++)
            {
                sb.Append("[");
                sb.Append(i + 1);
                sb.Append("] ");
                sb.AppendLine(Hand[i].GetCardName());
            }

            sb.AppendLine("[0] Pass this turn.");

            Console.WriteLine(sb.ToString());
        }

        public void RemoveCards(PlayedCards playedCards)
        {
            foreach (var BTCard in playedCards)
            {
                Hand.RemoveCard(BTCard);
            }
        }

        public void SortHand()
        {
            BTCard[] sortedHand = Hand.Cards.Sort();
            Hand = new BTPlayerHand(sortedHand);
        }

        public class QuestionAsked
        {
            public QuestionType Type { get; set; }
            public int MessageId { get; set; } = 0;
        }

        public enum QuestionType
        {
            Card, Player
        }
    }

    public class InvalidHandException : Exception
    {
        public PlayedCards Cards { get; private set; }

        public InvalidHandException(PlayedCards cards, string message) : base(message)
        {
            Cards = cards;
        }
    }
}

using BigTwoBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class PlayedCards : BTCardCollection
    {
        public PlayedCards(BTPlayer player)
        {
            Player = player;
        }

        public PlayedCards(BTPlayer player, IEnumerable<BTCard> cards) : base(cards)
        {
            Player = player;
        }

        public BTPlayer Player { get; private set; }

        public BTPokerHands Type { get; set; }

        public void Validate(PlayedCards activeCards)
        {
            if (activeCards == null || activeCards[0] == null || this[0] == null)
            {
                return;
            }

            if (activeCards.Count != this.Count)
            {
                throw new InvalidHandException(this, "You must play the same number of cards.");
            }

            if (this.Count == 1)
            {
                // Validate single BTCard hand
                var nextCard = this[0];
                var activeCard = activeCards[0];

                if (nextCard.GameValue < activeCard.GameValue)
                {
                    throw new InvalidHandException(this, "Cannot play a BTCard of lesser value.");
                }

                if (nextCard.GameValue == activeCard.GameValue && nextCard.Suit < activeCard.Suit)
                {
                    throw new InvalidHandException(this, "Cannot play an equal value BTCard of a lesser suit.");
                }
            }

            // todo: validate poker style hands
        }
    }
}

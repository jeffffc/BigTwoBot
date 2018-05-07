using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class BTDeck : List<BTCard>
    {
        private readonly BTCard[] cards;
        private const int SuitsInDeck = 4;
        private const int CardsInDeck = 52;
        private const int CardsPerSuit = CardsInDeck / SuitsInDeck;

        public BTDeck(IEnumerable<BTCard> deck) : base(deck) { }

        public BTDeck()
        {
            cards = new BTCard[CardsInDeck];

            var suits = new[]
            {
                BTCardSuit.Diamonds,
                BTCardSuit.Clubs,
                BTCardSuit.Hearts,
                BTCardSuit.Spades
            };

            for (int i = 0; i < SuitsInDeck; i++)
            {
                BTCardSuit suit = suits[i];


                for (int j = 0; j < CardsPerSuit; j++)
                {
                    var index = j + (i * CardsPerSuit);

                    var value = j + 1;
                    int gameValue;

                    switch (value)
                    {
                        case 1:
                            // aces are high
                            gameValue = 14;
                            break;
                        case 2:
                            // 2's are the highest valued cards in this game.
                            gameValue = 15;
                            break;
                        default:
                            gameValue = value;
                            break;
                    }

                    cards[index] = new BTCard(value, gameValue, index, suit);
                }
            }
            this.AddRange(cards);
        }

        public new BTCard this[int i]
        {
            get { return cards[i]; }
            set { cards[i] = value; }
        }
    }
}

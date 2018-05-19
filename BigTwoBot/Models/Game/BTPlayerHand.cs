using BigTwoBot.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class BTPlayerHand : BTCardCollection
    {
        public BTPlayerHand() : base()
        {
            cards = new List<BTCard>();
        }

        public BTPlayerHand(IEnumerable<BTCard> cards) : base(cards)
        {
        }

        public bool HasPokerType(BTPokerHands type)
        {
            switch (type)
            {
                case BTPokerHands.Single:
                case BTPokerHands.Double:
                case BTPokerHands.Triple:
                case BTPokerHands.Quadruple:
                    return HasSameValuedCards(type);
                case BTPokerHands.Straight:
                    return HasStraight();
                case BTPokerHands.Flush:
                    return HasFlush();
                case BTPokerHands.FullHouse:
                    return HasFullHouse();
                case BTPokerHands.StraightFlush:
                    return HasStraightFlush();
            }
            return true;
        }

        public bool HasStraight()
        {
            List<BTCard> tmpCards = new List<BTCard>(this); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            var values = tmpCards.DistinctBy(x => x.Value).OrderBy(x => x.Value).Select(x => x.Value).ToList();
            if (tmpCards.Any(x => x.Value == 1)) values.Add(14);
            var t = values.Zip(values.Skip(4), (a, b) => (a + 4) == b);
            return t.Any(x => x);
        }

        public bool HasFlush()
        {
            List<BTCard> tmpCards = new List<BTCard>(this); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            IEnumerable<IGrouping<BTCardSuit, BTCard>> triples = tmpCards.GroupBy(c => c.Suit).Where(g => g.Count() == 5);
            return triples.Any();
        }

        public bool HasFullHouse()
        {
            List<BTCard> tmpCards = new List<BTCard>(this); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            IEnumerable<IGrouping<int, BTCard>> triples = GetSameValuedCards(BTPokerHands.Triple);
            IEnumerable<IGrouping<int, BTCard>> doubles = GetSameValuedCards(BTPokerHands.Double);
            return triples.Any() && doubles.Any();
        }

        public bool HasStraightFlush()
        {
            return HasStraight() & HasFlush();
        }


        /// <summary>
        /// Has Single/Double/Triple/Quadripe
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasSameValuedCards(BTPokerHands type)
        {
            IEnumerable<IGrouping<int, BTCard>> pairs = GetSameValuedCards(type);
            return pairs.Any();
        }

        public IEnumerable<IGrouping<int, BTCard>> GetSameValuedCards(BTPokerHands type)
        {
            IEnumerable<IGrouping<int, BTCard>> matchingCards = this
                .GroupBy(c => c.Value)
                .Where(g => g.Count() == (int)type + 1);

            return matchingCards;
        }

        public IEnumerable<IGrouping<int, BTCard>> GetSameGameValuedCards(BTPokerHands type)
        {
            IEnumerable<IGrouping<int, BTCard>> matchingCards = this
                .GroupBy(c => c.GameValue)
                .Where(g => g.Count() == (int)type + 1);

            return matchingCards;
        }


        public bool IsCardPartOfStrongerHand(BTCard BTCard)
        {
            if (!this.Contains(BTCard))
            {
                throw new ArgumentOutOfRangeException("We don't have a " + BTCard.GetCardName());
            }

            // check if it's part of a pair, this return true for trips and quads as well.
            var isAtleastPair = GetSameValuedCards(BTPokerHands.Double)
                .Any(g => g
                    .Any(c => c.Value == BTCard.Value)
                );


            // todo: check if it's part of a poker hand


            // not part of stronger hand.
            return isAtleastPair;
        }
    }
}

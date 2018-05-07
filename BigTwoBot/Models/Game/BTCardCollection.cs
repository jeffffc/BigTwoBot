using BigTwoBot.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class BTCardCollection : IEnumerable<BTCard>
    {
        protected IList<BTCard> cards;

        public BTCardCollection()
        {
            this.cards = new List<BTCard>();
        }

        public BTCardCollection(IEnumerable<BTCard> cards)
        {
            this.cards = new List<BTCard>(cards);
        }

        public BTCard this[int i]
        {
            get { return cards[i]; }
        }

        public int Count
        {
            get { return cards.Count; }
        }

        public BTCard[] Cards
        {
            get { return cards.ToArray(); }
        }

        public void AddCard(BTCard BTCard)
        {
            cards.Add(BTCard);
        }

        public void AddCards(IEnumerable<BTCard> collection)
        {
            foreach (BTCard BTCard in collection)
            {
                this.cards.Add(BTCard);
            };
        }

        public void RemoveCard(BTCard BTCard)
        {
            cards.Remove(BTCard);
        }

        public void RemoveCards(IEnumerable<BTCard> toBeRemoved)
        {
            foreach (var c in toBeRemoved)
                cards.Remove(c);
        }

        public void Clear()
        {
            cards.Clear();
        }

        public IEnumerator<BTCard> GetEnumerator()
        {
            return this.cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (BTCard BTCard in cards)
            {
                sb.Append(BTCard.GetCardName());
                sb.Append(", ");
            }

            // remove the trailing comma and space
            return sb.ToString(0, sb.Length - 2);
        }
    }
}

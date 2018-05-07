using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class BTCard
    {
        public BTCard(int value, int gameValue, int index, BTCardSuit suit)
        {
            this.Value = value;
            this.Suit = suit;
            this.GameValue = gameValue;
            this.Index = index;
        }

        public BTCardSuit Suit { get; private set; }

        public int Value { get; private set; }

        public int GameValue { get; private set; }

        public int Index { get; private set; }

        public bool IsGreaterThan(BTCard other)
        {
            return (GameValue == other.GameValue && Suit > other.Suit) || GameValue > other.GameValue;
        }

        public static bool operator >(BTCard a, BTCard b)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a", "null value passed to greater than operator");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b", "null value passed to greater than operator");
            }

            return (a.GameValue == b.GameValue && a.Suit > b.Suit) || a.GameValue > b.GameValue;
        }

        public static bool operator <(BTCard a, BTCard b)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a", "null value passed to less than operator");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b", "null value passed to less than operator");
            }

            return (a.GameValue == b.GameValue && a.Suit < b.Suit) || a.GameValue < b.GameValue;
        }

        public static bool operator ==(BTCard a, BTCard b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Value == b.Value && a.Suit == b.Suit;
        }

        public static bool operator !=(BTCard a, BTCard b)
        {
            return !(a == b);
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o)) return false;
            if (ReferenceEquals(this, o)) return true;
            var card = o as BTCard;
            return Value == card.Value && Suit == card.Suit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public string GetCardName()
        {
            string name;

            switch (Value)
            {
                case 1:
                    name = "A";
                    break;
                case 11:
                    name = "J";
                    break;
                case 12:
                    name = "Q";
                    break;
                case 13:
                    name = "K";
                    break;
                default:
                    name = Value.ToString();
                    break;
            }

            return $"{Suit.ToEmoji()} {name}";
        }
    }
}

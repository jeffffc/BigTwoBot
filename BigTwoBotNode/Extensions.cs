using BigTwoBotNode.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelegramBotApi.Types;
using TelegramBotApi.Enums;
using System.ComponentModel;
using MoreLinq;
using BigTwoBot;

namespace BigTwoBotNode
{

    public static class ExtensionMethods
    {
        // Random List members
        public static List<T> Shuffle<T>(this List<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static List<T> Shuffle<T>(this List<T> list, int numberOfTimes)
        {
            for (int i = 0; i < numberOfTimes; i++)
                list.Shuffle();
            return list;
        }

        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.ElementAtOrDefault(Helpers.RandomNum(list.Count()));
        }

        public static T Pop<T>(this List<T> list)
        {
            T r = list.First();
            list.RemoveAt(0);
            return r;
        }


        public static void LogError(this Exception e, long chatId = 0)
        {
            string m = "Error occured." + Environment.NewLine;
            if (chatId > 0)
                m += $"ChatId: {chatId}" + Environment.NewLine + Environment.NewLine;
            var trace = e.StackTrace;
            do
            {
                m += e.Message + Environment.NewLine + Environment.NewLine;
                e = e.InnerException;
            }
            while (e != null);

            m += trace;

            Bot.Send(Constants.LogGroupId, m, parseMode: ParseMode.Html);
        }

        // Player Extensions
        public static string GetName(this BTCard c)
        {
            return c.GetCardName();
        }

        public static string GetName(this BTPlayer player)
        {
            var name = player.Name;
            if (!String.IsNullOrEmpty(player.Username))
                return $"<a href=\"https://telegram.me/{player.Username}\">{name.FormatHTML()}</a>";
            return name.ToBold();
        }

        public static string GetName(this User player)
        {
            var name = player.FirstName;
            if (!String.IsNullOrEmpty(player.Username))
                return $"<a href=\"https://telegram.me/{player.Username}\">{name.FormatHTML()}</a>";
            return name.ToBold();
        }

        public static string GetMention(this BTPlayer player)
        {
            var name = player.Name;
            return $"<a href='tg://user?id={player.TelegramId}'>{name}</a>";
        }

        public static string GetMention(this User player)
        {
            var name = player.FirstName;
            return $"<a href='tg://user?id={player.Id}'>{name}</a>";
        }

        // Bot Extensions
        public static string ToBold(this object str)
        {
            if (str == null)
                return null;
            return $"<b>{str.ToString().FormatHTML()}</b>";
        }

        public static string ToItalic(this object str)
        {
            if (str == null)
                return null;
            return $"<i>{str.ToString().FormatHTML()}</i>";
        }

        public static string ToCode(this string str)
        {
            if (str == null)
                return null;
            return $"<code>{str.FormatHTML()}</code>";
        }

        public static string FormatHTML(this string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        // Deck Extensions
        public static BTCard[] Sort(this BTCard[] cards)
        {
            return cards.OrderBy(x => x.GameValue).ThenBy(x => x.Suit).ToArray();
        }

        public static string ToEmoji(this BTCardSuit suit)
        {
            switch (suit)
            {
                case BTCardSuit.Diamonds:
                    return "♦️";
                case BTCardSuit.Clubs:
                    return "♣️";
                case BTCardSuit.Hearts:
                    return "♥️";
                case BTCardSuit.Spades:
                    return "♠️";
                default:
                    throw new InvalidEnumArgumentException("WTF none of the 4 suits?");
            }
        }

        public static string Selected(this BTCard card)
        {
            return $"✔️ {card.GetCardName()}";
        }

        public static string GetString(this List<BTCard> cards)
        {
            return cards.Select(x => x.GetCardName()).Aggregate((x, y) => x + " " + y);
        }

        public static string GetString(this List<BTPlayer> players, bool mention = false)
        {
            if (!mention)
                return players.Select(x => x.GetName()).Aggregate((x, y) => x + ">" + y);
            else
                return players.Select(x => x.GetMention()).Aggregate((x, y) => x + ">" + y);
        }

        public static string GetString(this Queue<BTPlayer> players, bool mention = false)
        {
            var temp = players.ToList();
            return temp.GetString(mention);
        }

        public static string GetNumOfCardsString(this List<BTPlayer> players, bool mention = false)
        {
            if (!mention)
                return players.Select(x => $"{x.GetName()} - {x.Hand.Count}").Aggregate((x, y) => x + "\n" + y);
            else
                return players.Select(x => $"{x.GetMention()} - {x.Hand.Count}").Aggregate((x, y) => x + "\n" + y);
        }

        public static string GetNumOfCardsString(this Queue<BTPlayer> players, bool mention = false)
        {
            var temp = players.ToList();
            return temp.GetNumOfCardsString(mention);
        }

        // card values checking
        public static HandComparison CompareHandWith(this IEnumerable<BTCard> hand, IEnumerable<BTCard> toBeCompared)
        {
            return hand.ToList().CompareHandWith(toBeCompared.ToList());
        }

        public static HandComparison CompareHandWith(this List<BTCard> hand, List<BTCard> toBeCompared)
        {
            if (toBeCompared == null || toBeCompared.Count() == 0)
                return new HandComparison(true);
            var count = hand.Count;
            if (count != toBeCompared.Count)
                return new HandComparison(false, HandComparisonFailReason.HandCountDifferent);
            switch (count)
            {
                case 1:
                    if (hand.First() < toBeCompared.First())
                        return new HandComparison(false, HandComparisonFailReason.ValueSmaller);
                    break;
                case 2:
                case 3:
                    if (hand.OrderByDescending(x => x.GameValue).ThenByDescending(x => x.Suit).First() < toBeCompared.OrderByDescending(x => x.GameValue).ThenByDescending(x => x.Suit).First())
                        return new HandComparison(false, HandComparisonFailReason.ValueSmaller);
                    break;
                case 5:
                    var thisType = hand.CheckChosenCards();
                    var thatType = toBeCompared.CheckChosenCards();
                    if (thisType < thatType)
                        return new HandComparison(false, HandComparisonFailReason.TypeSmaller);
                    if (thisType == thatType)
                    {
                        switch (thisType)
                        {
                            case BTPokerHands.StraightFlush:
                                return hand.CompareStraightOrFlush(toBeCompared, thisType);
                            case BTPokerHands.Quadruple:
                                var qa = hand.GetSameValuedCards(BTPokerHands.Quadruple).First().First().GameValue;
                                var qb = toBeCompared.GetSameValuedCards(BTPokerHands.Quadruple).First().First().GameValue;
                                if (qa < qb)
                                    return new HandComparison(false, HandComparisonFailReason.QuadrupleSmaller);
                                break;
                            case BTPokerHands.FullHouse:
                                var fa = hand.GetSameValuedCards(BTPokerHands.Triple).First().First().GameValue;
                                var fb = toBeCompared.GetSameValuedCards(BTPokerHands.Triple).First().First().GameValue;
                                if (fa < fb)
                                    return new HandComparison(false, HandComparisonFailReason.FullHouseTripleSmaller);
                                break;
                            case BTPokerHands.Flush:
                                return hand.CompareStraightOrFlush(toBeCompared, thisType);
                            case BTPokerHands.Straight:
                                return hand.CompareStraightOrFlush(toBeCompared, thisType);
                        }
                    }
                    break;
            }
            return new HandComparison(true);
        }

        public static BTPokerHands? CheckChosenCards(this List<BTCard> chosen)
        {
            var types = Enum.GetValues(typeof(BTPokerHands)).Cast<BTPokerHands>().Reverse();
            foreach (BTPokerHands pokerType in types)
            {
                if (chosen.HasPokerType(pokerType))
                {
                    return pokerType;
                }
            }
            return null;
        }

        public static bool HasPokerType(this List<BTCard> cards, BTPokerHands type)
        {
            switch (type)
            {
                case BTPokerHands.Single:
                case BTPokerHands.Double:
                case BTPokerHands.Triple:
                    return cards.Count == (int)type + 1 && cards.HasSameValuedCards(type);
                case BTPokerHands.Quadruple:
                    return cards.Count == 5 && cards.HasSameValuedCards(type);
                case BTPokerHands.Straight:
                    return cards.Count == 5 && cards.HasStraight();
                case BTPokerHands.Flush:
                    return cards.Count == 5 && cards.HasFlush();
                case BTPokerHands.FullHouse:
                    return cards.Count == 5 && cards.HasFullHouse();
                case BTPokerHands.StraightFlush:
                    return cards.Count == 5 && cards.HasStraightFlush();
            }
            return true;
        }

        public static bool HasSameValuedCards(this List<BTCard> cards, BTPokerHands type)
        {
            IEnumerable<IGrouping<int, BTCard>> pairs = cards.GetSameValuedCards(type);
            return pairs.Any();
        }

        public static IEnumerable<IGrouping<int, BTCard>> GetSameValuedCards(this List<BTCard> cards, BTPokerHands type)
        {
            var temp = type == BTPokerHands.Quadruple ? 4 : (int)type + 1;
            IEnumerable<IGrouping<int, BTCard>> matchingCards = cards
                .GroupBy(c => c.Value)
                .Where(g => g.Count() == temp);

            return matchingCards;
        }


        public static bool IsCardPartOfStrongerHand(this List<BTCard> cards, BTCard BTCard)
        {
            if (!cards.Contains(BTCard))
            {
                throw new ArgumentOutOfRangeException("We don't have a " + BTCard.GetCardName());
            }

            // check if it's part of a pair, this return true for trips and quads as well.
            var isAtleastPair = cards.GetSameValuedCards(BTPokerHands.Double)
                .Any(g => g
                    .Any(c => c.Value == BTCard.Value)
                );


            // todo: check if it's part of a poker hand


            // not part of stronger hand.
            return isAtleastPair;
        }

        public static HandComparison CompareStraightOrFlush(this List<BTCard> hand, List<BTCard> toBeCompared, BTPokerHands? handType)
        {
            HandComparisonFailReason reason = HandComparisonFailReason.Unknown;
            switch (handType)
            {
                case BTPokerHands.Flush:
                    reason = HandComparisonFailReason.FlushMaxSmaller;
                    break;
                case BTPokerHands.Straight:
                    reason = HandComparisonFailReason.StraightMaxSmaller;
                    break;
                case BTPokerHands.StraightFlush:
                    reason = HandComparisonFailReason.StraightFlushMaxSmaller;
                    break;
            }
            var sfa = hand.OrderByDescending(x => x.GameValue);
            var sfb = toBeCompared.OrderByDescending(x => x.GameValue);

            // compare value first
            for (int i = 0; i < sfa.Count(); i++)
            {
                if (sfa.ElementAt(i).GameValue < sfb.ElementAt(i).GameValue)
                    return new HandComparison(false, reason);
                if (sfa.ElementAt(i).GameValue == sfb.ElementAt(i).GameValue)
                    continue;
                else
                    return new HandComparison(true);
            }
            // compare suit
            if (sfa.ElementAt(0) < sfb.ElementAt(0))
                return new HandComparison(false, reason);
            else
                return new HandComparison(true);
        }

        public static List<int> GetStraightResult(this List<BTCard> cards)
        {
            var values = cards.DistinctBy(x => x.Value).OrderBy(x => x.Value).Select(x => x.Value).ToList();
            if (cards.Any(x => x.Value == 1)) values.Add(14);
            return values;
        }

        public static bool HasStraight(this List<BTCard> cards)
        {
            List<BTCard> tmpCards = new List<BTCard>(cards); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            var values = cards.GetStraightResult();
            var t = values.Zip(values.Skip(4), (a, b) => (a + 4) == b);
            return t.Any(x => x);
        }

        public static bool HasFlush(this List<BTCard> cards)
        {
            List<BTCard> tmpCards = new List<BTCard>(cards); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            IEnumerable<IGrouping<BTCardSuit, BTCard>> triples = tmpCards.GroupBy(c => c.Suit).Where(g => g.Count() == 5);
            return triples.Any();
        }

        public static bool HasFullHouse(this List<BTCard> cards)
        {
            List<BTCard> tmpCards = new List<BTCard>(cards); //local copy of the cards
            if (tmpCards.Count < 5) //need at least 5 cards to make a straight
                return false;
            IEnumerable<IGrouping<int, BTCard>> triples = cards.GetSameValuedCards(BTPokerHands.Triple);
            IEnumerable<IGrouping<int, BTCard>> doubles = cards.GetSameValuedCards(BTPokerHands.Double);
            return triples.Any() && doubles.Any();
        }

        public static bool HasStraightFlush(this List<BTCard> cards)
        {
            return cards.HasStraight() && cards.HasFlush();
        }
    }
}

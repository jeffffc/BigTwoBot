using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BigTwoBot.Models;
using BigTwoBot;
using System.Threading;
using Database;
using System.Diagnostics;
using System.IO;
using Telegram.Bot.Types.InlineKeyboardButtons;
using System.Xml.Linq;
using static BigTwoBot.Helpers;
using ConsoleTables;
using System.Drawing;
using System.Drawing.Imaging;
using static BigTwoBot.Models.BTPlayer;

namespace BigTwoBot
{
    public class BigTwo : IDisposable
    {
        #region Initiate Variables

        public long ChatId;
        public string GroupName;
        public int GameId;
        public string GroupLink;
        public Group DbGroup;
        public List<BTPlayer> Players;
        public Queue<BTPlayer> PlayerQueue = new Queue<BTPlayer>();
        public BTDeck Deck;
        public int ChooseCardTime = Constants.ChooseCardTime;
        public bool PlayChips = false;
        public int ChipsPerCard = Constants.ChipsPerCard;

        public List<BTCard> CurrentHand = new List<BTCard>();
        public BTPokerHands? CurrentHandType = null;
        public BTPlayer CurrentDealer;

        public BTPlayer Winner;
        public bool BigTwoDealt = false;
        public BTPlayer BigTwoDealtBy;

        public InlineKeyboardMarkup GroupMarkup = null;
        public InlineKeyboardMarkup BotMarkup;

        public BTPlayer Initiator;
        public Guid Id = Guid.NewGuid();
        public int JoinTime = Constants.JoinTime;
        public GamePhase Phase = GamePhase.Joining;
        private int _secondsToAdd = 0;
        private int _playCardSecondsToAdd = 0;
        private int _playerList = 0;
        public string CurrentTableStickerId;
        public Database.Game DbGame;

        public Locale Locale;
        public string Language = "English";

        #endregion


        public BigTwo(long chatId, User u, string groupName, string chatUsername = null)
        {
            ChatId = chatId;
            GroupName = groupName;
            Players = new List<BTPlayer>();
            Deck = new BTDeck();
            #region Creating New Game - Preparation
            using (var db = new BigTwoDb())
            {
                DbGroup = db.Groups.FirstOrDefault(x => x.GroupId == ChatId);
                DbGroup.UserName = chatUsername;
                GroupLink = DbGroup.UserName != null ? $"https://t.me/{DbGroup.UserName}" : DbGroup.GroupLink ?? null;
                LoadLanguage(DbGroup.Language);
                if (DbGroup == null)
                    Bot.RemoveGame(this);
                ChooseCardTime = DbGroup.ChooseCardTime ?? Constants.ChooseCardTime;
                PlayChips = DbGroup.PlayChips ?? false;
                ChipsPerCard = DbGroup.ChipsPerCard ?? Constants.ChipsPerCard;
                db.SaveChanges();
                if (GroupLink != null)
                    GroupMarkup = new InlineKeyboardMarkup(
                        new InlineKeyboardButton[][] {
                            new InlineKeyboardUrlButton[] {
                                new InlineKeyboardUrlButton(GetTranslation("BackGroup", GroupName), GroupLink)
                            }
                        });
                BotMarkup = new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[] {
                            new InlineKeyboardUrlButton(GetTranslation("GoToBot"), $"https://t.me/{Bot.Me.Username}")
                        }
                    });
            }
            // something
            #endregion

            var msg = GetTranslation("NewGame", u.GetName());
            if (PlayChips)
                msg += Environment.NewLine + GetTranslation("GameChipsPerCard", ChipsPerCard);
            // beta message
            msg += Environment.NewLine + Environment.NewLine + GetTranslation("Beta");
            Bot.Send(chatId, msg);
            AddPlayer(u, true);
            Initiator = Players[0];

            new Task(() => { NotifyNextGamePlayers(); }).Start();
            new Thread(GameTimer).Start();
        }

        #region Main method

        private void GameTimer()
        {
            while (Phase != GamePhase.Ending && Phase != GamePhase.KillGame)
            {
                try
                {
#if DEBUG
                AddPlayer(new User { Id = 433942669, FirstName = "Mud9User", IsBot = false, LanguageCode = "zh-HK", Username = "mud9user" });
                AddPlayer(new User { Id = 415774316, FirstName = "Avalonese Dev", IsBot = false, LanguageCode = "zh-HK", Username = "avalonesedev" });
                AddPlayer(new User { Id = 267359519, FirstName = "Ian", IsBot = false, LanguageCode = "zh-HK", Username = "lmaohahaha" });
#endif
                    for (var i = 0; i < JoinTime; i++)
                    {
                        if (this.Phase == GamePhase.InGame)
                            break;
                        if (this.Phase == GamePhase.Ending)
                            return;
                        if (this.Phase == GamePhase.KillGame)
                            return;
                        //try to remove duplicated game
                        if (i == 10)
                        {
                            var count = Bot.Games.Count(x => x.ChatId == ChatId);
                            if (count > 1)
                            {
                                var toDel = Bot.Games.Where(x => x.Players.Count < this.Players.Count).OrderBy(x => x.Players.Count).Where(x => x.Id != this.Id && x.Phase != GamePhase.InGame);
                                if (toDel != null)
                                {
                                    Send(GetTranslation("DuplicatedGameRemoving"));
                                    foreach (var g in toDel)		
	                                {		
	                                    g.Phase = GamePhase.KillGame;		
			
	                                    try		
	                                    {		
                                        Bot.RemoveGame(g);		
	                                    }		
	                                    catch		
	                                    {		
	                                        // should be removed already		
	                                    }		
	                                }
                                }
                            }
                        }
                        if (_secondsToAdd != 0)
                        {
                            i = Math.Max(i - _secondsToAdd, Constants.JoinTime - Constants.JoinTimeMax);
                            // Bot.Send(ChatId, GetTranslation("JoinTimeLeft", TimeSpan.FromSeconds(Constants.JoinTime - i).ToString(@"mm\:ss")));
                            _secondsToAdd = 0;
                        }
                        var specialTime = JoinTime - i;
                        if (new int[] { 10, 30, 60, 90 }.Contains(specialTime))
                        {
                            Bot.Send(ChatId, GetTranslation("JoinTimeSpecialSeconds", specialTime));
                        }
                        if (Players.Count == 4)
                            break;
                        Thread.Sleep(1000);
                    }

                    if (this.Phase == GamePhase.Ending)
                        return;
                    do
                    {
                        BTPlayer p = Players.FirstOrDefault(x => Players.Count(y => y.TelegramId == x.TelegramId) > 1);
                        if (p == null) break;
                        Players.Remove(p);
                    }
                    while (true);

                    if (this.Phase == GamePhase.Ending)
                        return;

                    if (this.Players.Count() == 4)
                        this.Phase = GamePhase.InGame;
                    if (this.Phase != GamePhase.InGame)
                    {
                        /*
                        this.Phase = GamePhase.Ending;
                        Bot.RemoveGame(this);
                        Bot.Send(ChatId, "Game ended!");
                        */
                    }
                    else
                    {
                        #region Ready to start game
                        if (Players.Count < 4)
                        {
                            Send(GetTranslation("GameEnded"));
                            return;
                        }

                        Send(GetTranslation("GameStart"));

                        // create game + gameplayers in db
                        using (var db = new BigTwoDb())
                        {
                            DbGame = new Database.Game
                            {
                                GrpId = DbGroup.Id,
                                GroupId = ChatId,
                                GroupName = GroupName,
                                TimeStarted = DateTime.UtcNow,
                                ChipsPerCard = ChipsPerCard
                            };
                            db.Games.Add(DbGame);
                            db.SaveChanges();
                            GameId = DbGame.Id;
                            foreach (var p in Players)
                            {
                                GamePlayer DbGamePlayer = new GamePlayer
                                {
                                    PlayerId = db.Players.FirstOrDefault(x => x.TelegramId == p.TelegramId).Id,
                                    GameId = GameId
                                };
                                db.GamePlayers.Add(DbGamePlayer);
                            }
                            db.SaveChanges();
                        }

                        PrepareGame();

                        // remove joined players from nextgame list
                        // RemoveFromNextGame(Players.Select(x => x.TelegramId).ToList());

                        #endregion

                        #region Start!
                        foreach (var player in Players)
                        {
                            // SendPM(player, GetPlayerInitialCards(player.CardsInHand));
                        }
                        while (Phase != GamePhase.Ending)
                        {
                            // _playerList = Send(GeneratePlayerList()).MessageId;
                            PlayersChooseCard();
                            if (Phase == GamePhase.Ending)
                                break;
                            if (BigTwoDealt)
                                NextPlayerAfterBigTwo(BigTwoDealtBy);
                            else
                                NextPlayer();
                        }
                        EndGame();
                        #endregion
                    }
                    this.Phase = GamePhase.Ending;
                    Bot.Send(ChatId, GetTranslation("GameEnded"));
                }
                catch (Exception ex)
                {
                    if (Phase == GamePhase.KillGame)
                    {
                        // normal
                    }
                    else
                    {
                        Log(ex);
                        Phase = GamePhase.KillGame;
                    }
                }
            }

            Bot.RemoveGame(this);
        }

        #endregion


        #region Player Control

        private void AddPlayer(User u, bool newGame = false)
        {
            var player = this.Players.FirstOrDefault(x => x.TelegramId == u.Id);
            if (player != null)
                return;

            using (var db = new BigTwoDb())
            {
                var DbPlayer = db.Players.FirstOrDefault(x => x.TelegramId == u.Id);
                if (DbPlayer == null)
                {
                    DbPlayer = new Player
                    {
                        TelegramId = u.Id,
                        Name = u.FirstName,
                        Language = "English"
                    };
                    db.Players.Add(DbPlayer);
                    db.SaveChanges();
                }
                BTPlayer p = new BTPlayer(u, DbPlayer.Id);
                try
                {
                    Message ret;
                    try
                    {
                        ret = SendPM(p, GetTranslation("YouJoined", GroupName), GroupMarkup);
                        if (ret == null)
                            throw new Exception();
                    }
                    catch
                    {
                        Bot.Send(ChatId, GetTranslation("NotStartedBot", u.GetName()), GenerateStartMe());
                        return;
                    }
                }
                catch { }
                this.Players.Add(p);
            }
            if (!newGame)
                _secondsToAdd += 15;

            do
            {
                BTPlayer p = Players.FirstOrDefault(x => Players.Count(y => y.TelegramId == x.TelegramId) > 1);
                if (p == null) break;
                Players.Remove(p);
            }
            while (true);

            Send(GetTranslation("JoinedGame", u.GetName()) + Environment.NewLine + GetTranslation("JoinInfo", Players.Count, 4));
        }

        private void RemovePlayer(User user)
        {
            if (this.Phase != GamePhase.Joining) return;

            var player = this.Players.FirstOrDefault(x => x.TelegramId == user.Id);
            if (player == null)
                return;

            this.Players.Remove(player);

            do
            {
                BTPlayer p = Players.FirstOrDefault(x => Players.Count(y => y.TelegramId == x.TelegramId) > 1);
                if (p == null) break;
                Players.Remove(p);
            }
            while (true);

            Send(GetTranslation("FledGame", user.GetName()) + Environment.NewLine + GetTranslation("JoinInfo", Players.Count, 3, 8));
        }

        public void CleanPlayers()
        {
            foreach (var p in Players)
            {
                p.Choice = null;
                p.CurrentQuestion = null;
                p.FirstPlayer = false;
            }
        }

        #endregion


        #region Visualize Cards Related

        /*
        public FileToSend GetTableCardsImage(List<BTCard[]> cards)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Image temp = (Image)Constants.boardImage.Clone();
                Graphics board = Graphics.FromImage(temp);

                int w = Constants.widthSides;
                int h = Constants.heightSides;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var c = cards[j][i];
                        if (c != null)
                            board.DrawImage(Constants.cardImages[c.Number - 1], w, h);
                        h += Constants.eachHeight;
                    }
                    w += Constants.eachWidth;
                    h = Constants.heightSides;
                }

                var s = ToStream(temp, ImageFormat.Png);
                // boardImage.Save(outputPath);
                return new FileToSend("sticker", s);
            }
        }

        private Stream ToStream(Image image, ImageFormat format)
        {
            // 
            // image.Save(stream, format);
            Bitmap img = new Bitmap(image);
            byte[] bytes;
            using (WebP webp = new WebP())
                bytes = webp.EncodeLossless(img);
            var stream = new System.IO.MemoryStream(bytes);
            stream.Position = 0;
            return stream;
        }
        */

        #endregion


        #region Game Methods

        public void PlayersChooseCard()
        {
            try
            {
                if (Players.Any(x => x.Hand.Cards.Count() == 0))
                    Phase = GamePhase.Ending;
                if (Phase == GamePhase.Ending) return;

                BigTwoDealt = false;
                BigTwoDealtBy = null;


                var p = PlayerQueue.First();

                // Send Menu to P
                if (CurrentDealer?.Id == p.Id)
                {
                    CurrentDealer = null;
                    CurrentHand.Clear();
                    CurrentHandType = null;
                }
                var forP = CurrentHand.Count > 0 ? GetTranslation("CurrentlyOnTable") + Environment.NewLine + CurrentHand.GetString() + Environment.NewLine + GetTranslation("ChooseCard") : GetTranslation("ChooseCard");
                SendMenu(p, forP, p.DealCardMenu.MarkUp);
                var forGroup = PlayerQueue.GetNumOfCardsString() + Environment.NewLine + Environment.NewLine;
                forGroup += CurrentHand.Count > 0 ? GetTranslation("CurrentlyOnTable") + CurrentDealer.GetName() + ":" + Environment.NewLine + CurrentHand.GetString() : GetTranslation("TableClean");
                forGroup += Environment.NewLine + Environment.NewLine + GetTranslation("PlayersTurn", p.GetMention(), ChooseCardTime);
                Send(forGroup, BotMarkup);

                // choose card time
                for (int i = 0; i < ChooseCardTime; i++)
                {
                    Thread.Sleep(1000);
                    if (p.CurrentQuestion == null)
                        break;
                    if (_playCardSecondsToAdd != 0)
                    {
                        i = i - _playCardSecondsToAdd;
                        // Bot.Send(ChatId, GetTranslation("JoinTimeLeft", TimeSpan.FromSeconds(Constants.JoinTime - i).ToString(@"mm\:ss")));
                        _playCardSecondsToAdd = 0;
                    }
                }

                if (p.CurrentQuestion != null)
                {
                    Bot.Edit(p.TelegramId, p.CurrentQuestion.MessageId, GetTranslation("TimesUpButton"));
                    p.CurrentQuestion = null;
                }

                if (CurrentDealer?.Id == p.Id)
                    Send(GetTranslation("PlayedHand", p.GetName(), GetTranslation(CurrentHandType.ToString()), CurrentHand.GetString()));
                else
                    Send(GetTranslation("Passed", p.GetName()));

                if (p.Hand.Count == 0)
                {
                    Winner = p;
                    Phase = GamePhase.Ending;
                }

                if (CurrentHand.Count == 1 && CurrentHand.First().Value == 2 && CurrentHand.First().Suit == BTCardSuit.Spades)
                {
                    Send("");
                    BigTwoDealt = true;
                    BigTwoDealtBy = p;
                    return;
                }

                CleanPlayers();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public void NextPlayer()
        {
            var p = PlayerQueue.Dequeue();
            p.Choice = 0;
            p.CurrentQuestion = null;
            PlayerQueue.Enqueue(p);
        }

        public void NextPlayerAfterBigTwo(BTPlayer p)
        {
            while (PlayerQueue.First() != p)
                NextPlayer();
        }

        public void PrepareGame()
        {
            var tempPlayerList = Players.Shuffle(10);
            PlayerQueue = new Queue<BTPlayer>(tempPlayerList);

            // Shuffle the cards before assigning to players
            Deck.Shuffle(10);

            bool ok = false;

            while (!ok)
            // assign cards to players
            {
                foreach (var p in Players)
                    p.Hand.Clear();
                for (int i = 0; i < Deck.Count; i += 4)
                {
                    var cards = Deck.Skip(i).Take(4).ToArray();
                    for (int j = 0; j < 4; j++)
                    {
                        Players[j].AddCard(cards[j]);
                    }
                }
                if (!Players.Any(x => x.CheckBadHand()))
                {
                    ok = true;
                }
            }

            // Sort their cards, create menu
            foreach (BTPlayer p in Players)
            {
                p.SortHand();
                p.DealCardMenu = new DealCardMenu(p, Id, ChatId);
            }

#if DEBUG
            while (PlayerQueue.First().TelegramId != 106665913) NextPlayer();
#else
            while (!PlayerQueue.First().Hand.Any(x => x.Value == 3 && x.Suit == BTCardSuit.Diamonds)) NextPlayer();
#endif

            // set first player, avoid not using diamond 3
            PlayerQueue.First().FirstPlayer = true;

            // Send the Queue
            Send(PlayerQueue.GetString(true));
        }

        public void EndGame()
        {
            var winner = PlayerQueue.First();
            Winner = winner;
            var finalMsg = $"{winner.GetName()} {GetTranslation("Won")}" + Environment.NewLine + Environment.NewLine;
            if (!PlayChips)
                finalMsg += PlayerQueue.GetNumOfCardsString();

            var winnerChips = Players.Select(x => x.CardCount).Sum() * ChipsPerCard;
            Send(finalMsg);
            finalMsg = "";
            using (var db = new BigTwoDb())
            {
                foreach (var p in Players)
                {
                    var dbgp = db.GamePlayers.FirstOrDefault(x => x.GameId == GameId && x.PlayerId == p.Id);
                    dbgp.Won = p == Winner;
                    dbgp.CardsLeft = p.CardCount;
                    db.SaveChanges();
                    Thread.Sleep(200);

                    if (PlayChips)
                    {
                        var dbct = new ChipsTransaction
                        {
                            GameId = DbGame.Id,
                            GamePlayerId = dbgp.Id,
                            PlayerId = db.Players.FirstOrDefault(x => x.TelegramId == p.TelegramId).Id,
                            ChipsTransacted = p.CardCount == 0 ? winnerChips : -(p.CardCount * ChipsPerCard)
                        };
                        db.ChipsTransactions.Add(dbct);
                        db.SaveChanges();
                        finalMsg += $"{p.GetName()} [{dbct.ChipsTransacted?.ToString("+#;-#;0").ToBold()}] - {p.CardCount}\n";
                        Thread.Sleep(200);
                    }
                }

                var g = db.Games.FirstOrDefault(x => x.Id == GameId);
                g.TimeEnded = DateTime.UtcNow;
                db.SaveChanges();

            }
            Send(finalMsg);

        }

        public void NotifyNextGamePlayers()
        {
            var grpId = ChatId;
            using (var db = new BigTwoDb())
            {
                var dbGrp = db.Groups.FirstOrDefault(x => x.GroupId == grpId);
                if (dbGrp != null)
                {
                    var toNotify = db.NotifyGames.Where(x => x.GroupId == grpId && x.UserId != Initiator.TelegramId).Select(x => x.UserId).ToList();
                    foreach (int user in toNotify)
                    {
                        Bot.Send(user, GetTranslation("GameIsStarting", GroupLink != null ? $"<a href='{GroupLink}'>{GroupName}</a>" : GroupName), GroupMarkup);
                    }
                    db.Database.ExecuteSqlCommand($"DELETE FROM NotifyGame WHERE GROUPID = {grpId}");
                    db.SaveChanges();
                }
            }
        }

        #endregion


        #region Bot API Related Methods

        public Message Send(string msg, InlineKeyboardMarkup markup = null)
        {
            try
            {
                return Bot.Send(ChatId, msg, markup);
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public Message SendPM(BTPlayer p, string msg, InlineKeyboardMarkup markup = null)
        {
            try
            {
                return Bot.Send(p.TelegramId, msg, markup);
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }

        public Message SendMenu(BTPlayer p, string msg, InlineKeyboardMarkup markup)
        {
            try
            {
                var sent = Bot.Send(p.TelegramId, msg, markup);
                p.CurrentQuestion = new QuestionAsked
                {
                    MessageId = sent.MessageId
                };
                return sent;
            }
            catch (Exception e)
            {
                e.LogError();
                return null;
            }
        }


        public Message Reply(int oldMessageId, string msg)
        {
            return BotMethods.Reply(ChatId, oldMessageId, msg);
        }

        public InlineKeyboardMarkup GenerateStartMe()
        {
            return Helpers.GenerateStartMe(Language);
        }

        /// <summary>
        /// Generate Menu for general card playing
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="cardList">Player's Cards in Hand</param>
        /// <returns>A 2-Column InlineKeyboardMarkup</returns>
        public InlineKeyboardMarkup GenerateMenu(BTPlayer p, List<BTCard> cardList)
        {
            var buttons = new List<Tuple<string, string>>();
            foreach (BTCard card in cardList)
            {
                // buttons.Add(new Tuple<string, string>(card.GetName(), $"{this.Id}|{p.TelegramId}|card|{card.Number.ToString()}"));
            }
            var row = new List<InlineKeyboardButton>();
            var rows = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < buttons.Count; i += 2)
            {
                row.Clear();
                var subButtons = buttons.Skip(i).Take(2).ToList();
                foreach (var button in subButtons)
                    row.Add(new InlineKeyboardCallbackButton(button.Item1, button.Item2));
                rows.Add(row.ToArray());
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        /// <summary>
        /// Generate Menu for player who has to choose a row to keep the cards 
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="tableCards">Table Cards</param>
        /// <returns>A InlineKeyboardMarkup</returns>
        public InlineKeyboardMarkup GenerateMenu(BTPlayer p, List<BTCard[]> tableCards)
        {
            var buttons = new List<Tuple<string, string>>();
            for (int i = 0; i < tableCards.Count; i++)
            {
                /*
                var cardRow = tableCards[i];
                var rowBullsCount = cardRow.Where(x => x != null).Sum(x => x.Bulls);
                var label = $"Row {i + 1}: {rowBullsCount} 🐮";
                buttons.Add(new Tuple<string, string>(label, $"{this.Id}|{p.TelegramId}|row|{i}"));
                */
            }
            var row = new List<InlineKeyboardButton>();
            var rows = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < buttons.Count; i++)
            {
                row.Clear();
                row.Add(new InlineKeyboardCallbackButton(buttons[i].Item1, buttons[i].Item2));
                rows.Add(row.ToArray());
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        public InlineKeyboardMarkup GeneratePlayerDealCardMenu(BTPlayer p)
        {
            var buttons = new List<Tuple<string, string>>();
            var hand = p.Hand;

            var firstRow = new List<InlineKeyboardButton>();
            
            var row = new List<InlineKeyboardButton>();
            var rows = new List<InlineKeyboardButton[]>();

            rows.Add(firstRow.ToArray());
            for (int i = 0; i < buttons.Count; i += 2)
            {
                row.Clear();
                var subButtons = buttons.Skip(i).Take(2).ToList();
                foreach (var button in subButtons)
                    row.Add(new InlineKeyboardCallbackButton(button.Item1, button.Item2));
                rows.Add(row.ToArray());
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        public InlineKeyboardMarkup GetRefreshMarkup(BTPlayer p)
        {
            return new InlineKeyboardMarkup(
                new InlineKeyboardButton[][] {
                    new InlineKeyboardButton[] {
                        new InlineKeyboardCallbackButton(GetTranslation("Refresh"), $"{this.Id.ToString()}|{p.TelegramId}|refresh")
                    }
                }
            );
        }
        #endregion


        #region Incoming Message/Query Handling

        public void HandleMessage(Message msg)
        {
            switch (msg.Text.ToLower().Substring(1).Split()[0].Split('@')[0])
            {
                case "join":
                    if (Phase == GamePhase.Joining)
                        AddPlayer(msg.From);
                    break;
                case "flee":
                    if (Phase == GamePhase.Joining)
                        RemovePlayer(msg.From);
                    else if (Phase == GamePhase.InGame)
                        Send(GetTranslation("CantFleeRunningGame"));
                    break;
                case "startgame":
                    if (Phase == GamePhase.Joining)
                        AddPlayer(msg.From);
                    break;
                case "forcestart":
                    if (this.Players.Count() >= 2) Phase = GamePhase.InGame;
                    else
                    {
                        Send(GetTranslation("GameEnded"));
                        Phase = GamePhase.Ending;
                        Bot.RemoveGame(this);
                    }
                    break;
                case "killgame":
                    Send(GetTranslation("KillGame"));
                    Phase = GamePhase.Ending;
                    Bot.RemoveGame(this);
                    break;
                case "seq":
                    if (_playerList == 0)
                        Reply(msg.MessageId, GetTranslation("PlayerSequenceNotStarted"));
                    else
                        Reply(_playerList, GetTranslation("GetPlayerSequence"));
                    break;
                case "extend":
                    if (Phase == GamePhase.Joining)
                    {
                        _secondsToAdd += Constants.ExtendTime;
                        Reply(msg.MessageId, GetTranslation("ExtendJoining", Constants.ExtendTime));
                    }
                    break;
                case "showdeck":
                    if (Phase == GamePhase.InGame)
                    {
                        var p = Players.FirstOrDefault(x => x.TelegramId == msg.From.Id);
                        p.DeckText = GetTranslation("CardsInHand") + Environment.NewLine + p.Hand.ToList().GetString();
                        if (SendPM(p, p.DeckText, GetRefreshMarkup(p)) != null)
                            msg.Reply(GetTranslation("SentPM"), BotMarkup);
                    }
                    break;

            }

        }

        public void HandleQuery(CallbackQuery query, string[] args)
        {
            try
            {
                // args[0] = GameGuid
                // args[1] = playerId
                // args[2] = "card" or "row"
                // args[2] = cardChosen
                var p = Players.FirstOrDefault(x => x.TelegramId == int.Parse(args[1]));
                _playCardSecondsToAdd += 3;
                switch (args[2])
                {
                    case "card":
                        var chosen = args[3];
                        if (chosen == "dummy")
                            Bot.Api.AnswerCallbackQueryAsync(query.Id);
                        else if (chosen == "skip")
                        {
                            if (CurrentHand.Count == 0) // no cards, you can use any cards
                            {
                                Bot.Api.AnswerCallbackQueryAsync(query.Id, GetTranslation("UseAnything"), true);
                                return;
                            }
                            Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, GetTranslation("Pass"), GroupMarkup);
                            p.UpdateChosenIndexes(empty: true);
                            p.CurrentQuestion = null;
                            return;
                        }
                        else if (chosen == "invalid")
                        {
                            Bot.Api.AnswerCallbackQueryAsync(query.Id, GetTranslation("InvalidHand"), true);
                            return;
                        }
                        else if (chosen == "go")
                        {
                            var chosenCards = p.Hand.Cards.Where(x => p.DealCardMenu.LastValidIndexes.Contains(x.Index)).ToList();
                            if (p.FirstPlayer && !chosenCards.Any(x => x.GameValue == 3 && x.Suit == BTCardSuit.Diamonds))
                            {
                                Bot.Api.AnswerCallbackQueryAsync(query.Id, GetTranslation("MustUseDiamondThree"), true);
                                return;
                            }
                            if (chosenCards.CheckChosenCards() == null)
                            {
                                Bot.Api.AnswerCallbackQueryAsync(query.Id, GetTranslation("PlayCheckAgain"), true);
                                p.DealCardMenu.UpdateMenu();
                                Bot.Api.EditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId, p.DealCardMenu.MarkUp);
                                return;
                            }
                            var res = chosenCards.CompareHandWith(CurrentHand);
                            if (!res.Success)
                            {
                                Bot.Api.AnswerCallbackQueryAsync(query.Id, res.Reason.ToString(), true);
                                return;
                            }
                            var text = chosenCards.GetString();
                            Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, text, GroupMarkup);
                            p.UseCards(chosenCards);
                            CurrentHand = chosenCards.ToList();
                            CurrentHandType = CurrentHand.CheckChosenCards();
                            CurrentDealer = p;
                            p.CurrentQuestion = null;
                            return;
                        }
                        else if (chosen == "reset")
                        {
                            p.UpdateChosenIndexes(empty: true);
                        }
                        else
                        {
                            var indexChosen = int.Parse(args[3]);
                            p.UpdateChosenIndexes(indexChosen);

                            var cardChosen = p.Hand.FirstOrDefault(x => x.Index == indexChosen);
                        }
                        Bot.Api.EditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId, p.DealCardMenu.MarkUp);

                        break;
                    case "refresh":
                        if (Phase == GamePhase.InGame)
                        {
                            var txt = GetTranslation("CardsInHand") + Environment.NewLine + p.Hand.ToList().GetString();
                            if (txt != p.DeckText)
                            {
                                p.DeckText = txt;
                                Bot.Edit(query.Message.Chat.Id, query.Message.MessageId, txt, GetRefreshMarkup(p));
                            }
                            else
                            {
                                BotMethods.AnswerCallback(query, GetTranslation("CardsNoChange"), true);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Bot.Send(query.From.Id, e.Message + e.StackTrace);
            }
        }

        #endregion


        #region Language Related

        public void LoadLanguage(string language)
        {
            try
            {
                var files = Directory.GetFiles(Constants.GetLangDirectory());
                var file = files.First(x => Path.GetFileNameWithoutExtension(x) == language);
                {
                    var doc = XDocument.Load(file);
                    Locale = new Locale
                    {
                        Language = Path.GetFileNameWithoutExtension(file),
                        XMLFile = doc,
                        LanguageName = doc.Descendants("language").FirstOrDefault().Attribute("name").Value
                    };
                }
                Language = Locale.Language;
            }
            catch
            {
                if (language != "English")
                    LoadLanguage("English");
            }
        }

        private string GetTranslation(string key, params object[] args)
        {
            try
            {
                var strings = Locale.XMLFile.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key) ??
                    Program.English.XMLFile.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key);
                if (strings != null)
                {
                    var values = strings.Descendants("value");
                    var choice = Helpers.RandomNum(values.Count());
                    var selected = values.ElementAt(choice).Value;

                    return String.Format(selected, args).Replace("\\n", Environment.NewLine);
                }
                else
                {
                    throw new Exception($"Error getting string {key} with parameters {(args != null && args.Length > 0 ? args.Aggregate((a, b) => a + "," + b.ToString()) : "none")}");
                }
            }
            catch (Exception e)
            {
                try
                {
                    //try the english string to be sure
                    var strings =
                        Program.English.XMLFile.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key);
                    var values = strings?.Descendants("value");
                    if (values != null)
                    {
                        var choice = Helpers.RandomNum(values.Count());
                        var selected = values.ElementAt(choice).Value;
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return String.Format(selected, args).Replace("\\n", Environment.NewLine);
                    }
                    else
                        throw new Exception("Cannot load english string for fallback");
                }
                catch
                {
                    throw new Exception(
                        $"Error getting string {key} with parameters {(args != null && args.Length > 0 ? args.Aggregate((a, b) => a + "," + b.ToString()) : "none")}",
                        e);
                }
            }
        }

        #endregion

        #region General Methods

        public void Dispose()
        {
            Players?.Clear();
            Players = null;
            Deck = null;
            // MessageQueueing = false;
        }

        public void Log(Exception ex)
        {
            ex.LogError(ChatId);
            Send("Sorry there is some problem with me, I gonna go die now.");
            this.Phase = GamePhase.Ending;
            Bot.RemoveGame(this);
        }

        #endregion

        #region Constants

        public enum GamePhase
        {
            Joining, InGame, Ending, KillGame
        }

        #endregion
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PokerServer.Core;
using PokerServer.Model;
using System.Collections.Generic;

namespace PokerServerUnitTests
{
    [TestClass]
    public class HandEvaluatorTests
    {
        private static HandEvaluator _evalautor = new HandEvaluator();

        [TestMethod]
        public void TestRoyalFlush()
        {
        }

        [TestMethod]
        public void TestFlush()
        {
            var cards = new List<Card>()
            {
                new Card(CardSuit.Clubs, 2),
                new Card(CardSuit.Clubs, 6),
                new Card(CardSuit.Clubs, 8),
                new Card(CardSuit.Clubs, 11),
                new Card(CardSuit.Clubs, 9),
                new Card(CardSuit.Diamonds, 3),
                new Card(CardSuit.Diamonds, 11)
            };
            Assert.IsTrue(_evalautor.IsFlush(cards), "Hand was not a flush.");
        }

        [TestMethod]
        public void TestStraight()
        {
            var cards = new List<Card>()
            {
                new Card(CardSuit.Spades, 10),
                new Card(CardSuit.Diamonds, 9),
                new Card(CardSuit.Spades, 7),
                new Card(CardSuit.Clubs, 6),
                new Card(CardSuit.Hearts, 8),
                new Card(CardSuit.Hearts, 1),
                new Card(CardSuit.Clubs, 11)
            };
            Assert.IsTrue(_evalautor.IsStraight(cards), "Hand was not a straight.");
        }
    }
}

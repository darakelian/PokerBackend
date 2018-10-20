using HoldemHand;
using PokerServer.Core;
using PokerServer.Model;
using System;

namespace PokerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var table = new Table();
            for (var i = 0; i < 4; i++)
                table.AddPlayer(new Player());
            table.StartGame();
            table.Flop();
            table.TurnOrRiver();
            table.TurnOrRiver();
            var winners = table.CalculateWinners();
            Console.WriteLine($"Table was {table.Board}");
            Console.WriteLine("Winners hand:");
            foreach (var winner in winners)
                Console.WriteLine(new Hand(winner.Hand, table.Board).Description);
            Console.WriteLine("Other hands were:");
            foreach (var player in table.Players)
                if (!winners.Contains(player))
                    Console.WriteLine(player.Hand);
            Console.Read();
        }
    }
}

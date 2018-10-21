using PokerServer.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PokerServer.Network
{
    namespace ServerPackets
    {
        struct LoginResponse
        {
            public byte PacketId;
            public byte LoginStatus;
            public int UserId;
        }

        struct InitialGameStatePacket
        {
            public byte PacketId;
            public int InitialChips;
            public char Rank1;
            public char Rank2;
            public char Suit1;
            public char Suit2;
            public int Pot;
            public byte NumPlayers;
            public int[] PlayersChips;
            public int[] PlayerIds;
        }

        struct UpdateFlopCards
        {
            public byte PacketId;
            public char[] Ranks;
            public char[] Suits;
        }

        struct UpdateTurnOrRiverCard
        {
            public byte PacketId;
            public char Rank;
            public char Suit;
        }

        struct MarkActivePlayer
        {
            public byte PacketId;
            public int UserId;
            public int HighestBid;
            public int PersonalBid;
        }

        struct UpdatePlayerChipCount
        {
            public byte PacketId;
            public int UserId;
            public int NewChipCount;
            public int NewPotCount;
        }

        struct UpdateWinners
        {
            public byte PacketId;
            public int[] WinnerIds;
            public int[] NewWinnerPots;
        }

        struct GameMessagePacket
        {
            public byte PacketId;
            public string Message;
        }
    }

    /// <summary>
    /// Packets sent by the client.
    /// 0 - Login
    /// 1 - Bet
    /// 2 - Check/Fold
    /// </summary>
    namespace ClientPackets
    {
        /// <summary>
        /// Represents a packet for "logging in". If a user ID of -1 is sent,
        /// that indicates the client is requesting a new account.
        /// </summary>
        struct LoginRequest
        {
            /// <summary>
            /// Packet ID.
            /// </summary>
            public byte PacketId;

            /// <summary>
            /// User ID of person making the request. Set to -1 if an ID is needed.
            /// </summary>
            public int UserId;
        }

        /// <summary>
        /// Represents a packet from the client when the user bets. A bet will
        /// also encompass a call for simplicity's sake despite technically
        /// being different actions.
        /// </summary>
        struct PlaceBetRequest
        {
            /// <summary>
            /// Packet ID.
            /// </summary>
            public byte PacketId;

            /// <summary>
            /// User ID of person making request.
            /// </summary>
            public int UserId;

            /// <summary>
            /// Amount being bet. Will either be enough to call or greater to
            /// indicate a raise.
            /// </summary>
            public uint BetAmount;
        }

        /// <summary>
        /// Represents a packet from the client that is a simple user interaction
        /// with the client that requires no more information than the packet ID.
        /// Examples include checking or folding.
        /// </summary>
        struct GeneralActionRequest
        {
            /// <summary>
            /// Packet ID.
            /// </summary>
            public byte PacketId;

            /// <summary>
            /// User ID of person making request.
            /// </summary>
            public int UserId;

            /// <summary>
            /// The action being taken (either fold or check here).
            /// </summary>
            public GeneralAction Action;
        }
    }
}

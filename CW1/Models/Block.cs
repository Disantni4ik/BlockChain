using System;
using System.Collections.Generic;
using System.Text;

namespace CW1.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public long Nonce { get; set; }
        public double MiningDuration { get; set; }
        public int Difficulty { get; set; }
        public int Size { get; set; }
        public string DiffucultyText { get; set; }

        public Block(int index, DateTime timeStamp, List<Transaction> transactions, string previousHash, int difficulty)
        {
            Index = index;
            TimeStamp = timeStamp;
            Transactions = transactions;
            Difficulty = difficulty;
            PreviousHash = previousHash;
            Hash = string.Empty;
            MiningDuration = 0;
        }

        public Block Copy()
        {
            return new Block(Index, TimeStamp, Transactions, PreviousHash, Difficulty)
            {
                Hash = this.Hash,
                Nonce = this.Nonce
            };
        }
    }
}

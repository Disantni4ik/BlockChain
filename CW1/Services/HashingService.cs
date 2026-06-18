using CW1.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CW1.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            var totalHash = "";
            foreach (var transaction in block.Transactions)
            {
                totalHash += ComputeHash(transaction.ToRawString());
            }

            string blockData = $"{block.Index}{block.TimeStamp.ToString("O")}{totalHash}{block.PreviousHash}{block.Nonce}";
            return ComputeHash(blockData);
        }
        private string ComputeHash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}

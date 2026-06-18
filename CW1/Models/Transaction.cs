using CW1.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CW1.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }

        public Transaction(string from, string to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.UtcNow;
            var rawString = ToRawString();
            byte[] bytes = Encoding.UTF8.GetBytes(rawString);
            byte[] hashBytes = SHA256.HashData(bytes);
            Id = Convert.ToBase64String(hashBytes);
        }
        public string ToRawString()
        {
            return $"From:{From},To:{To},Amount:{Amount}";
        }

    }

}

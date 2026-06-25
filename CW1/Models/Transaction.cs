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
        public decimal Fee { get; set; }
        public byte[] SenderPublicKey { get; set; }
        public byte[] Signature { get; set; }
        public Transaction()
        {
            
        }
        public Transaction(string from, string to, decimal amount, byte[] senderPublicKey)
        {
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.UtcNow;
            SenderPublicKey = senderPublicKey;
            var rawString = $"From:{From},To:{To},Amount:{Amount},TimeStamp:{TimeStamp:O}";
            byte[] bytes = Encoding.UTF8.GetBytes(rawString);
            byte[] hashBytes = SHA256.HashData(bytes);
            Id = Convert.ToBase64String(hashBytes);
        }
        public string ToRawString()
        {
            if (Signature == null)
            {
                return $"From:{From},To:{To},Amount:{Amount},TimeStamp:{TimeStamp:O}";
            }
            return $"From:{From},To:{To},Amount:{Amount},TimeStamp:{TimeStamp:O},Fee:{Fee} {Convert.ToHexString(Signature)}";
        }
        public byte[] GetDataToSign()
        {
            string raw = $"{Id}{From}{To}{Amount}{TimeStamp:O}{Fee}";
            return Encoding.UTF8.GetBytes(raw);
        }
    }

}

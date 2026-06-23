using CW1.Models;
using CW1.Services;

BlockChainService blockchain = new BlockChainService();
WalletService walletService = new WalletService(blockchain.Chain);

string minerAddress = "0x1234567890abcdef1234567890abcdef12345678";
string userA = "0xaaaa567890abcdef1234567890abcdef123456aa";
string userB = "0xbbbb567890abcdef1234567890abcdef123456bb";
string invalidUser = "Bob";

//#1
Transaction badTx = new Transaction(userA, invalidUser, 10, new byte[0]);
var validationResult = blockchain.FindBlockByHash("") == null;

List<Transaction> testBadList = new List<Transaction> { badTx };
blockchain.ProcessTransactions(testBadList, minerAddress);

//#2
List<Transaction> incomingTransactions = new List<Transaction>();

for (int i = 1; i <= 15; i++)
{
    Transaction tx = new Transaction("COINBASE", userB, i * 2, new byte[0]);
    incomingTransactions.Add(tx);
}
blockchain.ProcessTransactions(incomingTransactions, minerAddress);

Console.WriteLine(blockchain.Chain.Count);
using CW1.Models;
using CW1.Services;

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();

var blockchainService = new BlockChainService();
var transactionService = new TransactionService();

var transactions = new List<Transaction>();

for (int i = 1; i < 10; i++)
{
    var transaction = transactionService.CreateTransaction($"UserTestTesttest{i}", $"UserTest{i + 1}", i * 10);
    transactions.Add(transaction);
}


while (true)
{
    Console.WriteLine("Menu:");
    Console.WriteLine("1. Add Block");
    Console.WriteLine("2. Print BlockChain");
    Console.WriteLine("3. Validate BlockChain");
    Console.WriteLine("4. Add Transaction");
    Console.WriteLine("5. Exit");
    Console.Write("Choose an option: ");
    var choice = Console.ReadLine();

    switch (choice) 
    {
        case "1":
            blockchainService.AddBlock(transactions);
            Console.WriteLine("Block added successfully.");
            break;
        case "2":
            displayService.PrintBlockChain(blockchainService.Chain);
            break;
        case "3":
            bool isValid = blockchainService.IsValid();
            displayService.PrintValidationResult(isValid);
            break;
    }
}
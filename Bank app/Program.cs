// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using System.IO;

namespace BankApp
{
    class Program
    {
        static List<Account> accounts = new List<Account>();
        static Random random = new Random();

        static void Main(string[] args)
        {

            // Load accounts
            if (File.Exists("account.json"))
            {
                string jsonString = File.ReadAllText("account.json");
                var loadedAccounts = JsonSerializer.Deserialize<List<Account>>(jsonString);

                if (loadedAccounts != null)
                {
                    // Filter out invalid accounts
                    accounts = loadedAccounts.FindAll(acc =>
                        !string.IsNullOrWhiteSpace(acc.GetName()) &&
                        !string.IsNullOrWhiteSpace(acc.GetAccountNumber()));
                }
            }

            Console.Write("Enter your name to create an account: ");
            string name = Console.ReadLine();
            Account account = CreateAccount(name);

            Console.WriteLine("\nAccount created successfully!");
            DisplayAllAccounts();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Deposit\n2. Withdraw\n3. Transfer\n4. View Balance\n5. View All Accounts\n6. Exit");
                Console.Write("Choice: ");

                string input = Console.ReadLine();
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("Invalid choice. Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Enter deposit amount: ");
                        if (double.TryParse(Console.ReadLine(), out double depositAmount))
                        {
                            try
                            {
                                account.Deposit(depositAmount);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount entered.");
                        }
                        break;

                    case 2:
                        Console.Write("Enter withdraw amount: ");
                        if (double.TryParse(Console.ReadLine(), out double withdrawAmount))
                        {
                            try
                            {
                                account.Withdraw(withdrawAmount);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount entered.");
                        }
                        break;

                    case 3:
                        Console.Write("Enter recipient account number: ");
                        string toAccNo = Console.ReadLine();
                        Account toAccount = accounts.Find(acc => acc.GetAccountNumber() == toAccNo);

                        if (toAccount == null)
                        {
                            Console.WriteLine("Recipient account not found.");
                            break;
                        }

                        Console.Write("Enter transfer amount: ");
                        if (double.TryParse(Console.ReadLine(), out double transferAmount))
                        {
                            try
                            {
                                AccountManager manager = new AccountManager(account, toAccount, transferAmount);
                                manager.FundTransfer();
                                Console.WriteLine($"Transferred {transferAmount} from {account.GetAccountNumber()} to {toAccount.GetAccountNumber()}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount entered.");
                        }
                        break;

                    case 4:
                        Console.WriteLine($"Balance: {account.GetBalance()}");
                        break;

                    case 5:
                        DisplayAllAccounts();
                        break;

                    case 6:
                        exit = true;
                        Console.WriteLine("Exiting application...");
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }


        public static Account CreateAccount(string name, int balance = 0)
        {
            string accNumber;
            do
            {
                accNumber = GeneratedAccountNumber();
            } while (accounts.Exists(acc => acc.GetAccountNumber() == accNumber));

            Account account = new Account(name, accNumber, balance);
            accounts.Add(account);
            // Save to json
            string jsonString = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("account.json", jsonString);
            return account;
        }

        public static string GeneratedAccountNumber()
        {
            int randomValue = random.Next(300, 1000);
            return randomValue.ToString();
        }

        public static void DisplayAllAccounts()
        {
            Console.WriteLine("\n--- All Accounts ---");
            foreach (Account acc in accounts)
            {
                Console.WriteLine(acc);
            }
        }
    }

    class Account
    {
        private string Name { get; set; }
        private string AccountNumber { get; set; }
        private double Balance { get; set; }

        public Account(string name, string accountNumber, int balance)
        {
            Name = name;
            AccountNumber = accountNumber;
            Balance = balance;
        }

        public Account() { }

        public string GetName()
        {
            return Name;
        }

        public string GetAccountNumber()
        {
            return AccountNumber;
        }

        public double GetBalance()
        {
            return Balance;
        }

        public void Deposit(double amount)
        {
            if (amount < 0)
            {
                throw new Exception("You cannot deposit a negative amount.");
            }
            Balance += amount;
            Console.WriteLine($"{amount} successfully deposited in account.");
        }

        public void Withdraw(double amount)
        {
            if (amount < 0)
            {
                throw new Exception("You cannot withdraw a negative amount.");
            }
            if (amount > Balance)
            {
                throw new Exception("Insufficient funds.");
            }
            Balance -= amount;
            Console.WriteLine($"{amount} successfully withdrawn from account.");
        }

        public override string ToString()
        {
            return $"Name: {Name}, AccNo: {AccountNumber}, Balance: {Balance}";
        }
    }

    class AccountManager
    {
        private Account FromAccount;
        private Account ToAccount;
        private double TransferAmount;

        public AccountManager(Account accountFrom, Account accountTo, double amountTransfer)
        {
            FromAccount = accountFrom;
            ToAccount = accountTo;
            TransferAmount = amountTransfer;
        }

        public void FundTransfer()
        {
            lock (FromAccount)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} acquired lock on {FromAccount.GetAccountNumber()}");

                lock (ToAccount)
                {
                    FromAccount.Withdraw(TransferAmount);
                    ToAccount.Deposit(TransferAmount);
                }
            }
        }
    }
}

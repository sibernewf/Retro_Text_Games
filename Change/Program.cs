using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("I, YOUR FRIENDLY COMPUTER, WILL DETERMINE");
        Console.WriteLine("THE CORRECT CHANGE FOR ITEMS COSTING UP TO $100.");
        Console.WriteLine();

        while (true)
        {
            // --- Read COST ---
            double cost;
            while (true)
            {
                Console.Write("COST OF ITEM? ");
                var input = Console.ReadLine() ?? string.Empty;
                if (double.TryParse(input, out cost) && cost >= 0 && cost <= 100)
                    break;

                Console.WriteLine("Invalid cost. Please enter a value between 0 and 100.");
            }

            // --- Read PAYMENT ---
            double payment;
            while (true)
            {
                Console.Write("AMOUNT OF PAYMENT? ");
                var input = Console.ReadLine() ?? string.Empty;
                if (!double.TryParse(input, out payment))
                {
                    Console.WriteLine("Invalid amount. Try again.");
                    continue;
                }

                if (payment < cost)
                {
                    Console.WriteLine("SORRY, YOU HAVE SHORT CHANGED ME.");
                    Console.WriteLine();
                    // restart whole flow for a new transaction
                    goto AskAgain;
                }

                break;
            }

            // --- Compute change (in cents to avoid FP issues) ---
            double rawChange = payment - cost;
            int cents = (int)Math.Round(rawChange * 100, MidpointRounding.AwayFromZero);
            double changeDisplay = cents / 100.0;

            Console.WriteLine($"YOUR CHANGE IS ${changeDisplay:F2}");

            // --- Denominations ---
            int tens = cents / 1000; cents %= 1000;
            int fives = cents / 500; cents %= 500;
            int ones = cents / 100; cents %= 100;
            int halfDollars = cents / 50; cents %= 50;
            int quarters = cents / 25; cents %= 25;
            int dimes = cents / 10; cents %= 10;
            int nickels = cents / 5; cents %= 5;
            int pennies = cents;

            if (tens > 0) Console.WriteLine($"{tens} TEN DOLLAR BILL(S)");
            if (fives > 0) Console.WriteLine($"{fives} FIVE DOLLAR BILL(S)");
            if (ones > 0) Console.WriteLine($"{ones} ONE DOLLAR BILL(S)");
            if (halfDollars > 0) Console.WriteLine($"{halfDollars} ONE-HALF DOLLAR(S)");
            if (quarters > 0) Console.WriteLine($"{quarters} QUARTER(S)");
            if (dimes > 0) Console.WriteLine($"{dimes} DIME(S)");
            if (nickels > 0) Console.WriteLine($"{nickels} NICKEL(S)");
            if (pennies > 0) Console.WriteLine($"{pennies} PENNY(S)");

            Console.WriteLine("THANK YOU, COME AGAIN");
            Console.WriteLine();

        AskAgain:
            Console.Write("Would you like to calculate another change? (Y/N): ");
            string again = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (again != "Y")
                break;

            Console.WriteLine();
        }
    }
}

CHANGE – Player Documentation
Version: C# Console Edition
Original Author: Dennis Lunder – People's Computer Co., Menlo Park, CA
C# Conversion: 2025 Edition

Overview
In CHANGE, your computer acts as the cashier at a friendly candy store.
You tell it:

The cost of the item(s) you’re buying.

The amount you’re paying.

The computer then calculates the exact change you should receive and tells you the breakdown in bills and coins.

Objective
Provide the program with valid purchase and payment amounts to see how much change you will get, along with the exact denomination breakdown.

How to Play
Start the Program
Run the game. You will see a greeting:

I, YOUR FRIENDLY COMPUTER, WILL DETERMINE
THE CORRECT CHANGE FOR ITEMS COSTING UP TO $100.
Enter the Cost of Item(s)

When prompted with:

COST OF ITEM?
Type the total price (e.g., 4.59) and press Enter.

The price must be greater than 0 and up to 100 dollars.

Enter the Amount of Payment

When prompted with:

AMOUNT OF PAYMENT?
Enter the amount of money you’re paying (e.g., 10) and press Enter.

If the amount entered is less than the cost, the program will say:
SORRY, YOU HAVE SHORT CHANGED ME.
and ask you again.

View Your Change
The program displays:

The total change owed (in dollars and cents).

A list of bills and coins making up that change.
Example:

YOUR CHANGE IS $5.41
1 FIVE DOLLAR BILL(S)
1 QUARTER(S)
1 DIME(S)
1 NICKEL(S)
1 PENNY(S)
Play Again or Quit
After each transaction, you’ll be asked:

Would you like to calculate another change? (Y/N):
Type Y and press Enter to start another round.

Type N to exit.

Tips
Enter numbers using standard decimal format (e.g., 7.25).

The game rounds to the nearest cent for accuracy.

The breakdown will only show denominations you actually receive.

Works for any amount up to $100.

Example Game Session
I, YOUR FRIENDLY COMPUTER, WILL DETERMINE
THE CORRECT CHANGE FOR ITEMS COSTING UP TO $100.

COST OF ITEM? 4.59
AMOUNT OF PAYMENT? 10
YOUR CHANGE IS $5.41
1 FIVE DOLLAR BILL(S)
1 QUARTER(S)
1 DIME(S)
1 NICKEL(S)
1 PENNY(S)
THANK YOU, COME AGAIN

Would you like to calculate another change? (Y/N): N
To Play
Double-click run.bat in the game’s folder. This will build and run the game, and keep the window open when it finishes.

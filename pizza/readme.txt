Pizza Delivery – Hyattsville

Objective:
Take pizza orders from residents of Hyattsville and direct the delivery driver to the correct house coordinates on the map. Deliver pizzas accurately and as quickly as possible.

🗺 The City Map

The city is laid out as a 4×4 grid.

Houses are labelled A through P in row-major order.

Row 1 is the bottom row, Column 1 is the leftmost column.

Coordinates are entered as ROW,COL (numbers 1–4).

Example:
If the map shows A in the bottom-left corner, its coordinate is 1,1.

🎮 How to Play

A random house will “call” and order a pizza.

You must tell the delivery driver where that house is located.

Enter the coordinates in the form:

ROW,COL


If you are wrong, the person at that location will tell you they didn’t order pizza, and you’ll need to try again for the same customer.

When you get it right, the customer thanks you, and you move to the next order.

You can play until you type QUIT.

📖 Commands

While taking orders, you can type:

MAP – Show the city map again.

HELP – Show directions and coordinate rules.

QUIT – End the game and see where the log file is saved.

📝 Logging

Every order and every delivery attempt is recorded in a log file in the program’s folder.

The log filename is in the format:

pizza-log-YYYYMMDD-HHMMSS.txt


This allows you to review your performance and mistakes after the game.

🏆 Tips

Study the map at the start and memorize letter positions.

Think of the grid as a coordinate system — row first (bottom to top), column second (left to right).

Minimize wrong attempts to be more “efficient” (especially if we add cold pizza rules later!).
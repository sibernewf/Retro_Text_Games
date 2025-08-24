MNOPLY — Monopoly for Two
Player Doco (Updated)
Description
MNOPLY is a simulation of the classic board game Monopoly, designed for two players.
The computer enforces the rules, handles the money, and manages property ownership.
The new version adds player commands that let you check your cash, list owned properties, see your current status, and more — at any point during your turn before you roll the dice.

Starting the Game
When the game starts, you’ll be asked:

nginx
Copy
Edit
WHO IS PLAYER #1?
WHO IS PLAYER #2?
Type in your names (or nicknames) and press Return.
Player #1 always goes first.

Turn Sequence
At the start of your turn, you’ll see:

rust
Copy
Edit
==> YOURNAME'S TURN <==
Type ROLL (or HELP) >
Before you roll, you may enter any of the commands listed in Available Commands below.

When ready, type ROLL to roll the dice and move your token.

The computer tells you what space you landed on and what happens next (buy, pay rent, draw card, etc.).

If you land on a property that’s for sale, you’ll be prompted:

nginx
Copy
Edit
BUY <NAME> FOR $XXX?  Type BUY or PASS >
Type BUY to purchase it.

Type PASS to skip.

If you land on an opponent’s property, you’ll automatically pay rent.

After movement and transactions, you may be prompted:

vbnet
Copy
Edit
IF YOU WANT TO IMPROVE YOUR PROPERTY TYPE HOUSE >
Type HOUSE to build houses/hotels (must have a monopoly in that color group).

Press Return to skip.

Available Commands (Before You Roll)
You can type these at the Type ROLL (or HELP) > prompt:

Command	What It Does
HELP or ?	Lists all available commands.
CASH or MONEY	Shows how much money you currently have.
OWN or PROPS	Lists all properties, railroads, and utilities you own, with house/hotel/mortgage status.
STATUS	Shows your cash, current board position, Get Out of Jail Free cards, and asset counts.
WHERE	Tells you the name of the space you are currently on.
MORTGAGE	Lets you voluntarily mortgage properties to raise cash (only works if the property has no houses/hotel).

You can enter multiple commands before rolling — for example:

pgsql
Copy
Edit
Type ROLL (or HELP) > CASH
AL HAS $1270
Type ROLL (or HELP) > OWN
-- AL OWNS --
  BALTIC AVE [Brown]
Type ROLL (or HELP) > ROLL
Game Rules and Notes
Bankruptcy: If you cannot pay what you owe and cannot mortgage enough property to cover it, you lose. The other player wins.

Passing GO: You collect $200 each time you pass GO.

Mortgages: You can mortgage property for half its purchase price. You cannot mortgage if the property has houses/hotel, and all properties in a color group must be unmortgaged before you can build again.

Getting Out of Jail: Pay $50 at the start of your turn or use a Get Out of Jail Free card.

Rent Doubling: Owning all properties in a color group doubles base rent (when unimproved).

Monopolies & Building: You can only build houses/hotels when you own all properties in a color group.

Computer Limitations
Only two players are supported.

The computer handles all rent and transaction calculations automatically.

The game is text-based; no board graphics are shown.

Auctions are not implemented; passing on a property simply leaves it unowned.
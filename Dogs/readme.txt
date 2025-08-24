Player Documentation — DOGS (Dog Race)
Genre: Betting sim / arcade race
Players: 1–19 bettors (typed one after another)
Goal: Pick a dog and place a bet. If your dog wins, you split the total pool with other winners.

How a Race Works
The track posts each dog’s past record (Wins / Losses). Records persist between runs.

Up to 19 people may place a bet:

Dog Number: 1–10

Bet: $2.00–$500.00

The track posts odds (simple form-odds derived from prior W/L) — informative only.

The race runs (winner is biased by historical performance).

Payout (pari-mutuel): All money in the pool is split among the people who chose the winning dog, proportional to their bet.

Example: Pool = $1,863. Two winners bet $300 and $200 on the same dog.
Payouts: $1,863 × (300/500) = $1,118 and $1,863 × (200/500) = $745.

Controls & Input
Enter names, dog numbers (1–10), and bet amounts when prompted.

Type 1 for YES or 0 for NO to run another race.

Tips
Dogs with many wins tend to win again (they’re favorites).

Long-shot dogs have worse records; they win less often but can pay big if few people bet on them.

Watching posted W/L over multiple days makes the track feel “alive.”

How to Run
Windows: To play, double-click run.bat in the game’s folder. This will build and run the game, and keep the window open when it finishes.

macOS/Linux:

 dotnet build ./src && dotnet ./src/bin/Debug/net9.0/DogsDogRace.dll

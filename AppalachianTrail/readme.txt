Appalachian Trail — Player Guide
Quick Start

To play, double-click run.bat in the game’s folder. This will build and run the game, and keep the window open when it finishes.

Alternatively:

Windows: double-click AppalachianTrail.exe (if present), or open a terminal in the folder and run dotnet run.

macOS/Linux: open a terminal in the folder and run dotnet run (requires .NET installed) or run the published binary for your OS if included.

What is this?

A console adventure/simulator inspired by the classic 1986 BASIC game by David H. Ahl. Your goal: hike all 2,007 miles of the Appalachian Trail from Georgia to Maine before winter shuts you down.

Game Flow

Setup & Instructions
You’ll see a short intro, then enter basic info (sex, weight), fitness level, walking pace (mph), daily hours, poison ivy sensitivity, and your attitude toward rain.

Gear Selection
Choose one option in each category (Tent, Pack, Sleeping Bag, Pad, Stove, Boots, Raingear). The game checks total weight, cost, and pack volume. If your pack is too small for all items, you’ll be asked to rechoose.

Extras
Pick how many underwear changes to carry, and whether to bring a walking stick.

Food Strategy & Diet
Select food supply method (caches, post offices, or stores/restaurants) and assign diet percentages across five food groups totaling 100%. Then choose your daily calorie intake.

Hike Loop (every ~3 days)
The game advances in 3-day segments:

Shows the date (starting April 1), total miles walked, and nearby landmarks.

Applies pace, weight, weather, and possible mishaps (blisters, rain, snow, etc.).

Occasionally asks if you want to adjust pace/hours.

You’ll face events like New England snow and the Kennebec River crossing.

Finish or Fail
Reach Baxter Peak (Mt. Katahdin) to win, or get forced off the trail by snow, injury, or decision. You’ll get a final summary (days, miles, avg miles/day, weight change). Then choose to play again.

Controls

Prompts: Type your answer and press Enter.

Yes/No: Enter Y or N (case-insensitive). Empty input defaults to Y.

Numeric choices: Enter the number shown (e.g., 1–4 for gear).

Decimal inputs: Use . for decimals (e.g., 3.0).

Diet percentages: Enter five numbers that sum to 100.

Key Mechanics (What matters)

Pace & Hours: Higher mph or more hours increases distance but also burns more calories and raises risk.

Weight: Heavier gear/food slows you down and increases daily calorie burn.

Food Strategy: Using stores/restaurants slightly reduces trail efficiency vs caches, but offers flexibility.

Weather & Mishaps: Rain, snow, blisters, sprains, gear failures, and river crossings can reduce speed or add downtime.

Seasonal Deadline: Starting in April, you must reach Maine before November snow ends your run.

Tips

Keep daily miles reasonable early on; your fitness gating will cap progress until you’ve built up miles.

Don’t starve yourself—CAL too low vs CD (calories burned) risks hypothermia/starvation events.

A walking stick helps with certain encounters and can reduce risk (e.g., dogs, stability).

Don’t overpack bulky gear—pack volume must fit everything plus food & clothing.

Troubleshooting

Window closes immediately: Use run.bat or run from a terminal so you can read output.

“Input invalid” loops: Enter values in the shown range; for diet, ensure the five numbers sum to 100.

Weird characters: Use a terminal with UTF-8 if possible (Windows Terminal / PowerShell is fine).

System Requirements

Windows 10/11, macOS, or Linux.

A recent .NET runtime if you run via dotnet run. If you use the included executable, .NET may not be required.

Credits & Acknowledgements

Original concept © David H. Ahl (1986).

This edition is a C# console port inspired by the original BASIC program.

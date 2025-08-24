Adding Boards:
----------------
Where to store gameboards
Create a folder next to your executable (or next to your .csproj) called Boards.

Put each board in its own JSON file, e.g.:
 - Boards/classic.json (the board you already have)
 - Boards/sydney.json
 - Boards/christchurch.json

On startup, the game:
 - Lists all *.json files in: \bin\Debug\net9.0\Boards
 - Prompts the player to choose one
 - If the player just presses Enter, it loads classic.json
 - If Boards/ is missing or classic.json doesn’t exist, it will fall back to the built-in classic board (so the game still runs).

Tip: Commit Boards/ to your repo. You can ship multiple regional boards without touching the code again.

Monopoly theme ideas you could turn into JSON boards for your system — from real-world official sets to fun fan-made concepts:

Real-world Locations
 - Tokyo Monopoly – Shinjuku, Shibuya, Ginza, Akihabara.
 - Christchurch Monopoly – Hagley Park, Cathedral Square, Lyttelton.
 - Las Vegas Monopoly – Casinos, resorts, famous streets.

Pop Culture & Media
 - Harry Potter Monopoly – Diagon Alley, Hogsmeade, Hogwarts Houses.
 - Lord of the Rings Monopoly – Shire, Rivendell, Mordor.
 - Star Trek Monopoly – Planets, starships, sectors.
 - Marvel Monopoly – Avengers Tower, Wakanda, Stark Industries.
 - DC Comics Monopoly – Gotham, Metropolis, Themyscira.
 - Disney Monopoly – Rides, castles, lands from Disney parks.

Historical & Educational
 - Ancient Civilizations Monopoly – Pyramids, Colosseum, Great Wall.
 - World War II Monopoly – Major battles, cities, leaders (educational slant).
 - Space Race Monopoly – Apollo missions, space stations, Mars colony.

Fictional or Fun Fan-Made
 - Cyberpunk Monopoly – Neon districts, megacorp HQs, hacker hideouts.
 - Medieval Monopoly – Castles, villages, jousting arenas.
 - Zombie Apocalypse Monopoly – Safe zones, supply caches, zombie hordes.
 - Steampunk Monopoly – Airships, clockwork factories, steam stations.

Niche / Hobby Themes
 - Sports Monopoly – Stadiums, famous players, championship events.
 - Video Game Monopoly – Mario, Zelda, Minecraft, Halo, etc.
 - Music Monopoly – Iconic venues, albums, and artists.
 - Food Monopoly – Restaurants, cuisines, famous chefs.
 - Travel Monopoly – Airports, landmarks, resorts.


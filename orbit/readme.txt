ORBIT — Destroy an Orbiting Enemy Spaceship
Player Doco
Goal

Hit a cloaked enemy ship that’s flying a circular, counter-clockwise orbit around your planet.
You have 7 shots (hours). A bomb destroys the ship if it explodes within 5,000 miles of it.

The World (what you can assume)

The ship’s orbit radius (distance from the center) is constant, between 10,000–30,000 miles.

The ship completes one revolution every 12–36 hours (constant angular speed).

You don’t see the ship on the radar; after each shot you’re told how far your bomb was from it.

You choose the angle and radius for each bomb.

Angle & Radius

Angle 0° points east (to the right). Angles increase counter-clockwise:

90° = north, 180° = west, 270° = south.

Radius is the distance from the planet’s center (the radar’s “+”).

Bomb radius must be between 1,000–30,000 miles.

Controls / Input

Each “hour” (turn) you’ll be prompted for:

ANGLE (0..360) — where to aim, in degrees.

RADIUS (1000..30000) — how far from the center to detonate, in miles.

The game then:

Explodes the bomb at your requested point,

Prints the distance to the hidden ship,

Draws an ASCII radar with your bomb marked *,

Moves time forward one hour (the ship advances along its orbit).

Reading the Radar

Vertical | and horizontal — lines mark the axes; center is +.

Concentric dotted rings are every 5,000 miles.

Your last bomb is shown with * at your chosen angle/radius.

The ship is not shown—only its distance to your bomb is revealed.

Winning & Losing

Win: any bomb comes within 5,000 miles of the ship → “DIRECT HIT!”

Lose: all 7 shots miss → the ship escapes.

Hints & Strategy

Treat it like polar coordinate bracketing:

First, estimate the radius (pick a ring, e.g., 20,000 mi); then probe angles to learn angular position.

If you get a small distance at angle A, try nearby angles (A±Δ) at the same radius to refine the angle.

Use the reported distance to infer whether you’re off mainly in angle, radius, or both.

Remember the ship moves the same number of degrees every hour. Once you sense the angular step, predict where it will be on the next shot.

Keep notes of: your shot (angle, radius), distance returned, and hour number.


Logging

Every game writes a log file next to the executable:

orbit-log-YYYYMMDD-HHMMSS.txt

Contains:

Initial hidden parameters (ship radius, period, initial angle),

Each hour’s bomb angle/radius, the ship’s true angle, and the distance,

Whether the ship was destroyed.

Example Turn
HOUR 3 — AT WHAT ANGLE DO YOU WISH TO SEND YOUR PROTON BOMB?
ANGLE (0..360°): 295
RADIUS FROM ORIGIN (miles 1000..30000): 21000

YOUR PROTON BOMB EXPLODED 295.000° @ 21000 MILES FROM THE ORIGIN.
DISTANCE FROM ENEMY SHIP = 3,480 MILES.

[ASCII radar with * at your bomb]


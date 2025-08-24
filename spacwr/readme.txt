SPACWR — Player & Setup Guide (Star-Trek-style Spacewar)
What it is

A retro, text-only space-combat sim. You’re captain of the USS Enterprise. Clear the galaxy of Klingons before your 30-stardate time limit runs out.

Requirements

Windows/macOS/Linux with .NET 6+ (or 8/9) installed.

Terminal/console only; no extra libraries.

Run it:

dotnet run -c Release

Objective & Resources

Goal: Destroy all Klingons in the 8×8 galaxy before Stardate 30.0.

You start with: Energy 3000, Shields 0, Photon Torpedoes 10.

Starbases (B): Dock to refuel/repair/rearm.

Stars (*): Obstacles—can block movement/torpedoes.

The Galaxy

8×8 Quadrants (QY,QX) numbered 1..8, each contains an 8×8 Sector grid.

Your ship (E) sits in one sector inside the current quadrant.

Scans

Short Range Scan (1) — prints the 8×8 sector map of your current quadrant:

E Enterprise, K Klingon, B Starbase, * Star, . Empty

Long Range Scan (2) — shows 3×3 surrounding quadrants. Each cell is K-B-S

K = Klingons in that quadrant (0–3)

B = Starbases (0–1)

S = Stars (0–9 shown; may be more)

Commands (numbers you type at the prompt)

Short Range Scan — draw local 8×8 sector view.

Long Range Scan — 3×3 quadrants around you (K-B-S codes).

Warp Move — travel by course and warp factor.

Fire Phasers — spend energy to damage/kill all Klingons in your quadrant (split by distance).

Fire Photon Torpedo — launch 1 torpedo along a straight line; hits the first object.

Shields / Dock / Repair — transfer energy↔shields, or refit at a base if docked.

Status — totals: Klingons left, starbases left, stardate, energy, shields, torpedoes.

Help — recap commands (or quit from your console like usual).

Movement (3 = Warp)

Course uses numeric keypad directions:

  7 8 9
  4 5 6
  1 2 3


Warp factor (0.1–8.0) ≈ how many sector steps to attempt.

Movement costs energy and advances stardate.

You can cross quadrant borders; the game rebuilds the new sector map.

Stars/objects stop you if you’d collide; navigation halts just short.

Tip: Use Long Range Scan before long moves so you don’t warp into a dead end.

Combat
Phasers (4)

Choose an energy amount to fire (1..current energy).

Damage is split among all Klingons in this quadrant, reduced by distance.

Sufficient damage destroys a Klingon ship.

Firing phasers consumes energy.

Photon Torpedoes (5)

Enter a course (1..9); torpedo flies cell by cell:

Hits K → Klingon destroyed.

Hits * → fizzles on a star.

Hits B → oops… you just destroyed a starbase.

You start with 10 torpedoes.

Enemy Fire

If Klingons are in the quadrant, they’ll shoot after your action.

Shields absorb damage first; leftover damage reduces energy.

Docked at a base? Enemy fire is blocked.

Shields, Docking & Repairs (6)

Transfer energy↔shields at any time.

Dock when you are adjacent (8-way) to a B.

Option 3 (Repair/Resupply) when docked:

Energy refilled, Shields boosted, Torpedoes restocked.

Costs a small stardate time penalty.

Status (7)

Shows:

Klingons Left, Starbases Left

Stardate (must finish before 30.0)

Your Energy, Shields, Torpedoes

Win / Lose Conditions

Win: All Klingons destroyed before SD 30.0.

Lose: Time runs out, or energy hits 0 (ship powerless).

Self-inflicted pain: Torpedoing a base reduces your support network.

Quick Flow Example

2 (Long Range Scan) to decide where to go.

3 (Warp) with course/warp to reach a Klingon quadrant.

1 (Short Scan) to see K, B, * layout.

4 (Phasers) to soften the enemy or 5 (Torpedo) to snipe a K.

6 (Shields/Dock) to top up at B when low on energy/torps.

Repeat until galaxy is clear.

Tips

Don’t roam with shields at 0; keep some buffer against return fire.

Use torpedoes when a star blocks clean phaser damage or for precise kills.

Plan routes with Long Range Scan to avoid star-choked quadrants.

Dock early if a fight goes long—running out of energy ends your run.

Kill groups of Klingons fast; the more in a quadrant, the more return fire you’ll eat.

Troubleshooting

“Movement blocked by star.” Pick a slightly different course or warp shorter hops.

“Torpedo missed (left quadrant).” Your course left the 8×8 sector; fire again from a better angle.

Can’t repair/refuel. You must be adjacent to B and use command 6 → option 3.

Out of energy. Dock ASAP; if you hit 0, the mission fails.

Possible Additions:
=======================
1. Ship & System Management

Damage Control

Track individual subsystem damage (Warp Drive, Phasers, Torpedo Tubes, Sensors, Shields, Computer, Life Support).

Damage reduces effectiveness (e.g., Warp max speed drops, phaser accuracy falls).

Repairs require time, energy, or docking at starbase.

Gradual Repairs vs. Instant Dock Repairs.

Energy Transfer Between Systems (divert from life support to weapons in a pinch).

System Failures from enemy fire or random malfunctions.

Red Alert / Yellow Alert status with auto shield raise.

2. Sensors & Intelligence

Short Range Sensor Failures (temporarily blind in local quadrant).

Long Range Sensor Jamming (enemy interference, nebulae).

Library Computer options:

Galactic map & known enemy locations.

Calculator for torpedo firing solutions.

Klingon strength analysis.

Starbase status report.

Damage & repair estimate charts.

Historical battle log.

Probe Launches to scan distant quadrants without moving there.

3. Navigation Enhancements

Course Autopilot (multi-quadrant warp routes).

Warp Accuracy Variations (imperfect navigation; higher warp speeds less accurate).

Warp Core Overload Risk if pushed past max warp.

Impulse Power for small moves without advancing stardate much.

Nebulae & Black Holes — cause navigation drift, sensor loss, or destroy anything entering.

4. Weapons & Combat Variety

Different Torpedo Types:

Standard Photon Torpedo.

Proximity Torpedo (detonates near target).

Heavy Plasma Torpedo (slower but more damage).

Phaser Modes:

Wide Beam (hits all enemies but less damage).

Narrow Beam (high damage single target).

Cloaking Device:

Reduces enemy hit chance but drains energy quickly.

Enemy Cloaking for surprise attacks.

Missed Shots / Critical Hits mechanics.

5. Enemy AI Improvements

Klingon Movement between turns — they can warp between quadrants.

Enemy Reinforcements over time if mission takes too long.

Klingon Groups coordinating attacks in nearby quadrants.

Different Enemy Types:

Romulans (cloaking, plasma torpedoes).

Pirates (steal energy/torpedoes).

Borg-like faction (slow but very powerful).

6. Mission & Story Elements

Special Missions alongside main Klingon hunt:

Rescue stranded science vessel.

Deliver critical supplies to a base.

Protect convoys from attack.

Timed Events (distress calls expire if ignored).

Victory Conditions Variations (destroy all enemies OR survive until relief fleet arrives).

Ranking & Medals at end based on performance.

7. Environmental Hazards

Asteroid Fields (blocks movement & torpedoes).

Ion Storms (damage random systems).

Radiation Zones (drains shields & energy over time).

Gravity Wells (warp harder to escape).

8. Economy & Resource Upgrades

Salvage from Destroyed Enemies (energy, torpedoes, rare parts).

Trade with Starbases for upgrades.

Ship Upgrades mid-game (better phasers, warp efficiency, shield regen).

9. Immersion & UI Polish

Command Log History (scrollable).

Replay Last Scan without using a turn.

Customizable Ship Name & Crew List (affects random text output).

ASCII Art Ship/Map Animations for warp and explosions.

Sound FX / Beeps for retro feel.

10. Difficulty & Customization

Galaxy Size Options (8×8, 10×10, etc.).

Adjustable Enemy Count & Starbase Count.

Hardcore Mode:

Permanent system damage until docked.

No starbase locations revealed at start.

Klingons actively hunt you.

If you put this full list in your documentation, you basically have a future expansion roadmap for SPACWR.
I’d recommend marking each as Core, Advanced, or Experimental so you can decide which ones to add gradually without breaking gameplay balance.

Football — Player Guide (Beginner Friendly)
Goal
Score more points than the CPU by the end of the game (4 quarters, 12:00 each). If tied, there’s a 10:00 sudden-death overtime.

The Field (how to read positions)
Field goes 0–100 yards.

On offense, you’ll see spots like OWN 32 (your 32-yard line) or OPP 18 (opponent’s 18, i.e., you’re close to their end zone).

Basic Turn Flow
 - You and the CPU alternate plays until someone scores or the ball changes teams.
 - Each drive starts 1st down & 10. You have four downs (tries) to gain 10 yards:
   - Gain 10+ yards → First down (downs reset to 1st & 10).
   - Fail by 4th down → you usually punt or try a field goal.

What to call on Offense
At the prompt:

  OFFENSE (1-7,15,16 / T timeout / Q quit):

 - 1 Inside Run – safe, modest gains
 - 2 Outside Run – can pop bigger gains, a bit riskier
 - 3 Draw – delayed run vs. pass rush
 - 4 Option – run with higher variance
 - 5 Screen Pass – short pass; safer vs blitz, modest yards
 - 6 Short Pass – 8–15 yards typical; can be incomplete/sacked/intercepted
 - 7 Long Pass – big gains possible; more risk of incompletion/sack/INT
 - 15 Punt – kick it away on 4th down to push them back
 - 16 Field Goal – 3 points if within range (rough guide: ≤52 yards from the goal posts, which equals line of scrimmage + 17)
 - T timeout (3 per half). Q quit.

What to call on Defense
When the CPU has the ball:

  DEFENSE (1-6 / T timeout / Q quit):

- 1 Normal – balanced
- 2 Run Commit – better vs runs; worse vs passes
- 3 Pass Shell – better vs passes; softer vs runs
- 4 Blitz – pressure the QB; can cause sacks but risks big plays
- 5 Intercept – more INT chance; softer coverage elsewhere
- 6 Block Kick – try to block punts/field goals

Scoring
 - Touchdown = 6 points
   After a TD you choose:
   - Kick Extra Point (+1) — ~94% success
   - Go for Two (+2) — ~45–50% success; you choose Run/Pass
 - Field Goal = 3 points
 - Safety isn’t modeled here (rare in this sim).

Kickoffs & Onside
After you score you kick off. You may try an Onside Kick (low success ~12%) to keep the ball—usually only when you’re behind late.

Turnovers & Misc
 - Interceptions on risky throws; sacks on blitzes; fumbles are rare on runs.
 - If you reach the opponent’s 0 → TD. If a kick goes into/through the end zone → touchback and they start at the 25.
 - Overtime: one 10:00 sudden-death period (first score wins). Coin toss → kickoff.

Quick Strategy Tips
 - On 4th & long near midfield → usually punt.
 - Inside the 35 (their side) on 4th & medium → consider a field goal.
 - Mix runs and short passes; take deep shots sparingly or when they sell out vs run.
 - On defense, Run Commit on short-yardage downs; Pass Shell on long downs; sprinkle Blitz.

Sample Playthrough (short)
(Your inputs are in bold. Output is abbreviated but matches the game’s prompts.)

THE COIN IS FLIPPED...
HOME RECEIVES.
KICKOFF: 63 YARDS, RETURN 21.
Q1  12:00   HOME BALL
DOWN 1 & 10  ON OWN 33
SCORE  HOME 0 — CPU 0

OFFENSE (1-7,15,16 / T timeout / Q quit): **1**
INSIDE RUN — GAIN OF 6.
Q1  11:40   HOME BALL
DOWN 2 & 4  ON OWN 39

OFFENSE (...): **6**
PASS COMPLETE — GAIN OF 12.
Q1  11:18   HOME BALL
DOWN 1 & 10  ON OWN 49

OFFENSE (...): **2**
OUTSIDE RUN — GAIN OF 9.
Q1  11:00   HOME BALL
DOWN 2 & 1  ON OPP 42

OFFENSE (...): **3**
DRAW — GAIN OF 5.
FIRST DOWN!
Q1  10:40   HOME BALL
DOWN 1 & 10  ON OPP 37

OFFENSE (...): **7**
LONG PASS COMPLETE — GAIN OF 29.
Q1  10:20   HOME BALL
DOWN 1 & GOAL  ON OPP 8

OFFENSE (...): **1**
INSIDE RUN — GAIN OF 8.
TOUCHDOWN!
AFTER TD: 1) Kick XP  2) Go for Two  -> **1**
EXTRA POINT GOOD.
ONSIDE KICK? (Y/N): **N**
KICKOFF: 60 YARDS, RETURN 23.

Q1  10:00   CPU BALL
DOWN 1 & 10  ON CPU OWN 28
SCORE  HOME 7 — CPU 0

DEFENSE (1-6 / T timeout / Q quit): **2**
CPU: INSIDE RUN — GAIN OF 2.
Q1  09:44   CPU BALL
DOWN 2 & 8  ON CPU OWN 30

DEFENSE (...): **3**
CPU: PASS INCOMPLETE.
Q1  09:34   CPU BALL
DOWN 3 & 8  ON CPU OWN 30

DEFENSE (...): **3**
CPU: PASS COMPLETE — GAIN OF 6.
Q1  09:12   CPU BALL
DOWN 4 & 2  ON CPU OWN 36

DEFENSE (...): **1**
CPU: THE PUNT IS 43, RETURN 7.
Q1  08:50   HOME BALL
DOWN 1 & 10  ON HOME OWN 28

OFFENSE (...): **5**
SCREEN PASS COMPLETE — GAIN OF 13.
Q1  08:30   HOME BALL
DOWN 1 & 10  ON HOME OWN 41

OFFENSE (...): **7**
LONG PASS — PASS INTERCEPTED!
TURNOVER ON SPOT.

Q1  08:08   CPU BALL
DOWN 1 & 10  ON HOME OPP 44

DEFENSE (...): **4**
CPU: SHORT PASS — QUARTERBACK SACKED — LOSS OF 9.
Q1  07:54   CPU BALL
DOWN 2 & 19  ON CPU OWN 47

DEFENSE (...): **3**
CPU: PASS INCOMPLETE.
Q1  07:44   CPU BALL
DOWN 3 & 19  ON CPU OWN 47

DEFENSE (...): **3**
CPU: PASS COMPLETE — GAIN OF 12.
Q1  07:22   CPU BALL
DOWN 4 & 7  ON HOME OPP 41

DEFENSE (...): **6**
CPU: FIELD GOAL ATTEMPT FROM 58 — THE KICK IS NO GOOD.
CHANGE OF POSSESSION.

Q1  07:00   HOME BALL
DOWN 1 & 10  ON HOME OPP 41
SCORE  HOME 7 — CPU 0

(…game continues. You’d likely run a few plays, maybe add a field goal before halftime.
If the CPU scores late and trails by 2, it may go for a 2-point conversion.
If tied after Q4, overtime kicks off; first score wins.)

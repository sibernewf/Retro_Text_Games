FOTBAL — High School Football Simulation
Converted from BASIC to C#
Original Source: Raymond W. Miseyka, Butler Senior High School, PA

Description
FOTBAL is an American football simulation where you are always on offense and the computer plays defense.
The game uses simplified professional rules with no penalties, and a custom play chart for both offense and defense.

You control play selection on offense, choose when to punt or attempt a field goal, and manage the clock. The computer will automatically respond with a defensive formation.

Objective
Score more points than the CPU by the end of four quarters of play.
Points are scored in standard football fashion:
 - Touchdown = 6 points
 - Field Goal = 3 points
 - Extra Point after TD (automatically attempted in this version) = +1 point if good

Quarter Length
 - This version uses shorter quarters (12 minutes) compared to the full pro game, but can be adjusted.

Game Flow
 - The game begins with a coin toss to decide who receives the opening kickoff.
 - You will start with the ball if you win the toss.
 - On each down:
   - You select an offensive play from the chart.
   - The CPU chooses a defensive strategy.
   - The result of the play is calculated and displayed.
 - The game tracks:
   - Yard line
   - Down and distance
   - Quarter and game clock
   - Score
 - First downs are earned by gaining at least 10 yards in 4 plays.
 - On 4th down, you can:
   - Go for it (run another offensive play)
   - Punt (kick to change field position)
   - Attempt a field goal (if in range)

Offense Play Chart
Type the number shown to call the play.
| #  | Play Name       | Typical Use                  |
| -- | --------------- | ---------------------------- |
| 1  | Pitchout        | Safe run to outside          |
| 2  | Triple Reverse  | Trick play, high variance    |
| 3  | Dive            | Quick inside run             |
| 5  | QB Sneak        | Short yardage                |
| 8  | End Around      | Moderate risk/gain           |
| 10 | Counter Reverse | Trick run, changes direction |
| 12 | Left Sweep      | Strong run to left           |
| 14 | Off Tackle      | Run aimed outside tackle     |
| 15 | Wishbone Option | Option run/pass blend        |
| 16 | Slip Screen     | Short pass                   |
| 18 | Screen Pass     | Short pass with blockers     |
| 19 | Sideline Pass   | Quick pass to boundary       |
| 20 | Bomb!!!         | Long pass for big yardage    |

Special Commands:
 - 97 = Punt
 - 96 = Field Goal attempt
 - 98 = Timeout (stop clock, 3 per half)
 - Q = Quit game

Defense (CPU-controlled)
The CPU will choose one of several defensive formations automatically:
 - 4–3 Base
 - 5–2
 - Nickel
 - Dime
 - Blitz
 - Line Slant
 - Press
 - Two-Deep Zone
 - Cover-3
 - Quarters

Defensive choices affect your play outcomes — certain defenses are stronger against specific types of plays.

Scoring Rules
Touchdown (TD) = 6 points
Automatic extra point kick attempted after TD (worth +1 if successful).

Field Goal (FG) = 3 points. Distance affects success chance.

No 2-point conversions in this version.

No safeties or penalties modeled.

Strategy Tips
Use QB Sneak (5) on 4th-and-short near the goal line.

Save Bomb!!! (20) for long-yardage or surprise plays — high chance of incompletion.

Screen passes (16, 18) work well against blitz-heavy defenses.

Punt when deep in your own territory on 4th down to avoid giving CPU a short field.

Attempt field goals when inside opponent’s 35-yard line.

Ending the Game
 - After 4 quarters, the team with the highest score wins.
 - No overtime is implemented in this version — ties are possible.

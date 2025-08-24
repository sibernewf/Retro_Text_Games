FOOTBL — Professional Football Simulation
Converted from BASIC to C#
Original Source: Digital Equipment Corp., Maynard, MA

Description
FOOTBL is a simulation of an American professional football game where you take the role of one team’s coach and the computer controls the other team.
You can play offense or defense, with both sides selecting plays from a fixed set.
The game follows professional rules except there are no penalties and no overtime.

Objective
Score more points than the computer by the end of four quarters of play.
Points are awarded in standard football scoring:

Touchdown = 6 points

Extra Point Kick = 1 point (attempted after touchdown)

Field Goal = 3 points

Quarter Length
Default is shorter than real-life (can be changed in settings or code).

Game clock counts down each play.

Game Flow
Coin Toss — randomly determines which team kicks off first.

Kickoff — start of each half and after scoring plays.

On each down:

If you are on offense, you select a play from the Offensive Play Chart.

If you are on defense, you select a play from the Defensive Play Chart.

The result is calculated based on:

Your choice vs. the opponent’s choice

Random chance

Predefined play effectiveness

The ball moves up or down the field based on play results.

First downs are awarded for gaining 10 yards in 4 plays.

Offensive Play Chart
Enter the number of the play you want to run.
| # | Play Name     | Type    |
| - | ------------- | ------- |
| 1 | Run Left      | Run     |
| 2 | Run Right     | Run     |
| 3 | Run Up Middle | Run     |
| 4 | Screen Pass   | Pass    |
| 5 | Short Pass    | Pass    |
| 6 | Medium Pass   | Pass    |
| 7 | Long Pass     | Pass    |
| 8 | Punt          | Special |
| 9 | Field Goal    | Special |

Defensive Play Chart
Enter the number of the defense to use.
| # | Play Name             | Focus                  |
| - | --------------------- | ---------------------- |
| 1 | Goal Line Defense     | Stop short runs        |
| 2 | Short Yardage Defense | Stop short gains       |
| 3 | Balanced Defense      | Covers run & pass      |
| 4 | Pass Defense          | Focus on pass coverage |
| 5 | Blitz                 | Pressure QB, high risk |

Special Rules
 - On 4th down, offense can punt, attempt a field goal, or run a play.
 - Field Goal range depends on starting position.
 - If a pass is incomplete or a run is stopped, the play ends and the clock may stop.
 - Turnovers (interceptions and fumbles) can happen.

Scoring
 - Touchdown (TD): 6 points
 - Extra Point Kick: +1 (attempted automatically after a touchdown)
 - Field Goal (FG): 3 points
 - No 2-point conversions in this version.
 - No safeties or penalties modeled.

Strategy Tips
 - Use Run Up Middle (3) for short yardage when close to a first down.
 - Screen Pass (4) works well if you expect a blitz.
 - Avoid Long Pass (7) unless it’s late in the game and you need big yards — higher risk of turnover.
 - On defense, a Blitz (5) is risky but can cause turnovers.
 - Mix up your play calls — the CPU will adapt.

Ending the Game
 - After four quarters, the higher score wins.
 - Ties remain as ties (no overtime in this version).

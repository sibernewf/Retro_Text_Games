ROCKETLAND AN APOLLO CAPSULE ON THE MOON
Description
ROCKET, known also as LUNAR, LEM, and APOLLO, is by far and away
the single most popular computer game. It exists in versions
that start you anywhere from 500 feet to 200 miles above the
moon, or other planets, too. Some allow the control of directional
stabilization rockets and/or the retro rocket. The three versions
presented here appear to be the most popular of the many variations.

ROCKET. In this program, you set the burn rate of the retro
rockets (pounds of fuel per second) every 10 seconds and attempt
to achieve a soft landing on the moon. 200 lbs/sec really puts
the brakes on, and 0 lbs/sec is free fall. Ignition occurs at
8 lbs/sec, so do not use burn rates between 1 and 7 lbs/sec.
To make the landing more of a challenge, but more closely approximate
the real Apollo LEM capsule, you should make the
available fuel at the start (N) equal to 16,000 lbs, and the
weight of the capsule (M) equal to 32,500 lbs in Statement 15.

Some computers object to the series expansion calculations in
Statements 91 and 94 (as you near the lunar surface, these
numbers get very small). If yours does, substitute the expanded
form--for the expansion in Statement 91:

-Q*(1+Q*(1/2+Q*(1/3+Q*(1/4+Q/5»»

You should be able to figure the other one out yourself.
ROCKTI. In this version, you start 500 feet above the lunar
surface and control the burn rate in I-second bursts. Each unit
of fuel slows your descent by 1 ft/sec. The maximum thrust of
your engine is 30 ft/sec/sec.

ROCKT2. This is the most comprehensive of the three versions
and permits you to control the time interval of firing, the
thrust, and the attitude angle. It also allows you to work in
the metric or English system of measurement. The instructions
in the program dialog are very complete, so you shouldn't have
any trouble.

In most versions of ROCKET, the temptation is to slow up too
soon and then have no fuel left for the lower part of the
journey. This, of course, is disasterous (as you will find out
when you land your own capsule)!

ROCKET — Lunar Landing Simulation
🎯 Objective

Land an Apollo Lunar Module safely on the moon by controlling the retro rocket burn rate. Your aim is to touch down gently—too fast and you’ll crash, too slow and you’ll run out of fuel before landing.

🕹 How to Play
Starting Conditions

Altitude: 120 miles above the lunar surface

Velocity: 3,600 MPH downward

Capsule weight: 32,500 lbs

Fuel weight: 16,500 lbs

Burn rate: Between 0 (free fall) and 200 lbs/sec (maximum thrust)

Ignition threshold: Burn rates from 1–7 lbs/sec have no effect (must be at least 8 lbs/sec)

Game Loop

Every 10 seconds, you must enter a burn rate.

0 = free fall (save fuel, but you accelerate downward).

200 = maximum deceleration (uses fuel quickly).

Any value between 8 and 200 will slow descent proportionally.

The computer will update:

Time elapsed in seconds

Altitude in miles and feet

Velocity in MPH

Remaining fuel in pounds

Your current burn rate

Continue adjusting your burn rate until you reach the lunar surface (0 altitude).

🏆 Landing Outcomes

When you reach the surface, your impact velocity determines the result:

Impact Velocity	Outcome
≤ 1 ft/sec	Perfect landing — congratulations!
≤ 10 ft/sec	Good landing (could be better)
≤ 30 ft/sec	Craft damaged — you survive, but repairs needed
≤ 60 ft/sec	Bad landing — severe damage, may not survive
> 60 ft/sec	Crash — total destruction
📌 Strategy Tips

Fuel Management:
Don’t use maximum burn too early—you may run out of fuel before landing.

Controlled Deceleration:
Start slowing down in the final few miles, but keep enough fuel for last-second adjustments.

Avoid “Dead Zones”:
Burn rates between 1–7 lbs/sec are wasted—they consume fuel without slowing descent.

Final Phase:
Try to reach a slow, steady descent rate in the last few hundred feet.

🧮 Example Run
SEC   MI + FT      MPH     LB FUEL   BURN RATE
0     120   0      3600    16500     70
10    199  5815    3672    16500     70
...
220     0  45      1.6546   265      79.3
ON MOON AT 233.183 SEC - IMPACT VELOCITY 1.6547 MPH
GOOD LANDING (COULD BE BETTER)

💡 Player Notes

Landing at 0 ft/sec wastes fuel—slightly above 0 is fine.

Always plan your fuel burn for the final 1–2 miles—that’s when mistakes are fatal.

Each 10-second decision has long-term consequences—think ahead.

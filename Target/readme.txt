📘 Player Documentation – TARGET: Destroy a Target in 3-D Space
🎮 Mission Briefing

You are the Weapons Officer on the Starship Enterprise.
Your task is to locate and destroy an enemy target in 3-dimensional space.

The Enterprise sits at the origin (0,0,0) of the X, Y, Z coordinate system.
The computer generates the approximate location of the target and provides:

Angular deviations from the X and Z axes (in radians and degrees).

An approximate distance to the target.

The approximate target coordinates (X, Y, Z).

Your weapon will fire along the angles you enter. A shot that lands within 20 kilometers of the target destroys it.

🕹️ How to Play

Read the information provided:

Bearings from X axis and Z axis (in radians and degrees).

Approximate target coordinates.

Approximate distance to target.

Enter your shot information when prompted:

Angle deviation from X (degrees)

Angle deviation from Z (degrees)

Shot distance (km)

The computer calculates where your shot explodes and how far it is from the target.

If you miss, you’ll receive feedback:

Shot behind / ahead / above / below / left / right of target.

Distance from target.

An updated approximation of target coordinates.

Keep firing until you score a hit (≤ 20 km from target).

📋 Example Run
READINGS FROM X AXIS = 6.13957
READINGS FROM Z AXIS = 0.8759
APPROX DEGREES FROM X AXIS = 354.72
APPROX DEGREES FROM Z AXIS = 61.89
TARGET SIGHTED! APPROX POSITION = X:14082.5, Y:-1485.9, Z:8072.0
ESTIMATED DISTANCE = 16399

INPUT ANGLE DEVIATION FROM X: 20
INPUT ANGLE DEVIATION FROM Z: 15
INPUT SHOT DISTANCE: 15400

SHOT POSITION: X:14470, Y:-1553, Z:8226
DISTANCE FROM TARGET = 507.88
MISS — TARGET STILL ACTIVE.

🎯 Objective

Destroy the target by scoring a hit within 20 km.

A skilled officer can usually eliminate a target in 3–4 shots.

💡 Tips

Small changes in angle have large effects at long ranges.

Use the explosion feedback to adjust your aim:

If you overshoot, reduce the distance.

If you’re too far left/right, adjust your X/Z angles slightly.

Patience and practice will improve your accuracy.

▶️ To Play

Double-click run.bat in the game’s folder.

This will build and run the game, and keep the window open when it finishes.
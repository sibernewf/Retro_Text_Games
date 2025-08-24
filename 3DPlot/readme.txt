📘 Player Documentation – 3DPLOT
🎮 What It Does

3DPLOT draws a family of curves that look 3-dimensional when plotted as ASCII dots on the screen. Each curve is the graph of a mathematical function applied across points in a circular grid (radius = 30).

🕹️ How to Use

Start the program.

The computer will plot the chosen function as a “surface” rising out of the x-y plane.

When complete, you’ll see a 3D-like curve made of dots.

🔄 Changing Functions

At the top of the program, uncomment the function you want to plot. Examples:

30*EXP(-Z*Z/100) → Gaussian bell curve

SQR(900-Z*Z)*.9-2 → Hemisphere shape

30*(COS(Z/16))^2 → Wave pattern

30*SIN(Z/10) → Ripple effect

30*EXP(-COS(Z/16))-30 → Bessel function (Summerfeld’s Integral)

Re-run to see the new surface.

🎯 Goal

There’s no “winning” — this is a mathematical art generator. The fun is experimenting with functions and seeing what 3D illusions appear.

▶️ To Play

Double-click run.bat in the game’s folder.

This will build and run the program, then display the curve in your console.
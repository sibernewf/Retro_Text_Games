using System;
using System.Collections.Generic;

class Program
{
    static Random random = new Random();
    static string[] pitcherOptions = { "FASTBALL", "CURVE", "SLIDER", "KNUCKLEBALL", "SCREWBALL", "SINKER", "SPITBALL (ILLEGAL)" };
    static string[] batterOptions = { "BUNT", "HIT-AND-RUN", "SWING", "SACRIFICE", "GROUNDER", "FLY", "KILL" };

    static void Main()
    {
        Console.WriteLine("WELCOME TO HUISMAN STADIUM FOR TODAY'S GREAT GAME BETWEEN");
        Console.WriteLine("THE PDP-8 PANTHERS AND (YOUR NAME PLEASE) THE SISTER ROCKETS");
        Console.WriteLine();
        Console.WriteLine("A HIGHLY PARTISAN CROWD OF 53971 FANS IS ANXIOUSLY AWAITING THE START OF THE GAME.");
        Console.WriteLine("HERE IS MY TEAM, AND HERE IS YOURS:");

        Console.WriteLine("\nPITCHER'S OPTIONS ARE:");
        foreach (var option in pitcherOptions)
            Console.WriteLine(option);

        Console.WriteLine("\nBATTER'S OPTIONS ARE:");
        foreach (var option in batterOptions)
            Console.WriteLine(option);

        Console.WriteLine("\nType Q at any prompt to quit the game.");
        Console.WriteLine("Press ENTER to start...");
        if (Console.ReadLine()?.Trim().ToUpper() == "Q") return;

        bool gameOn = true;

        while (gameOn)
        {
            Console.Write("\nYOUR PLAY? ");
            string play = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrEmpty(play)) continue;
            if (play == "Q") break;

            if (Array.Exists(pitcherOptions, o => o == play))
            {
                // Handle pitcher choice
                Console.WriteLine($"You pitch a {play}...");
                HandlePitch();
            }
            else if (Array.Exists(batterOptions, o => o == play))
            {
                // Handle batter choice
                Console.WriteLine($"You choose to {play}...");
                HandleBat();
            }
            else
            {
                Console.WriteLine("Invalid option. Try again.");
            }
        }

        Console.WriteLine("\nThanks for playing!");
    }

    static void HandlePitch()
    {
        int result = random.Next(1, 5);
        switch (result)
        {
            case 1: Console.WriteLine("Batter swings and misses. STRIKE!"); break;
            case 2: Console.WriteLine("Batter hits a grounder. Out at first!"); break;
            case 3: Console.WriteLine("Batter pops up to shallow left."); break;
            case 4: Console.WriteLine("Batter smacks a deep fly into the gap!"); break;
        }
    }

    static void HandleBat()
    {
        int result = random.Next(1, 5);
        switch (result)
        {
            case 1: Console.WriteLine("You bunt successfully. Safe at first!"); break;
            case 2: Console.WriteLine("You line a single into center!"); break;
            case 3: Console.WriteLine("You smash a double into deep right."); break;
            case 4: Console.WriteLine("You swing and miss. STRIKE!"); break;
        }
    }
}

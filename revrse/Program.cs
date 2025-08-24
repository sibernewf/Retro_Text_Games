using System;
using System.Collections.Generic;
using System.Linq;

namespace ReverseGame
{
    class Program
    {
        static readonly Random Rng = new Random();

        static void Main()
        {
            Console.Title = "REVERSE — Order a List of Numbers";
            Console.WriteLine("=== REVERSE — a game of skill ===\n");

            while (true)
            {
                var n = AskLengthOrRandom();
                var list = Enumerable.Range(1, n).ToList();
                Shuffle(list);

                Play(list);
                Console.Write("\nPlay again? (y/n) ");
                var yn = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (yn is not ("y" or "yes")) break;

                Console.Clear();
            }
        }

        static int AskLengthOrRandom()
        {
            while (true)
            {
                Console.Write("Choose length N (5–15) or press Enter for random: ");
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                    return Rng.Next(7, 11);  // nice default

                if (int.TryParse(s.Trim(), out int n) && n >= 5 && n <= 15)
                    return n;

                Console.WriteLine("Please enter an integer from 5 to 15.");
            }
        }

        static void Shuffle(List<int> list)
{
    for (int i = list.Count - 1; i > 0; i--)
    {
        int j = Rng.Next(i + 1);
        (list[i], list[j]) = (list[j], list[i]);
    }
    // avoid trivial already-sorted
    if (IsSorted(list)) Shuffle(list);
}


        static void Play(List<int> nums)
        {
            int n = nums.Count;
            int moves = 0;
            var history = new Stack<List<int>>();
            int bound = 2 * n - 3; // classic upper bound

            ShowRules(n);

            while (true)
            {
                PrintState(nums, moves, bound);

                if (IsSorted(nums))
                {
                    Console.WriteLine($"\nYOU WON IN {moves} MOVES! (Bound ~ {bound})");
                    return;
                }

                Console.Write("Reverse how many (1..N)?  [H=hint, U=undo, N=new, Q=quit] > ");
                var input = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input is "Q" or "QUIT") return;

                if (input is "N" or "NEW")
                {
                    Console.WriteLine("Starting a new puzzle...");
                    return; // back to main loop to start fresh
                }

                if (input is "U" or "UNDO")
                {
                    if (history.Count > 0)
                    {
                        nums = history.Pop();
                        moves = Math.Max(0, moves - 1);
                    }
                    else Console.WriteLine("Nothing to undo.");
                    continue;
                }

                if (input is "H" or "HINT")
                {
                    var hint = ComputeHint(nums);
                    Console.WriteLine(hint.message);
                    if (hint.k > 0)
                    {
                        history.Push(new List<int>(nums));
                        ReversePrefix(nums, hint.k);
                        moves++;
                    }
                    continue;
                }

                if (!int.TryParse(input, out int k) || k < 0 || k > n)
                {
                    Console.WriteLine("Please enter an integer between 0 and N, or H/U/N/Q.");
                    continue;
                }

                if (k == 0)
                {
                    Console.WriteLine("Reverse 0 does nothing — enter a value from 1..N, or Q to quit.");
                    continue;
                }

                if (k == 1) { Console.WriteLine("Reversing 1 has no effect."); continue; }

                history.Push(new List<int>(nums));
                ReversePrefix(nums, k);
                moves++;
            }
        }

        static void ShowRules(int n)
        {
            Console.WriteLine("\nArrange the list in **ascending** order by repeatedly reversing");
            Console.WriteLine("a prefix (counting from the left). Example: with");
            Console.WriteLine("   2 3 4 5 1 6 7 8 9");
            Console.WriteLine("if you reverse 5, it becomes");
            Console.WriteLine("   1 5 4 3 2 6 7 8 9");
            Console.WriteLine("You may type: a number (1..N), H=hint, U=undo, N=new, Q=quit.");
            Console.WriteLine($"This puzzle has N={n}; a well-known algorithm solves it in ≤ {2 * n - 3} moves.\n");
        }

        static void PrintState(IReadOnlyList<int> nums, int moves, int bound)
        {
            Console.WriteLine($"\nMoves: {moves}   (target ≤ {bound})");
            Console.WriteLine("List:  " + string.Join(" ", nums));
        }

        // Change signature to accept IList<int>
        static bool IsSorted(IList<int> a)
{
    for (int i = 1; i < a.Count; i++)
        if (a[i - 1] > a[i]) return false;
    return true;
}




        static void ReversePrefix(List<int> a, int k)
        {
            int i = 0, j = k - 1;
            while (i < j) { (a[i], a[j]) = (a[j], a[i]); i++; j--; }
        }

        // --- HINT: one step of classic pancake sort -------------------------
        // Place the largest unsorted value m at position pos = m-1.
        // If m already at pos, shrink window; else:
        //  - If m is not at the front, flip to front (k = idx+1).
        //  - Then flip front->pos (k = pos+1).
        static (int k, string message) ComputeHint(List<int> a)
        {
            int n = a.Count;
            // find largest m that is not in correct place (m at index m-1)
            int m = n;
            while (m > 1 && a[m - 1] == m) m--;
            if (m <= 1)
                return (0, "Already sorted — no hint.");

            int idx = a.IndexOf(m);
            int targetIdx = m - 1;

            if (idx == targetIdx)
                return (0, "Largest remaining already in place; try a smaller prefix.");

            if (idx != 0)
                return (idx + 1, $"Hint: bring {m} to front with reverse {idx + 1}.");

            // already at front: move it to its target position
            return (targetIdx + 1, $"Hint: place {m} by reversing {targetIdx + 1}.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AwariGame
{
    enum Player { Human = 0, Computer = 1 }

    class Board
    {
        // pits[player][0..5]
        public int[][] Pits = { new int[6], new int[6] };
        public int[] Score = new int[2];

        public Board(int seedsPerPit = 3)
        {
            for (int i = 0; i < 6; i++) { Pits[0][i] = seedsPerPit; Pits[1][i] = seedsPerPit; }
        }

        public Board Clone()
        {
            var b = new Board(0);
            b.Pits[0] = (int[])Pits[0].Clone();
            b.Pits[1] = (int[])Pits[1].Clone();
            b.Score[0] = Score[0];
            b.Score[1] = Score[1];
            return b;
        }

        public bool HasMove(Player p) => Pits[(int)p].Any(x => x > 0);

        public IEnumerable<int> LegalMoves(Player p)
        {
            for (int i = 0; i < 6; i++) if (Pits[(int)p][i] > 0) yield return i;
        }

        // Apply move; returns false if move illegal
        public bool ApplyMove(Player p, int pitIndex)
        {
            int me = (int)p, opp = 1 - me;
            if (pitIndex < 0 || pitIndex > 5 || Pits[me][pitIndex] == 0) return false;

            int seeds = Pits[me][pitIndex];
            Pits[me][pitIndex] = 0;

            int side = me, idx = pitIndex;
            while (seeds > 0)
            {
                idx++;
                if (idx >= 6) { idx = 0; side = 1 - side; }
                Pits[side][idx]++; seeds--;
            }

            // tentative capture if last landed on opponent side
            int captured = 0;
            if (side == opp)
            {
                int tIdx = idx;
                while (tIdx >= 0 && Pits[opp][tIdx] >= 2 && Pits[opp][tIdx] <= 3)
                {
                    captured += Pits[opp][tIdx]; Pits[opp][tIdx] = 0; tIdx--;
                }

                // no-starve rule: if opponent now has zero seeds on board, undo capture
                if (Pits[opp].Sum() == 0)
                {
                    // undo capture
                    tIdx = idx; int giveBack = captured;
                    while (giveBack > 0)
                    {
                        if (Pits[opp][tIdx] == 0) { int add = Math.Min(3, giveBack); Pits[opp][tIdx] += add; giveBack -= add; }
                        tIdx--;
                        if (tIdx < 0) break;
                    }
                    captured = 0;
                }
            }
            Score[me] += captured;
            return true;
        }

        // If game over, sweep remaining seeds to the side that still has them
        public bool CheckGameOver(out string message)
        {
            message = "";
            if (HasMove(Player.Human) && HasMove(Player.Computer)) return false;

            if (!HasMove(Player.Human) && !HasMove(Player.Computer))
            {
                message = "No moves on either side.";
            }
            else if (!HasMove(Player.Human))
            {
                Score[1] += Pits[1].Sum(); Array.Fill(Pits[1], 0);
                message = "Human has no legal move.";
            }
            else
            {
                Score[0] += Pits[0].Sum(); Array.Fill(Pits[0], 0);
                message = "Computer has no legal move.";
            }
            return true;
        }

        public int Evaluate(Player p) => (Score[(int)p] + Pits[(int)p].Sum())
                                       - (Score[1 - (int)p] + Pits[1 - (int)p].Sum());

        public void Print()
        {
            // Computer row shown left-to-right, human row left-to-right beneath (like the scan)
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < 6; i++) Console.Write($"{Pits[1][i],2} ");
            Console.WriteLine($"   (COMPUTER SCORE: {Score[1]})");
            Console.Write("   ");
            for (int i = 0; i < 6; i++) Console.Write($"{Pits[0][i],2} ");
            Console.WriteLine($"   (YOUR SCORE: {Score[0]})\n");
        }
    }

    class AI
    {
        readonly int depth;
        readonly Random rng = new Random();
        public AI(int depth = 4) => this.depth = Math.Max(1, depth);

        public int ChooseMove(Board b)
        {
            int bestMove = b.LegalMoves(Player.Computer).First();
            int bestVal = int.MinValue;

            foreach (var m in b.LegalMoves(Player.Computer))
            {
                var c = b.Clone();
                c.ApplyMove(Player.Computer, m);
                int val = -Negamax(c, depth - 1, Player.Human);
                if (val > bestVal || (val == bestVal && rng.Next(2) == 0))
                {
                    bestVal = val; bestMove = m;
                }
            }
            return bestMove;
        }

        int Negamax(Board b, int d, Player toMove)
        {
            if (d == 0 || b.CheckGameOver(out _)) return b.Evaluate(Player.Computer);

            int best = int.MinValue;
            foreach (var m in b.LegalMoves(toMove))
            {
                var c = b.Clone();
                c.ApplyMove(toMove, m);
                int val = -Negamax(c, d - 1, (Player)(1 - (int)toMove));
                if (val > best) best = val;
            }
            if (best == int.MinValue) return b.Evaluate(Player.Computer); // no moves
            return best;
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("*** GAME OF AWARI ***");
            var board = new Board(seedsPerPit: 3);
            var ai = new AI(depth: 4);

            Player turn = Player.Human; // same as sample run
            board.Print();

            while (true)
            {
                if (board.CheckGameOver(out var why))
                {
                    Console.WriteLine("GAME OVER — " + why);
                    Console.WriteLine($"YOU: {board.Score[0]}   COMPUTER: {board.Score[1]}");
                    Console.WriteLine(board.Score[0] > board.Score[1] ? "YOU WIN!" :
                                      board.Score[0] < board.Score[1] ? "I WIN!" : "IT'S A DRAW!");
                    break;
                }

                if (turn == Player.Human)
                {
                    int move = PromptMove(board);
                    board.ApplyMove(Player.Human, move);
                    Console.WriteLine($"\nYOUR MOVE: {move + 1}");
                }
                else
                {
                    int move = ai.ChooseMove(board);
                    board.ApplyMove(Player.Computer, move);
                    Console.WriteLine($"\nMY MOVE IS {move + 1}");
                }

                board.Print();
                turn = (Player)(1 - (int)turn);
            }
        }

        static int PromptMove(Board b)
        {
            while (true)
            {
                Console.Write("YOUR MOVE? (1-6) ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out int n) && n >= 1 && n <= 6 && b.Pits[0][n - 1] > 0)
                    return n - 1;

                Console.WriteLine("Pick a non-empty pit from 1 to 6 on your row.");
            }
        }
    }
}

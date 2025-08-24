using System;

namespace TicTacToe
{
    internal static class Program
    {
        private static readonly char[,] Board = new char[3, 3];
        private static readonly Random Rnd = new Random();

        static void Main()
        {
            Console.WriteLine("YOU HAVE THE OPPORTUNITY OF TRYING TO BEAT THE COMPUTER");
            Console.WriteLine("AT TIC-TAC-TOE.");
            Console.WriteLine("ENTER YOUR MOVES AS: ROW, COL (e.g., 1,1 or 2,3).");
            Console.WriteLine("ROWS = HORIZONTAL, COLUMNS = VERTICAL.\n");

            bool playAgain;
            do
            {
                ResetBoard();
                PlayGame();
                playAgain = AskYesNo("\nDO YOU WANT TO PLAY ANOTHER GAME? (YES=1, NO=0) ");
            } while (playAgain);

            Console.WriteLine("\nIT'S BEEN FUN, COME AGAIN SOMETIME!");
        }

        private static void PlayGame()
        {
            bool gameOver = false;
            while (!gameOver)
            {
                PrintBoard();

                // Player move
                (int r, int c) = GetPlayerMove();
                if (Board[r, c] != ' ')
                {
                    Console.WriteLine("*** ILLEGAL MOVE — TRY AGAIN ***");
                    continue;
                }
                Board[r, c] = 'Y'; // you

                if (CheckWin('Y'))
                {
                    PrintBoard();
                    Console.WriteLine("YOU WIN THIS TIME!");
                    return;
                }
                if (BoardFull())
                {
                    PrintBoard();
                    Console.WriteLine("TIE GAME ...");
                    return;
                }

                // Computer move
                Console.WriteLine("\nPDP THINKING...");
                (int cr, int cc) = GetComputerMove();
                Board[cr, cc] = 'C';

                if (CheckWin('C'))
                {
                    PrintBoard();
                    Console.WriteLine("SORRY — THE PDP-8 WINS THIS TIME.");
                    return;
                }
                if (BoardFull())
                {
                    PrintBoard();
                    Console.WriteLine("TIE GAME ...");
                    return;
                }
            }
        }

        private static (int, int) GetPlayerMove()
        {
            while (true)
            {
                Console.Write("\nYOUR MOVE? ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(',');
                if (parts.Length != 2) continue;

                if (int.TryParse(parts[0], out int row) &&
                    int.TryParse(parts[1], out int col) &&
                    row >= 1 && row <= 3 &&
                    col >= 1 && col <= 3)
                {
                    return (row - 1, col - 1);
                }

                Console.WriteLine("PLEASE ENTER A MOVE AS ROW,COL (E.G., 2,3).");
            }
        }

        private static (int, int) GetComputerMove()
        {
            // Simple AI:
            // 1. If computer can win, take it.
            if (TryWinOrBlock('C', out var move)) return move;
            // 2. If player could win next, block.
            if (TryWinOrBlock('Y', out move)) return move;
            // 3. Otherwise random empty.
            int r, c;
            do
            {
                r = Rnd.Next(3);
                c = Rnd.Next(3);
            } while (Board[r, c] != ' ');
            return (r, c);
        }

        private static bool TryWinOrBlock(char symbol, out (int, int) move)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (Board[r, c] != ' ') continue;
                    Board[r, c] = symbol;
                    bool win = CheckWin(symbol);
                    Board[r, c] = ' ';
                    if (win)
                    {
                        move = (r, c);
                        return true;
                    }
                }
            }
            move = (0, 0);
            return false;
        }

        private static void ResetBoard()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Board[r, c] = ' ';
        }

        private static void PrintBoard()
        {
            Console.WriteLine();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    char ch = Board[r, c];
                    if (ch == 'Y') ch = '*'; // you
                    if (ch == 'C') ch = 'P'; // PDP
                    Console.Write(ch == ' ' ? "   " : $" {ch} ");
                }
                Console.WriteLine();
            }
        }

        private static bool CheckWin(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Board[i, 0] == player && Board[i, 1] == player && Board[i, 2] == player) return true;
                if (Board[0, i] == player && Board[1, i] == player && Board[2, i] == player) return true;
            }
            if (Board[0, 0] == player && Board[1, 1] == player && Board[2, 2] == player) return true;
            if (Board[0, 2] == player && Board[1, 1] == player && Board[2, 0] == player) return true;
            return false;
        }

        private static bool BoardFull()
        {
            foreach (var cell in Board) if (cell == ' ') return false;
            return true;
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "1" || s?.ToUpper() == "YES") return true;
                if (s == "0" || s?.ToUpper() == "NO") return false;
            }
        }
    }
}

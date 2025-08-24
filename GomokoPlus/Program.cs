using System;
using System.Globalization;
using System.Linq;

namespace Gomoko_V2
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "GOMOKO+ — X vs O with win detection";
            Console.WriteLine("GOMOKO+ — get five in a row. You=X, CPU=O, empty='.'");
            Console.WriteLine("Board size 7..19. Enter row,col. Q quits.");
            int n = AskInt("BOARD SIZE (7..19)? ", 7, 19);
            if (n == int.MinValue) return;
            new Game(n).Run();
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return int.MinValue;
                if (int.TryParse(s, out var v) && v >= min && v <= max) return v;
                Console.WriteLine($"Enter {min}..{max} (or Q to quit).");
            }
        }
    }

    internal sealed class Game
    {
        readonly int n;
        readonly int[,] b;
        readonly Random rng = new();

        public Game(int size) { n = size; b = new int[n,n]; }

        public void Run()
        {
            while (true)
            {
                Print();

                // Human
                var mv = AskMove("YOUR MOVE (row,col): ");
                if (mv.quit) return;
                if (!TryPlace(mv.r - 1, mv.c - 1, 1))
                {
                    Console.WriteLine("OCCUPIED / OUT OF BOUNDS. TRY AGAIN.");
                    continue;
                }
                if (Winner(1)) { Print(); Console.WriteLine("YOU WIN!"); break; }
                if (Full())    { Print(); Console.WriteLine("DRAW.");   break; }

                // CPU
                var (cr, cc) = CpuMove();
                b[cr, cc] = 2;
                Console.WriteLine($"CPU: {cr+1},{cc+1}");
                if (Winner(2)) { Print(); Console.WriteLine("CPU WINS."); break; }
                if (Full())    { Print(); Console.WriteLine("DRAW.");     break; }
            }

            Console.Write("\nPLAY AGAIN (Y/N)? ");
            var again = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (again is "Y" or "YES") new Game(n).Run();
        }

        (int r, int c, bool quit) AskMove(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return (0,0,true);
                var parts = s.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                    return (r,c,false);

                Console.WriteLine("Format: row,col  (e.g., 10,7)  or Q to quit.");
            }
        }

        bool TryPlace(int r, int c, int who)
        {
            if (r < 0 || r >= n || c < 0 || c >= n) return false;
            if (b[r, c] != 0) return false;
            b[r, c] = who;
            return true;
        }

        void Print()
        {
            Console.WriteLine();
            for (int i = 0; i < n; i++)
            {
                var line = string.Join(" ", Enumerable.Range(0, n).Select(j =>
                    b[i, j] == 0 ? "." : b[i, j] == 1 ? "X" : "O"));
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        bool Full()
        {
            for (int i=0;i<n;i++) for (int j=0;j<n;j++) if (b[i,j]==0) return false;
            return true;
        }

        bool Winner(int who)
        {
            int[][] dirs = { new[]{0,1}, new[]{1,0}, new[]{1,1}, new[]{-1,1} };
            for (int i=0;i<n;i++)
            for (int j=0;j<n;j++)
            {
                if (b[i,j]!=who) continue;
                foreach (var d in dirs)
                {
                    int cnt=1, r=i+d[0], c=j+d[1];
                    while (r>=0 && r<n && c>=0 && c<n && b[r,c]==who)
                    { cnt++; r+=d[0]; c+=d[1]; if (cnt==5) return true; }
                }
            }
            return false;
        }

        (int r, int c) CpuMove()
        {
            // 1) win if possible
            if (FindWinningMove(2, out var w)) return w;
            // 2) block if you can win next
            if (FindWinningMove(1, out var bmv)) return bmv;
            // 3) heuristic best
            (int r,int c,int s) best=(-1,-1,int.MinValue);
            for (int r=0;r<n;r++)
                for (int c=0;c<n;c++)
                    if (b[r,c]==0)
                    {
                        int s = HeuristicScore(r,c);
                        if (s>best.s) best=(r,c,s);
                    }
            if (best.r>=0) return (best.r,best.c);
            // 4) fallback
            for (int t=0;t<n*n;t++){ int r=rng.Next(n), c=rng.Next(n); if (b[r,c]==0) return (r,c); }
            return (0,0);
        }

        bool FindWinningMove(int who, out (int r,int c) mv)
        {
            for (int r=0;r<n;r++)
                for (int c=0;c<n;c++)
                    if (b[r,c]==0)
                    {
                        b[r,c]=who;
                        bool win = Winner(who);
                        b[r,c]=0;
                        if (win){ mv=(r,c); return true; }
                    }
            mv=(0,0); return false;
        }

        int HeuristicScore(int r, int c)
        {
            int score=0;
            int[][] dirs = { new[]{0,1}, new[]{1,0}, new[]{1,1}, new[]{-1,1} };
            foreach (var d in dirs)
            {
                score += LinePotential(r,c,d[0],d[1],2)*3;
                score += LinePotential(r,c,d[0],d[1],1)*2;
            }
            int centerBias = -Math.Abs(r - n/2) - Math.Abs(c - n/2);
            return score*10 + centerBias;
        }

        int LinePotential(int r, int c, int dr, int dc, int who)
        {
            int count=1, open=0;
            int i=r+dr, j=c+dc;
            while (In(i,j) && b[i,j]==who){ count++; i+=dr; j+=dc; }
            if (In(i,j) && b[i,j]==0) open++;
            i=r-dr; j=c-dc;
            while (In(i,j) && b[i,j]==who){ count++; i-=dr; j-=dc; }
            if (In(i,j) && b[i,j]==0) open++;
            if (count>=5) return 1000;
            return (count*count) + (open*2);
        }

        bool In(int r,int c)=> r>=0 && r<n && c>=0 && c<n;
    }
}

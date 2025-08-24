using System;
using System.IO;
using System.Text.Json;

namespace GuessTheAnimal
{
    class Program
    {
        static readonly string SavePath = "animals.json";
        static Node? root;
        static void Main()
        {
            Console.WriteLine("PLAY 'GUESS THE ANIMAL' WITH ARTS");
            Console.WriteLine("THINK OF AN ANIMAL AND THE COMPUTER WILL TRY TO GUESS IT...\n");

            root = Load() ?? Seed();

            do
            {
                PlayRound();
            } while (AskYesNo("\nARE YOU THINKING OF AN ANIMAL?"));

            Save(root!);
            Console.WriteLine("\nSAVED. BYE!");
        }

        static void PlayRound()
        {
            var node = root!;
            Node? parent = null;
            bool fromYes = false;

            // Walk down the tree by asking questions
            while (!node.IsLeaf)
            {
                bool answer = AskYesNo(node.Question!);
                parent = node;
                if (answer)
                {
                    node = node.Yes!;
                    fromYes = true;
                }
                else
                {
                    node = node.No!;
                    fromYes = false;
                }
            }

            // We reached a guess
            if (AskYesNo($"ARE YOU THINKING OF A(N) {node.Animal?.ToUpperInvariant()}?"))
            {
                Console.WriteLine("I GOT IT!");
                return;
            }

            // Learn
            string newAnimal = AskNonEmpty("PLEASE TYPE IN THE ANIMAL YOU WERE THINKING OF:");
            string newQ = AskNonEmpty($"PLEASE TYPE IN A QUESTION THAT WOULD DISTINGUISH A {newAnimal.ToUpper()} FROM A {node.Animal!.ToUpper()}");
            bool answerYesMeansNew = AskYesNo($"FOR A {newAnimal.ToUpper()} THE ANSWER WOULD BE? (YES/NO)");

            // Build new subtree
            var newAnimalNode = Node.AnimalLeaf(newAnimal);
            var oldAnimalNode = Node.AnimalLeaf(node.Animal!);
            var newQuestionNode = new Node
            {
                Question = NormalizeQ(newQ),
                Yes = answerYesMeansNew ? newAnimalNode : oldAnimalNode,
                No  = answerYesMeansNew ? oldAnimalNode : newAnimalNode
            };

            if (parent == null)
            {
                root = newQuestionNode; // tree had only one leaf
            }
            else
            {
                if (fromYes) parent.Yes = newQuestionNode;
                else parent.No = newQuestionNode;
            }

            Console.WriteLine("THANKS — I’LL REMEMBER THAT.");
        }

        // -------- Helpers --------

        static string NormalizeQ(string q)
        {
            q = q.Trim();
            if (!q.EndsWith("?")) q += "?";
            return q.ToUpperInvariant();
        }

        static string AskNonEmpty(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                Console.Write("> ");
                var s = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(s)) return s;
            }
        }

        static bool AskYesNo(string q)
        {
            while (true)
            {
                Console.WriteLine(q.ToUpper());
                Console.Write("YES OR NO? ");
                var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (s == "y" || s == "yes") return true;
                if (s == "n" || s == "no") return false;
                Console.WriteLine("PLEASE ANSWER 'YES' OR 'NO'.");
            }
        }

        static Node Seed()
        {
            // Very small starting knowledge base (you can change these).
            // The BASIC listing often started with a few animals; we’ll seed with FISH vs BIRD.
            return new Node
            {
                Question = "DOES IT FLY?",
                Yes = Node.AnimalLeaf("bird"),
                No  = Node.AnimalLeaf("fish")
            };
        }

        static void Save(Node root)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SavePath, JsonSerializer.Serialize(root, opts));
        }

        static Node? Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                var json = File.ReadAllText(SavePath);
                return JsonSerializer.Deserialize<Node>(json);
            }
            catch
            {
                return null; // if corrupted, just start fresh
            }
        }
    }

    // Binary tree node: either a question (internal node) or an animal (leaf).
    public class Node
    {
        public string? Question { get; set; }   // if null => leaf
        public string? Animal { get; set; }     // only set on leaf
        public Node? Yes { get; set; }          // question: yes-branch
        public Node? No { get; set; }           // question: no-branch

        public bool IsLeaf => string.IsNullOrWhiteSpace(Question);

        public static Node AnimalLeaf(string name) => new Node { Animal = name };
    }
}

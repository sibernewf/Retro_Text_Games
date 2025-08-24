using System;
using System.Collections.Generic;

namespace LITQZ
{
    internal static class Program
    {
        private sealed class Question
        {
            public string Prompt { get; }
            public string[] Choices { get; }
            public int CorrectIndex { get; } // 0-based
            public string CorrectText => Choices[CorrectIndex];

            public Question(string prompt, string[] choices, int correctIndex)
            {
                Prompt = prompt;
                Choices = choices;
                CorrectIndex = correctIndex;
            }
        }

        private static readonly List<Question> Quiz = new()
        {
            new Question(
                "IN 'PINOCCHIO', WHAT WAS THE NAME OF THE CAT?",
                new[] { "TIGGER", "CICERO", "FIGARO", "GUIPETTO" }, 2),

            new Question(
                "FROM WHOSE GARDEN DID BUGS BUNNY STEAL THE CARROTS?",
                new[] { "MR. NIXON", "ELMER FUDD", "JUD CLAMPETT", "STROMBOLI" }, 1),

            new Question(
                "IN 'THE WIZARD OF OZ', DOROTHY'S DOG WAS NAMED",
                new[] { "CICERO", "TRIXIE", "KING", "TOTO" }, 3),

            new Question(
                "WHO WAS THE FAIR MAIDEN WHO ATE THE POISON APPLE?",
                new[] { "SLEEPING BEAUTY", "CINDERELLA", "SNOW WHITE", "WENDY" }, 2),
        };

        static void Main()
        {
            Console.Title = "LITQZ — Children's Literature Quiz";

            do
            {
                RunQuiz();
            } while (AskYesNo("\nPLAY AGAIN? (Y/N) "));
        }

        private static void RunQuiz()
        {
            Console.WriteLine("TEST YOUR KNOWLEDGE OF CHILDREN'S LITERATURE.");
            Console.WriteLine("THIS IS A MULTIPLE-CHOICE QUIZ.");
            Console.WriteLine("TYPE 1, 2, 3, OR 4 AFTER THE QUESTION MARK.");
            Console.WriteLine("GOOD LUCK!!\n");

            int score = 0;
            int qNum = 1;

            foreach (var q in Quiz)
            {
                Console.WriteLine($"{qNum++}. {q.Prompt}");
                for (int i = 0; i < q.Choices.Length; i++)
                    Console.WriteLine($"   {i + 1}) {q.Choices[i]}");
                int choice = ReadChoice(" ? ");

                if (choice - 1 == q.CorrectIndex)
                {
                    Console.WriteLine(qNum == 3   // after Q2 in original tone it says "PRETTY GOOD!"
                        ? "PRETTY GOOD!"
                        : qNum == 2
                            ? "VERY GOOD! HERE'S ANOTHER."
                            : qNum == 5
                                ? "YEA! YOU'RE A REAL LITERATURE GIANT!"
                                : "GOOD MEMORY!");
                    score++;
                }
                else
                {
                    Console.WriteLine("OH, COME ON NOW... IT WAS " + q.CorrectText.ToUpper() + ".");
                }

                Console.WriteLine();
            }

            // Closing verdict (mirrors booklet tone)
            Console.WriteLine(score switch
            {
                4 => "WOW; THAT'S SUPER; YOU REALLY KNOW YOUR NURSERY!",
                3 => "GOOD MEMORY!",
                2 => "NOT BAD, BUT YOU MIGHT SPEND A LITTLE MORE TIME READING THE NURSERY GREATS.",
                _ => "NURSERY SCHOOL FOR YOU, MY FRIEND."
            });
        }

        private static int ReadChoice(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, out int n) && n >= 1 && n <= 4)
                    return n;
                Console.WriteLine("Please enter 1, 2, 3, or 4.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                string? s = Console.ReadLine();
                if (s == null) return false;
                s = s.Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
                Console.Write("Please answer Y or N: ");
            }
        }
    }
}

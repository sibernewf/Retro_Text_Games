📘 Player Documentation – WEKDAY: Facts About Your Birthday
🎮 The Program

WEKDAY is a light-hearted date utility written in PDP-11 BASIC style.
It asks for today’s date and your date of birth (or any date of interest) and then:

Tells you the day of the week you were born on.

Calculates your age in years, months, and days (approximate, using a 365-day year / 30-day month model).

Estimates how much of your life you’ve spent:

Sleeping

Eating

Working/Studying (or Playing if you’re very young)

Relaxing

Predicts your retirement year (birth year + 65).

Prints warnings for quirky cases, like Friday the 13th or dates before 1582 (before the Gregorian calendar).

🕹️ How to Use

Run the program.

Enter today’s date in this form:

MONTH,DAY,YEAR


Example:

6,12,1973


(2-digit years like 73 are accepted → 1973.)

Enter your date of birth in the same form.
Example:

9,24,1948


The program prints:

The weekday you were born on.

Your age breakdown.

Time spent sleeping, eating, working/studying (or playing), and relaxing.

Your retirement year.

📋 Example Run
ENTER TODAY'S DATE IN THIS FORM: MONTH,DAY,YEAR? 6,12,1973

THIS PROGRAM DEMONSTRATES PDP-11 BASIC AND ALSO GIVES
FACTS ABOUT A DATE OF INTEREST TO YOU

ENTER DATE OF BIRTH IN THIS FORM: MO,DAY,YEAR? 9,24,1948

9 / 24 / 1948  WAS A FRIDAY

YOUR AGE         YEARS   MONTHS    DAYS
                  24       8       18
YOU HAVE SLEPT     8       7       27
YOU HAVE EATEN     4       2       13
YOU HAVE WORKED    5       8       10
YOU HAVE RELAXED   6       1       28

**YOU MAY RETIRE IN 2013**

CALCULATED BY THE BEST MINICOMPUTER TODAY - THE PDP-11

🏆 Notes

Calendar Limit: No dates before 1582 (before the modern calendar).

Friday the 13th: If today’s date is Friday the 13th, it warns you with a fun “BEWARE!” message.

Young Ages: If you’re very young, the “Work/Study” category is labeled as Play or Play/Study.

Approximation: Months are treated as 30 days for simplicity, to mimic the original PDP program’s math.

▶️ To Run

Double-click run.bat in the game’s folder.

This builds and runs the program, and keeps the window open when it finishes.

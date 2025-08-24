Poetry Suite — Player Guide
What this app makes
The generator writes short poems using different forms:
 - Haiku (5–7–5 syllables)
 - Couplet (AA rhyme)
 - Quatrain (ABAB or ABBA)
 - Tercet (ABA)
 - Quintain (AABBA — limerick-like)
 - Free verse (3–6 lines, optional internal rhyme)

You can toggle:
 - Themes (e.g., Nature, Poe) → changes the vocabulary
 - Alliteration → biases many content words in a line to the same starting letter
 - Save to file → writes the poem to Poems/poem-YYYYMMDD-HHMMSS.txt

How to play
 - Run the program (console).
 - Pick a form (1–7).
 - Press T to cycle themes (Nature, Poe, or any you add).
 - Press A to toggle alliteration (ON/OFF).
 - After the poem prints, choose whether to save it to a .txt.

📁 Theme Files — Where they live
  <APP FOLDER>/
    Program.cs (your app)
    Themes/
      Nature/
        lexicon.txt
        templates.txt    (optional)
      Poe/
        lexicon.txt
        templates.txt    (optional)
      <YourThemeName>/
        lexicon.txt
        templates.txt    (optional)

 - Each subfolder under Themes/ is a theme.
 - A theme is detected if it has a lexicon.txt.
 - templates.txt is optional; if omitted, the app uses built-in defaults.

If the entire Themes/ folder is missing, the app falls back to small embedded vocabularies so it still runs.

🗃️ lexicon.txt — EXACT format (excruciating detail)
One word per line, pipe-delimited:

  word | pos | syl | rhymeKey | weight

 - word (string, required)
   The literal token to print. Can be multi-word (e.g., wide sky).
   Use ASCII or UTF-8; spaces are fine. Example: ever dark.
 - pos (required) — part of speech (lowercase):
   - det = determiner/article (the, a, this, my)
   - adj = adjective (soft, wild, golden)
   - noun = noun (river, door, raven)
   - verb = verb (sings, wanders, broods)
   - prep = preposition (under, across, within)
   - adv = adverb (slowly, forever)
   - gerund = -ing word acting like a noun phrase (falling, shimmering, beating)
     (Why it matters: templates tell the generator which POS comes where.)
 - syl (integer, required) — syllable count
   Used to target meters (5–7–5; 8–10 syllables, etc.).
   Estimate is fine — the engine pads/adjusts.
   Tips to estimate syllables quickly:
   - “Clap method” (vowel groups).
   - If stuck, treat common -ing, -ed endings as 1 syllable unless clearly pronounced as 2.
   - Hyphenated compounds: add parts (e.g., ever-more ≈ 3).
 - rhymeKey (string, optional) — groups end-rhyme families
   Lines that should rhyme end with a word sharing the same key.
   Choose any label (e.g., ORE, EEN, AWN). Blank is allowed.
   Best practice: assign rhyme keys to many nouns/verbs/adverbs, not only adjectives.
 - weight (integer ≥1, optional; default = 1) — frequency bias
   The word is inserted into the vocabulary weight times to be picked more often.

Minimal valid line
  the|det|1||

Full featured line
  breeze|noun|1|EEZ|3

Multi-word tokens
  wide sky|noun|2|AI
  ever dark|adj|3|ARK

Comments & blanks
 - Anything after # is ignored:

    dawn|noun|1|AWN   # simple one-syllable noun

 - Empty lines are ignored.

What gets rejected (silently ignored)
 - Missing required fields (fewer than 3 segments)
 - Unknown POS tag (must be exactly one of the 7 above)
 - Non-integer syllables
 - Negative/zero weights

The app ignores malformed lines rather than crashing.
If nothing loads for a theme (e.g., all lines invalid), the theme is skipped.

🧩 templates.txt — EXACT format (optional)
One template per line — a sequence of POS tags that shapes a line:

  det adj noun verb prep det noun
  det noun verb adv
  gerund prep det noun
  noun verb prep det adj noun
  det adj noun

 - Tokens are separated by spaces (commas/tabs also OK).
 - Allowed tags are the same 7 POS: det adj noun verb prep adv gerund.
 - Blank lines and # comments are ignored.
 - At runtime, the engine picks one template per line and fills it with words of the requested POS, then adjusts/pads to get close to the target syllables.

If templates.txt is missing or empty → the app uses built-in defaults.

🧠 How the generator uses your files

1. Theme loading
   - For each folder in Themes/, read lexicon.txt.
   - Parse each line into (word, pos, syl, rhymeKey, weight).
   - Expand the vocabulary list by weight copies.
   - If templates.txt exists, load its POS sequences; else use defaults.
   - If a theme has no valid words, it’s ignored.

2. Picking words
   - The chosen template dictates the POS order (e.g., det adj noun verb …).
   - For each slot, the engine picks a word that fits the remaining syllable budget (tries up to 12 times).
   - If alliteration is ON, content words (adj, noun, verb) are biased to share the same starting letter.
   - If the form requires rhyme (AA, ABAB, etc.), the last word of each rhymed line is replaced with one whose rhymeKey matches the scheme.

3. Syllables
    - The generator seeks the target (e.g., 5, 7, 10).
   - If the line runs short, it may pad with tiny words (e.g., determiners/adverbs) that fit the remaining budget.

4. Punctuation & polish
   - Sometimes inserts a comma or em dash near the end of the line.
   - Capitalizes the first character.

Ends lines with ., ,, ;, or … (random taste, slightly biased toward .).

✅ Quick-start checklist (for your own theme)
 - Create a folder: Themes/Cyberpunk/
 - Make lexicon.txt with at least ~30 words. Mix POS types:
   - 6–8 determiners (det) — glue words
   - 8–12 adjectives (adj) — color
   - 12–20 nouns (noun) — end-word candidates → give many a rhymeKey
   - 8–15 verbs (verb) — action → also give some rhymeKey
   - 5–10 preps (prep), 5–8 adverbs (adv)
   - 4–8 gerunds (gerund)
 - (Optional) Make templates.txt with 4–8 POS patterns.
 - Run the app, press T until your theme appears, generate poems.
 - If lines feel bland, add more words with weights or grow rhyme families.
 - If lines feel ungrammatical, add more determiners/prepositions and tune templates.

🧪 Examples (good vs. bad)
Good (balanced vocabulary; rhymeable ends)
  the|det|1||
  a|det|1||
  chrome|adj|1|OME
  neon|adj|2||
  silent|adj|2||
  street|noun|1|EET|2
  code|noun|1|ODE|2
  server|noun|2|ERV
  signal|noun|2|IG
  glitch|noun|1|ITCH
  hums|verb|1|UMS
  flickers|verb|2|IK
  wanders|verb|2|AN
  under|prep|2||
  across|prep|2||
  slowly|adv|2||
  falling|gerund|2|AWL

Bad (common mistakes)
  the|article|1||          # ❌ "article" is not a valid POS tag (use "det")
  light|noun|one|ITE       # ❌ "syl" must be an integer
  river|noun|2             # ❌ missing trailing fields is OK, but you need at least 3 fields — this is OK actually (word|pos|syl); (rhymeKey, weight) are optional
  neon||2||                # ❌ missing POS
  hums|verb|1|-1           # ❌ weight negative (4th field is rhymeKey; 5th is weight)

(Note: word|pos|syl is the minimum — the river example is valid. Missing rhymeKey and weight is fine.)

🧵 Example templates.txt
  # Simple descriptive lines
  det adj noun
  det adj noun verb prep det noun
  det noun verb adv
  gerund prep det noun
  noun verb prep det adj noun


Why these work well:
 - Begin with det so adjectives/nouns flow naturally (“the quiet river …”).
 - Include verbs and prepositions to keep lines sentence-like.
 - Sprinkle gerund lines for variety (“shimmering across the stone”).

🎼 Rhyme keys (how to pick them)
 - Rhyme keys are just labels that must match for words that rhyme.
 - You decide the naming scheme: ORE, EEN, AWN, INE, ARK, etc.
 - Attach each key to several nouns/verbs/adverbs so the generator has choices.
 - If a form asks for rhyme and the engine can’t find a word with that key, it will just skip the rhyme (line still prints).

Tip: If you want tight rhymes, make sure end-position favorites (nouns/verbs) carry keys and appear multiple times via weight.

🧯 Troubleshooting
 - My theme doesn’t show up.
   Ensure Themes/<Name>/lexicon.txt exists and contains at least one valid line (word|pos|syl minimum). Folder names can’t be empty.
 - Lines look ungrammatical.
   Add more det (the, a, this), prep (under, within), and tune templates.txt to include those slots.
 - Rhyme schemes don’t seem to rhyme.
   Add more words with the same rhymeKey for nouns/verbs/adverbs; avoid putting keys only on adjectives.
 - Haiku syllables feel off.
   Adjust syl values in the lexicon. Perception varies; being off by ±1 is common. Add more 1-syllable pads (det/adv) so the engine can fine-tune.
 - Too repetitive.
   Reduce weights on overused words, add synonyms, and add more templates.
 - Alliteration is too strong/weak.
   Toggle A to disable/enable, or add more 1-syllable content words with varied initials.

🧰 Power tips
 - Use weights to “tune the voice”:
   breeze|noun|1|EEZ|3 (common), ocean|noun|2|OH|1 (rarer).
 - Make theme-specific templates (e.g., Poe uses more adv and prep, Nature uses more adj).
 - Add multi-word nouns for richer images: wide sky|noun|2|AI, broken light|noun|3|ITE.
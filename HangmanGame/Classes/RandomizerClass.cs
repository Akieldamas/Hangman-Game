using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Hangman.Classes
{
    public class RandomizerClass
    {
        string FileText;
        string[] Mots;
        string WordsFile = "words.txt";

        public RandomizerClass() {}

        public string GetRandomWord()
        {
            FileText = File.ReadAllText(WordsFile);
            Mots = FileText.Split(' ');

            Random random = new Random();
            int randomIndex = random.Next(0, Mots.Length);

            return Mots[randomIndex].ToUpper();
        }

        public char GetRandomLetter(string RandomWord, string HiddenWord)
        {
            int SacrificeIndex = HiddenWord.IndexOf("*");
            char letter = RandomWord[SacrificeIndex];
            Debug.WriteLine(letter);
            return letter;
        }
    }
}

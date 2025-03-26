using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Diagnostics.Metrics;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Diagnostics.Eventing.Reader;
using System.ComponentModel;
using Hangman.Classes;
using System.Media;
using Microsoft.Win32;

namespace Hangman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        DispatcherTimer timer;
        // Initialize all values
        string RandomWord;
        string HiddenWord;
        char[] RandomWordArray;
        int index;

        bool SoundIsPlaying = false;
        String AudioPath = @"Resource\Sounds\BGM_Music.mp3";
        MediaPlayer BGMPlayer = new MediaPlayer();
        MediaPlayer SoundEffect = new MediaPlayer();

        RandomizerClass Randomizer = new RandomizerClass(); // Class used to get a random word and a random letter for the sacrifice button

        int CurrentLives = 6;
        int MaxLives = 6;

        int timeleft;
        int SacrificeIndex;



        public void NewGame() // Function to restart the game instead of closing and reopening each time, initializes everything back to default
        {
            foreach (var btn in LettersGrid.Children.OfType<Button>())
            {
                btn.IsEnabled = true;
            }
            HelpSacrifice.IsEnabled = true;

            RandomWord =Randomizer.GetRandomWord();

            Debug.WriteLine(RandomWord);
            CurrentLives = MaxLives;
            HiddenWord = new string('*', RandomWord.Length);
            LabelLives.Content = "Lives " + CurrentLives + "/" + MaxLives;
            WordTextbox.Text = HiddenWord;

            Uri resourceUri = new Uri(@"Resource/images/character_" + CurrentLives + ".png", UriKind.Relative); // Uri = Adresse lien de l'image
            character.Source = new BitmapImage(resourceUri); // bitmap is a object tha we use here to change the image

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_tick);
            timeleft = 90;
            timer.Start();
            if (SoundIsPlaying == false)
            {
                PlayMusic();
            }

        }
        public MainWindow()
        {
            InitializeComponent();
            NewGame();

            BGMPlayer.MediaEnded += new EventHandler(Media_Ended); // Creer evenement pour relancer la musique quand elle est finie
        }

        void timer_tick(object sender, EventArgs e) // Countdown timer
        {
            VictoryLossChecker();
            if (timeleft > 0 && CurrentLives > 0 && HiddenWord.Contains("*") == true)
            {
                timeleft = timeleft - 1;
                TimerLabel.Content = string.Format("0{0}:{1:00}", timeleft / 60, timeleft % 60);
            }
            else if (timeleft == 0)
            {
                timer.Stop();
            }
            else if (timeleft <= 10 && CurrentLives > 0)
            {
                timeleft = timeleft - 1;
                TimerLabel.Content = string.Format("0{0}:{1:00}", timeleft / 60, timeleft % 60);
            }

            //CommandManager.InvalidateRequerySuggested();
        }

        void Media_Ended(object sender, EventArgs e) // Function to restart the music when it ends
        {
            BGMPlayer.Position = TimeSpan.Zero;
            BGMPlayer.Play();
        }

        private void PlayMusic() // Function to play the music
        {
            BGMPlayer.Volume = 0.1;
            BGMPlayer.Open(new Uri(AudioPath, UriKind.Relative));
            SoundIsPlaying = true;
            BGMPlayer.Play();

        }
        private void StopMusic() // Function to stop the music
        {
            BGMPlayer.Stop();
            SoundIsPlaying = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e) // All button letters
        {
            Button btn = sender as Button;
            String btnContent = btn.Content.ToString();

            char letter = Char.Parse(btnContent);

            Debug.WriteLine(btnContent);
            
            if (timeleft > 0 && HiddenWord.Contains("*") == true)
            {
                btn.IsEnabled = false;
                if (RandomWord.Contains(letter) && CurrentLives > 0)
                {
                    WordChecker(letter);
                }
                else if (RandomWord.Contains(letter) == false && CurrentLives > 0)
                {
                    LivesCounter();
                }

            }
            
        }
        private void WordChecker(char letter) // Checks if the letter is in the word and multiple times and changes
        {
            StringBuilder newHiddenWord = new StringBuilder(HiddenWord); // We have to use object: stringbuilder (other things are possible) so we
                                                                         // can replace the letter found to the other because strings are immutable

            for (int i = 0; i < RandomWord.Length; i++)
            {
                if (RandomWord[i] == letter)
                {
                    newHiddenWord[i] = letter;
                }
                HiddenWord = newHiddenWord.ToString();
                WordTextbox.Text = HiddenWord;
                VictoryLossChecker();
            }
        }
        private void LivesCounter() // Counter for lives
        {
            if (CurrentLives > 0)
            {
                CurrentLives -= 1;
                LabelLives.Content = "Lives " + CurrentLives + "/" + MaxLives;
                character.Source = new BitmapImage(new Uri(@"Resource/images/character_" + CurrentLives + ".png", UriKind.Relative));
                Debug.WriteLine(character.Source.ToString());
                VictoryLossChecker();
            }
        }

        private void VictoryLossChecker() // Checks if the player wins or not depending on lives, time and if the hiddenword still has *
        {   
            if (CurrentLives > 0 && timeleft > 0 && HiddenWord.Contains("*") == false)
            {
                timer.Stop();
                WordTextbox.Text = "YOU WIN (" + RandomWord + ")";
                SoundEffect.Volume = 0.1;
                SoundEffect.Open(new Uri(@"Resource\Sounds\WinSFX.mp3", UriKind.Relative));
                SoundEffect.Play();
            }
            else if (CurrentLives == 0 && HiddenWord.Contains("*") == true || timeleft <= 0 && HiddenWord.Contains("*") == true)
            {
                timer.Stop();
                WordTextbox.Text = "YOU LOST (" + RandomWord + ")";
            }
        }

       
        private void ReplayButton_Click(object sender, RoutedEventArgs e) // Stops time to prevent bugs and restarts the game
        {
            timer.Stop();
            NewGame();
        }

        private void HelpSacrifice_Click(object sender, RoutedEventArgs e) // Sacrifice 1 life for a random letter (risky move)
        {
            if (CurrentLives > 1 && HiddenWord.Contains("*") == true)
            {
                LivesCounter();
                char letter = Randomizer.GetRandomLetter(RandomWord, HiddenWord);
                WordChecker(letter);
                HelpSacrifice.IsEnabled = false;
            }
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e) // Button to play or stop the music
        {
            if (SoundIsPlaying == false)
            {
                PlayMusic();
            }
            else
            {
                StopMusic();
            }    
        }
    }
}

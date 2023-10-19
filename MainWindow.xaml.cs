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

namespace Hangman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;

        string FileText;
        string[] Mots;
        string WordsFile = "words.txt";

        string RandomWord;

        string HiddenWord;
        char[] RandomWordArray;
        int index;

        int CurrentLives = 6;
        int MaxLives = 6;

        int timeleft;

        public void NewGame()
        {

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();
            timeleft = 151;

             foreach (var btn in LettersGrid.Children.OfType<Button>())
            {
                  btn.IsEnabled = true;
            }

            HelpSacrifice.IsEnabled = true;


            FileText = File.ReadAllText(WordsFile);
            Mots = FileText.Split(' ');

            Random random = new Random();
            int randomIndex = random.Next(0, Mots.Length);

            RandomWord = Mots[randomIndex].ToUpper();

            Debug.WriteLine(RandomWord);
            CurrentLives = MaxLives;
            HiddenWord = new string('*', RandomWord.Length);
            LabelLives.Content = "Lives " + CurrentLives + "/" + MaxLives;
            WordTextbox.Text = HiddenWord;
            Uri resourceUri = new Uri(@"images/character_" + CurrentLives + ".png", UriKind.Relative);
            character.Source = new BitmapImage(resourceUri);
            

            hangman.Visibility = Visibility.Visible;

        }
        public MainWindow()
        {
            InitializeComponent();
            NewGame();
        }

        void timer_tick(object sender, EventArgs e)
        {
            if (timeleft > 0 && CurrentLives > 0 && HiddenWord.Contains("*") == true)
            {
                timeleft = timeleft - 1;
                TimerLabel.Content = string.Format("0{0}:{1:00}", timeleft / 60, timeleft % 60);
            }
            else if (timeleft <= 10 && CurrentLives > 0)
            {
                timeleft = timeleft - 1;
                TimerLabel.Content = string.Format("0{0}:{1:00}", timeleft / 60, timeleft % 60);
            }
            else
            {
                timer.Stop();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
        private void WordChecker(char letter)
        {
            StringBuilder newHiddenWord = new StringBuilder(HiddenWord);

            for (int i = 0; i < RandomWord.Length; i++)
            {
                if (RandomWord[i] == letter)
                {
                    newHiddenWord[i] = letter;
                }
                HiddenWord = newHiddenWord.ToString();
                WordTextbox.Text = HiddenWord;
            }
        }
        private void LivesCounter()
        {
            if (CurrentLives > 0)
            {
                CurrentLives -= 1;
                LabelLives.Content = "Lives " + CurrentLives + "/" + MaxLives;
                character.Source = new BitmapImage(new Uri(@"images/character_" + CurrentLives + ".png", UriKind.Relative));
                Debug.WriteLine(character.Source.ToString());
            }
        }

       
        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            NewGame();
        }

        private void HelpSacrifice_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLives > 1)
            {
                LivesCounter();
                
                if (HiddenWord.Contains("*") == true)
                {
                    LabelLives.Content = "Lives " + CurrentLives + "/" + MaxLives;
                    Random randomLetter = new Random();
                    int Index = randomLetter.Next(RandomWord.Length);

                    char letter = RandomWord[Index];
                    Debug.WriteLine(letter);
                    WordChecker(letter);
                    HelpSacrifice.IsEnabled = false;
                }

            }
        }
    }
}

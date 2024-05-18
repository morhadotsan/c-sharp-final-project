using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class BFAWindow : Window
    {
        public BFAWindow()
        {
            InitializeComponent();
            ThreadsComboBox.SelectedIndex = 0; // Default to 2 thread
        }

        private async void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            // Show progress bar and status label
            DecryptionProgressBar.Visibility = Visibility.Visible;
            DecryptionStatusLabel.Visibility = Visibility.Visible;

            string textToDecrypt = TextToDecryptTextBox.Text;
            int charLength = VarClass.ProgCharLength(); // length of char to generate
            string targetString = TextToDecryptTextBox.Text; // target string to search for
            string progSalt = VarClass.ProgSalt();
            string progPass = VarClass.ProgPassword();
            int numberOfThreads = int.Parse((ThreadsComboBox.SelectedItem as ComboBoxItem).Content.ToString());

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool decryptionSuccessful = await Task.Run(() =>
            {
                Thread[] threads = new Thread[numberOfThreads];

                for (int i = 0; i < numberOfThreads; i++)
                {
                    threads[i] = new Thread(() => BFAClass.SearchInCombinations(targetString, charLength, progPass, progSalt));
                    threads[i].Start();
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                return BFAClass.SearchInCombinations(targetString, charLength, progPass, progSalt);
            });

            stopwatch.Stop();
            TimeSpan timeElapsed = stopwatch.Elapsed;

            // Hide progress bar and status label
            DecryptionProgressBar.Visibility = Visibility.Collapsed;
            DecryptionStatusLabel.Visibility = Visibility.Collapsed;

            if (decryptionSuccessful)
            {
                string decText = File.ReadAllText("decrypted_text.txt");

                // message box with decryption details
                MessageBox.Show($"Number of Threads used: {numberOfThreads}\n" +
                                $"Decryption Successful!\n" +
                                $"Text Decrypted: {decText}\n" +
                                $"Time Taken: {timeElapsed.TotalSeconds:F3} seconds",
                                "Decryption Completed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
            {
                // message box with decryption failure details
                MessageBox.Show($"Number of Threads used: {numberOfThreads}\n" +
                                $"Decryption Was Not Successful!\n" +
                                $"Time Taken: {timeElapsed.TotalSeconds:F3} seconds",
                                "Decryption Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
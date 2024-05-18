using System.Data.SQLite;
using System.Windows;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        private const string DatabaseName = "Accounts.db";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseName};Version=3;"))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (Id INTEGER PRIMARY KEY, AccountName TEXT, Password TEXT)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string accountName = AccountNameTextBox.Text;
            string password = PasswordTextBox.Password;

            string progSalt = VarClass.ProgSalt();
            string progPass = VarClass.ProgPassword();
            string encryptedPassword = AesClass.Encrypt(password, progPass, progSalt);

            using (var connection = new SQLiteConnection($"Data Source={DatabaseName};Version=3;"))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Accounts (AccountName, Password) VALUES (@AccountName, @Password)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountName", accountName);
                    command.Parameters.AddWithValue("@Password", encryptedPassword);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Account password saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clears the text boxes after the message box is closed
            AccountNameTextBox.Clear();
            PasswordTextBox.Clear();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var viewWindow = new ViewWindow();
            viewWindow.Show();
        }

        private void ClearDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all records from the database?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var connection = new SQLiteConnection($"Data Source={DatabaseName};Version=3;"))
                {
                    connection.Open();
                    string clearTableQuery = "DELETE FROM Accounts";
                    using (var command = new SQLiteCommand(clearTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("All records have been cleared from the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenDecryptWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var decryptWindow = new BFAWindow();
            decryptWindow.Show();
        }
    }
}
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class ViewWindow : Window
    {
        private const string DatabaseName = "Accounts.db";

        public ObservableCollection<Account> Accounts { get; set; }

        public ViewWindow()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            Accounts = new ObservableCollection<Account>();
            using (var connection = new SQLiteConnection($"Data Source={DatabaseName};Version=3;"))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Accounts";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string encryptedPassword = reader.GetString(2);
                            string progSalt = VarClass.ProgSalt();
                            string progPass = VarClass.ProgPassword();
                            string decryptedPassword = AesClass.Decrypt(encryptedPassword, progPass, progSalt);

                            Accounts.Add(new Account
                            {
                                Id = reader.GetInt32(0),
                                AccountName = reader.GetString(1),
                                EncryptedPassword = encryptedPassword,
                                DecryptedPassword = decryptedPassword
                            });
                        }
                    }
                }
            }
            AccountsDataGrid.ItemsSource = Accounts;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Account selectedAccount)
            {
                DeleteAccount(selectedAccount.Id);
                Accounts.Remove(selectedAccount);
            }
        }

        private void CopyEncPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Account selectedAccount)
            {
                Clipboard.SetText(selectedAccount.EncryptedPassword);
                MessageBox.Show("Encrypted password copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteAccount(int id)
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseName};Version=3;"))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Accounts WHERE Id = @Id";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string EncryptedPassword { get; set; }
        public string DecryptedPassword { get; set; }
    }
}
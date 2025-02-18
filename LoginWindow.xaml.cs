using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ListaSessji01
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        // Stała do przechowywania ścieżki do pliku konfiguracyjnego
        private const string ConfigFilePath = "config.json";
        // Inicjalizacja Loggera
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        // Klucz i wektor inicjalizacyjny do szyfrowania AES
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("your-key");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("your-key");
        // Konstruktor inicjalizuje komponenty okna i ładuje konfigurację z pliku
        public LoginWindow()
        {
            InitializeComponent();
            LoadConfig();
        }
        // Metoda wczytująca konfigurację z pliku          
        private void LoadConfig()
        {
            // Sprawdza czy plik istnieje
            if (File.Exists(ConfigFilePath))
            {
                // Jeśli tak to odczytuje jego zawartość i deserializuje do obiektu klasy Config
                try
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    Config config = JsonConvert.DeserializeObject<Config>(json);

                    // Ustawienie wartości pól na podstawie odczytanej konfiguracji
                    LoginTextBox.Text = config.Login;
                    PasswordBox.Password = DecryptString(config.Password); ;
                    HostName.Text = config.Hostname;
                    Ports.Text = config.Ports;
                    RefreshTime.Text = config.RefreshTime;
                }
                // Jeśli nie to wyświetla błąd
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd podczas ładowania konfiguracji: " + ex.Message);
                    //logger.Error("Błąd podczas ładowania konfiguracji: " + ex.Message);
                }
            }
        }
        // Metoda zapisująca konfigurację do pliku
        private void SaveConfig()
        {
            // Tworzy obiekt Config z wartościami pól tekstowych
            try
            {
                Config config = new Config
                {
                    Login = LoginTextBox.Text,
                    Password = EncryptString(PasswordBox.Password),
                    Hostname = HostName.Text,
                    Ports = Ports.Text,
                    RefreshTime = RefreshTime.Text
                };
                // Serializuje obiekt do formatu JSON i zapisuje do pliku
                string json = JsonConvert.SerializeObject(config);
                File.WriteAllText(ConfigFilePath, json);
                //MessageBox.Show("Konfiguracja zapisana pomyślnie.");
            }
            // Wyświetlenie komunikatu o błędzie w przypadku niepowodzenia
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas zapisywania konfiguracji: " + ex.Message);
                //logger.Error("Błąd podczas zapisywania konfiguracji: " + ex.Message);
            }
        }
        // Metoda do szyfrowania hasła
        private static string EncryptString(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
        // Metoda do deszyfrowania hasła
        private static string DecryptString(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        // Metoda do obsługi kliknięcia przycisku "Zaloguj"
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie danych z pól tekstowych
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;
            string hostname = HostName.Text;
            string refreshTime = RefreshTime.Text;
            // Oddziela porty od siebie (porty defaultowo powinny być rozdzielone przecinkiem)
            string[] ports = Ports.Text.Split(',');

            // Tworzenie pustego słownika sessionData
            var sessionData = new Dictionary<string, string>();
            var rClients = new Dictionary<string, RClient>();
            // Dla każdego portu tworzy RClient i pobiera listę sesji.
            foreach (var port in ports)
            {
                // Tworzy URL do zalogowania
                string restURL = $"http://{hostname}:{port}/connect/rest/";
                //logger.Trace($"restURL: {restURL}");
                var rClient = new RClient(restURL, login, password);
                // Weryfikuje poprawność logowania
                if (rClient.isCorrectlyAddress())
                {
                    // Zmienna do przechowywania odpowiedzi z serwera w postaci JSON
                    string sessionListJson = rClient.GetConnectionList();
                    // Sprawdza czy odpowiedź zawiera błąd
                    if (!sessionListJson.Contains("Błąd połączenia"))
                    {
                        sessionData[port] = sessionListJson;
                        rClients[port] = rClient;
                    }
                    else
                    {
                        MessageBox.Show($"Błąd podczas logowania na porcie {port}: " + sessionListJson);
                        //logger.Fatal($"Błąd podczas logowania na porcie {port}: " + sessionListJson);
                    }
                }
                else
                {
                    MessageBox.Show($"Niepoprawny adres serwera na porcie {port}.");
                    //logger.Fatal($"Niepoprawny adres serwera na porcie {port}.");
                }
            }

            if (sessionData.Count > 0)
            {
                // Jeśli logowanie się powiodło, zapisuje konfigurację
                //MessageBox.Show("Logowanie powiodło się!");
                SaveConfig();

                // Otwiera główne okno aplikacji jeżeli logowanie się powiodło
                MainWindow mainWindow = new MainWindow(sessionData, rClients, refreshTime);
                mainWindow.Show();
                this.Close();
            }
        }

        // Metoda do obsługi przycisku zamknięcia
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Metoda do obsługi przycisku zapisu konfiguracji
        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }
    }
}

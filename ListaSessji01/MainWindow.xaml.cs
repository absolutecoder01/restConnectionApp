using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace ListaSessji01
{
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, RClient> _rClients;
        private DispatcherTimer _timer;
        private Dictionary<string, string> _currentSessionIDs = new Dictionary<string, string>();
        private Dictionary<string, string> _selectedSessionIDs = new Dictionary<string, string>();
        
        public MainWindow(Dictionary<string, string> sessionData, Dictionary<string, RClient> rClients, string refreshTime)
        {
            InitializeComponent();
            _rClients = rClients;

            foreach (var port in _rClients.Keys)
            {
                InitializeSession(port);
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(refreshTime));
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.Closing += MainWindow_Closing;

            foreach (var port in sessionData.Keys)
            {
                ParseAndDisplayData(sessionData[port], port);
            }
        }

        private async void InitializeSession(string port)
        {
            try
            {
                var rClient = _rClients[port];
                string currentSessionJson = await Task.Run(() => rClient.GetCurrentSession());
                if (IsValidJson(currentSessionJson))
                {
                    var currentSessionData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(currentSessionJson);
                    if (currentSessionData != null && currentSessionData.ContainsKey("result") && currentSessionData["result"].Count > 0)
                    {
                        _currentSessionIDs[port] = currentSessionData["result"].FirstOrDefault();

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd inicjalizacji sesji dla portu {port}: {ex.Message}");
                MessageBox.Show($"Błąd inicjalizacji sesji dla portu {port}: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                CloseCurrentSession();
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas zamykania: {ex.Message}");
                MessageBox.Show($"Błąd podczas zamykania: {ex.Message}");
            }
        }
        public class SessionData
        {
            [JsonProperty("result")]
            public List<string> Result { get; set; }
        }

        private void CloseCurrentSession()
        {
            try
            {
                foreach (var rClient in _rClients.Values)
                {
                    string currentSessionJson = rClient.GetCurrentSession();
                    var sessionIDs = ExtractSessionIDs(currentSessionJson);

                    if (sessionIDs != null && sessionIDs.Count > 0)
                    {
                        foreach (var sessionId in sessionIDs)
                        {
                            if (!string.IsNullOrEmpty(sessionId) && !_currentSessionIDs.Values.Contains(sessionId))
                            {
                                if (IsValidSessionID(sessionId))
                                {
                                    rClient.CloseSession(sessionId);
                                    //HideEndSessionButton(sessionId);
                                }
                                else
                                {
                                    logger.Warn($"Nieprawidłowy format SessionID: {sessionId}");
                                    MessageBox.Show($"Nieprawidłowy format SessionID: {sessionId}");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Warn("Nie znaleziono identyfikatorów sesji w odpowiedzi JSON.");
                        //MessageBox.Show("No session IDs found in the JSON response.");
                    }
                }
                logger.Info("Bieżące sesje zostały zamknięte.");
            }
            catch (Exception ex)
            {
                logger.Fatal("Błąd podczas zamykania sesji: " + ex.Message);
                MessageBox.Show("Błąd podczas zamykania sesji: " + ex.Message);
            }
        }

        private List<string> ExtractSessionIDs(string json)
        {
            var sessionIDs = new List<string>();
            var matchCollection = System.Text.RegularExpressions.Regex.Matches(json, @"(\d+\.\d+\.\d+)");
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                sessionIDs.Add(match.Value);
            }
            return sessionIDs;
        }

        private bool IsValidJson(string json)
        {
            try
            {
                logger.Info($"Sprawdzanie JSON: {json}");
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                logger.Warn($"Nieprawidłowy format JSON: {json}. Error: {ex.Message}");
                // MessageBox.Show($"Invalid JSON format: {json}. Error: {ex.Message}");
                return false;
            }
        }


        private bool IsValidSessionID(string sessionId)
        {
            return !string.IsNullOrEmpty(sessionId) && sessionId.All(c => char.IsLetterOrDigit(c) || c == '.');
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                RefreshSessionList();
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas odliczania czasu: {ex.Message}");
                MessageBox.Show($"Błąd podczas odliczania czasu: {ex.Message}");
            }
        }

        private void RefreshSessionList()
        {
            // Save selected session IDs
            foreach (TabItem tabItem in SessionsTabControl.Items)
            {
                try
                {
                    if (tabItem.Content is DataGrid dataGrid && dataGrid.SelectedItem is Session selectedSession)
                    {
                        _selectedSessionIDs[tabItem.Header.ToString()] = selectedSession.SessionID;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Błąd podczas zapisywania wybranej sesji dla karty {tabItem.Header}: {ex.Message}");
                    MessageBox.Show($"Błąd podczas zapisywania wybranej sesji dla karty {tabItem.Header}: {ex.Message}");
                }
            }

            foreach (var port in _rClients.Keys)
            {
                try
                {
                    string sessionListJson = _rClients[port].GetConnectionList();
                    ParseAndDisplayData(sessionListJson, port);
                }
                catch (Exception ex)
                {
                    logger.Error($"Błąd podczas pobierania listy połączeń dla portu {port}: {ex.Message}");
                    MessageBox.Show($"Błąd podczas pobierania listy połączeń dla portu {port}: {ex.Message}");
                }
            }

            // Restore selected session IDs
            foreach (TabItem tabItem in SessionsTabControl.Items)
            {
                try
                {
                    if (tabItem.Content is DataGrid dataGrid && _selectedSessionIDs.TryGetValue(tabItem.Header.ToString(), out string selectedSessionID))
                    {
                        foreach (var item in dataGrid.Items)
                        {
                            if (item is Session session && session.SessionID == selectedSessionID)
                            {
                                dataGrid.SelectedItem = item;
                                dataGrid.ScrollIntoView(item);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Błąd podczas przywracania wybranej sesji dla karty {tabItem.Header}: {ex.Message}");
                    MessageBox.Show($"Błąd podczas przywracania wybranej sesji dla karty {tabItem.Header}: {ex.Message}");
                }
            }
        }

        public class Session
        {
            public string SessionID { get; set; }
            public string Protocol { get; set; }
            public string ClientIP { get; set; }
            public string User { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ExtraData { get; set; }
            public string Status { get; set; }
            public int LifeDuration { get; set; }
            public int ExpiresIn { get; set; }
            public string SessionLifetime { get; set; }
            public string ElapsedSinceLastActvity { get; set; }
            public string WorkOnPort { get; set; }
            public bool IsCurrentSession { get; set; }
        }

        private void ParseAndDisplayData(string json, string port)
        {
            try
            {
                var outerWrapper = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                var innerJson = outerWrapper["result"][0];
                List<Session> sessions = JsonConvert.DeserializeObject<List<Session>>(innerJson);
                DisplaySessionsAsTabs(sessions, port);
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas analizowania danych: {ex.Message}");
                MessageBox.Show($"Błąd podczas analizowania danych: {ex.Message}");
            }
        }

        private void DisplaySessionsAsTabs(List<Session> sessions, string port)
        {
            try
            {
                foreach (var session in sessions)
                {
                    logger.Info($"Session ID: {session.SessionID}, IsCurrentSession: {session.IsCurrentSession}");
                }

                string currentSessionId = _rClients[port].GetCurrentSession();
                foreach (var session in sessions)
                {
                    session.IsCurrentSession = session.SessionID.Trim().Equals(currentSessionId.Trim());
                }

                // Filter out the current session from the displayed sessions
                var filteredSessions = sessions.Where(s => !s.IsCurrentSession).ToList();

                var existingTabItem = SessionsTabControl.Items.Cast<TabItem>().FirstOrDefault(t => t.Header?.ToString() == $"Port {port}");

                DataGrid dataGrid;
                if (existingTabItem != null)
                {
                    // If the tab already exists, just update the data source
                    dataGrid = (DataGrid)((Grid)existingTabItem.Content).Children[0]; // Get the DataGrid from the Grid
                    dataGrid.ItemsSource = filteredSessions; // Update the ItemsSource directly
                }
                else
                {
                    // Create a new Grid instance
                    var grid = new Grid();

                    // Define row definitions
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // DataGrid row
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Button row

                    // Create a new DataGrid instance
                    dataGrid = new DataGrid
                    {
                        AutoGenerateColumns = false,
                        IsReadOnly = true,
                    };

                    // Define columns
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Id Sesji", Binding = new Binding("SessionID") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Protokół", Binding = new Binding("Protocol") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "IP", Binding = new Binding("ClientIP") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Użytkownik", Binding = new Binding("User  ") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Rozpoczęcie sesji", Binding = new Binding("CreatedOn") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ExtraData", Binding = new Binding("ExtraData") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Długość sesji", Binding = new Binding("LifeDuration") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Wygasa za", Binding = new Binding("ExpiresIn") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Długość życia sesji", Binding = new Binding("SessionLifetime") });
                    dataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Brak Aktywności",
                        Binding = new Binding("ElapsedSinceLastActvity") // Ensure this matches the property name
                    });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Port", Binding = new Binding("WorkOnPort") });

                    // Create a DataGridTemplateColumn for the End Session button
                    DataGridTemplateColumn endSessionColumn = new DataGridTemplateColumn
                    {
                        Header = "Akcja",
                        CellTemplate = CreateEndSessionButtonTemplate()
                    };
                    dataGrid.Columns.Add(endSessionColumn);

                    // Add the DataGrid to the first row of the Grid
                    Grid.SetRow(dataGrid, 0);
                    grid.Children.Add(dataGrid);

                    // Create the End All Sessions button
                    Button endAllSessionsButton = new Button
                    {
                        Content = "Zakończ wszystkie sesje",
                        Style = Application.Current.Resources["EndAllSessionsButtonStyle"] as Style // Optional: Define a style in XAML
                    };
                    endAllSessionsButton.Click += (s, e) => EndAllSessionsForPort(port);

                    // Add the button to the second row of the Grid
                    Grid.SetRow(endAllSessionsButton, 1);
                    grid.Children.Add(endAllSessionsButton);

                    // Create a new TabItem
                    var tabItem = new TabItem
                    {
                        Header = $"Port {port}",
                        Content = grid
                    };
                    SessionsTabControl.Items.Add(tabItem);
                }

                // Set the ItemsSource for the DataGrid
                dataGrid.ItemsSource = filteredSessions;

                // Restore selected session if applicable
                if (_selectedSessionIDs.TryGetValue($"Port {port}", out string selectedSessionID))
                {
                    foreach (var item in dataGrid.Items)
                    {
                        if (item is Session session && session.SessionID == selectedSessionID)
                        {
                            dataGrid.SelectedItem = item;
                            dataGrid.ScrollIntoView(item);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd wyświetlania sesji dla portu {port}: {ex.Message}");
                MessageBox.Show($"Błąd wyświetlania sesji dla portu {port}: {ex.Message}");
            }
        }
        private void EndAllSessionsForPort(string port)
        {
            try
            {
                var rClient = _rClients[port];
                string currentSessionId = rClient.GetCurrentSession(); // Get the current session ID
                var sessionIDs = ExtractSessionIDs(rClient.GetConnectionList()); // Get all session IDs

                // Iterate through all session IDs and close them, except for the current session
                foreach (var sessionId in sessionIDs)
                {
                    if (!string.IsNullOrEmpty(sessionId) && sessionId != currentSessionId) // Skip the current session
                    {
                        if (IsValidSessionID(sessionId))
                        {
                            rClient.CloseSession(sessionId);
                        }
                        else
                        {
                            logger.Warn($"Nieprawidłowy format SessionID: {sessionId}");
                            MessageBox.Show($"Nieprawidłowy format SessionID {sessionId}");
                        }
                    }
                }

                // Refresh the session list to reflect the changes
                RefreshSessionList();
                MessageBox.Show($"Wszystkie sesje dla portu {port} zostały zamknięte, z wyjątkiem sesji bieżącej.");
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas kończenia wszystkich sesji dla portu {port}: {ex.Message}");
                MessageBox.Show($"Błąd podczas kończenia wszystkich sesji dla portu {port}: {ex.Message}");
            }
        }
        private DataTemplate CreateEndSessionButtonTemplate()
        {
            var template = new DataTemplate();
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var buttonFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Button)); // Specify WPF Button
            buttonFactory.SetValue(Button.ContentProperty, "Zakończ sesję");
            buttonFactory.SetValue(Button.CommandParameterProperty, new Binding("SessionID"));
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(EndSessionButton_Click));

            // Apply the style to the button
            buttonFactory.SetValue(Button.StyleProperty, Application.Current.Resources["EndSessionButtonStyle"] as Style);

            var visibilityBinding = new Binding("IsCurrentSession")
            {
                Converter = new BooleanToVisibilityConverter(),
                ConverterParameter = false // Visible if IsCurrentSession is false
            };
            buttonFactory.SetBinding(Button.VisibilityProperty, visibilityBinding);

            stackPanelFactory.AppendChild(buttonFactory);
            template.VisualTree = stackPanelFactory;
            return template;
        }

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is string sessionId)
                {
                    if (!_currentSessionIDs.Values.Contains(sessionId))
                    {
                        EndSession(sessionId);
                    }
                    else
                    {
                        logger.Warn("Próba zamknięcia własnej sesji została zablokowana.");
                        MessageBox.Show("Próba zamknięcia własnej sesji została zablokowana.");
                    }
                }
                else
                {
                    logger.Error("Błąd podczas kończenia sesji.");
                    MessageBox.Show("Błąd podczas kończenia sesji.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas kończenia sesji: {ex.Message}");
                MessageBox.Show($"Błąd podczas kończenia sesji: {ex.Message}");
            }
        }

        private void EndSession(string sessionId)
        {
            try
            {
                foreach (var rClient in _rClients.Values)
                {
                    rClient.CloseSession(sessionId);
                }
                RefreshSessionList();
            }
            catch (Exception ex)
            {
                logger.Error($"Błąd podczas kończenia sesji {sessionId}: " + ex.Message);
                MessageBox.Show($"Błąd podczas kończenia sesji {sessionId}: " + ex.Message);
            }
        }
    }
}
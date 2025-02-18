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
                logger.Error($"Error initializing session for port {port}: {ex.Message}");
                MessageBox.Show($"Error initializing session for port {port}: {ex.Message}");
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
                logger.Error($"Error during closing: {ex.Message}");
                MessageBox.Show($"Error during closing: {ex.Message}");
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
                                    logger.Warn($"Invalid SessionID format: {sessionId}");
                                    MessageBox.Show($"Invalid SessionID format: {sessionId}");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Warn("No session IDs found in the JSON response.");
                        //MessageBox.Show("No session IDs found in the JSON response.");
                    }
                }
                logger.Info("Current sessions have been closed.");
            }
            catch (Exception ex)
            {
                logger.Fatal("Error closing sessions: " + ex.Message);
                MessageBox.Show("Error closing sessions: " + ex.Message);
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
                logger.Info($"Checking JSON: {json}");
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                logger.Warn($"Invalid JSON format: {json}. Error: {ex.Message}");
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
                logger.Error($"Error during timer tick: {ex.Message}");
                MessageBox.Show($"Error during timer tick: {ex.Message}");
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
                    logger.Error($"Error saving selected session for tab {tabItem.Header}: {ex.Message}");
                    MessageBox.Show($"Error saving selected session for tab {tabItem.Header}: {ex.Message}");
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
                    logger.Error($"Error getting connection list for port {port}: {ex.Message}");
                    MessageBox.Show($"Error getting connection list for port {port}: {ex.Message}");
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
                    logger.Error($"Error restoring selected session for tab {tabItem.Header}: {ex.Message}");
                    MessageBox.Show($"Error restoring selected session for tab {tabItem.Header}: {ex.Message}");
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
            public int ElapsedSinceLastActivity { get; set; }
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
                logger.Error($"Error parsing data: {ex.Message}");
                MessageBox.Show($"Error parsing data: {ex.Message}");
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
                foreach (var session in sessions)
                {
                    // Uzyskaj ID aktualnej sesji
                    string currentSessionId = _rClients[port].GetCurrentSession();


                    // Sprawdź, czy ID sesji jest równe ID aktualnej sesji
                    session.IsCurrentSession = session.SessionID.Trim().Equals(currentSessionId.Trim());

                    if (session.IsCurrentSession)
                    {
                        session.IsCurrentSession = true;
                        logger.Info($"Session ID {session.SessionID} is current.");
                    }
                    else
                    {
                        session.IsCurrentSession = false;
                        logger.Info($"Session ID {session.SessionID} is not current.");
                    }
                }

                var existingTabItem = SessionsTabControl.Items.Cast<TabItem>().FirstOrDefault(t => t.Header?.ToString() == $"Port {port}");

                DataGrid dataGrid;
                if (existingTabItem != null)
                {
                    dataGrid = (DataGrid)existingTabItem.Content;
                    dataGrid.ItemsSource = null; // Oczyść stare przed dodaniem nowych
                }
                else
                {
                    dataGrid = new DataGrid
                    {
                        AutoGenerateColumns = false,
                        IsReadOnly = true
                    };

                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "SessionID", Binding = new Binding("SessionID") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Protocol", Binding = new Binding("Protocol") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ClientIP", Binding = new Binding("ClientIP") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "User", Binding = new Binding("User") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "CreatedOn", Binding = new Binding("CreatedOn") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ExtraData", Binding = new Binding("ExtraData") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "LifeDuration", Binding = new Binding("LifeDuration") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ExpiresIn", Binding = new Binding("ExpiresIn") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "SessionLifetime", Binding = new Binding("SessionLifetime") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ElapsedSinceLastActivity", Binding = new Binding("ElapsedSinceLastActivity") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "WorkOnPort", Binding = new Binding("WorkOnPort") });

                    DataGridTemplateColumn endSessionColumn = new DataGridTemplateColumn
                    {
                        Header = "Action",
                        CellTemplate = CreateEndSessionButtonTemplate()
                    };

                    dataGrid.Columns.Add(endSessionColumn);

                    var tabItem = new TabItem
                    {
                        Header = $"Port {port}",
                        Content = dataGrid
                    };
                    SessionsTabControl.Items.Add(tabItem);
                }

                dataGrid.ItemsSource = sessions;

                // Przywróć wybraną sesję
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
                logger.Error($"Error displaying sessions for port {port}: {ex.Message}");
                MessageBox.Show($"Error displaying sessions for port {port}: {ex.Message}");
            }
        }

        private DataTemplate CreateEndSessionButtonTemplate()
        {
            var template = new DataTemplate();
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "End Session");
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
                        logger.Warn("Attempt to close own session was blocked.");
                        MessageBox.Show("Attempt to close own session was blocked.");
                    }
                }
                else
                {
                    logger.Error("Error ending session.");
                    MessageBox.Show("Error ending session.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error ending session: {ex.Message}");
                MessageBox.Show($"Error ending session: {ex.Message}");
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
                logger.Error($"Error ending session {sessionId}: " + ex.Message);
                MessageBox.Show($"Error ending session {sessionId}: " + ex.Message);
            }
        }
    }
}
# restConnectionApp
An application that allows you to connect to a server, display sessions, and manage them. Developed as part of an internship at Streamsoft.

## Technologies Used:
- C# .NET 4.6 WPF
- RestSharp (for REST API communication)
- Newtonsoft.Json (for JSON parsing)
- NLog (for logging)
- ILMerge (for merging assemblies)
- Contura Fody (for weaving properties)

## Features

### Login Window

- **Configuration Management**: Saves configuration data in a JSON file for persistent settings.
- **Password Encryption**: Ensures secure handling of user credentials.
- **RESTful Login**: Utilizes a REST API to authenticate and manage user login.

<div align="center">
  <img src="https://github.com/user-attachments/assets/68489513-c964-4943-9ee7-2f68e6498af1" alt="Login Window" style="border-radius: 15px;" width="300" />
</div>

### Main Window

- **Dynamic Tabs for Ports**: Displays each port in a new tab for easy navigation.
- **JSON Data Parsing**: Parses and displays session data based on unique session IDs.
- **Session Management**: 
  - Each session has an “End Session” button for managing active sessions.
  - To avoid accidentally closing a session that's still in use, the button for the current session is hidden.

<div align="center">
  <img src="https://github.com/user-attachments/assets/17a8e90a-9771-4a1d-b2eb-7343cf909fcd" alt="Main Window" style="border-radius: 15px;" width="500" />
</div>

## Setup Instructions

1. **Clone the repository**:
   
    ```
    git clone https://github.com/absolutecoder01/restConnectionApp.git
    ```


2. **Open the Solution**:
- Open `ListaSessji01.csproj` in Visual Studio.

3. **Build the Project**:
- Compile the project and run the application.

<hr/>

# restConnectionApp Code Documentation

# `LoginWindow.cs`

The `LoginWindow.cs` file is responsible for the login window of the application. It handles user authentication, configuration file loading and saving, and AES encryption/decryption of user credentials. The main components include:

## Key Methods:

### `LoginWindow()`
- **Description**: Constructor that initializes the window components and loads the configuration from a file.
- **Functionality**: Calls `LoadConfig()` to populate the login fields with saved credentials if available.

### `private void LoadConfig()`
- **Description**: Loads the application configuration from a `config.json` file if it exists.
- **Functionality**: 
  - Checks if the configuration file exists.
  - Reads the file content and deserializes it into a `Config` object.
  - Populates the login form fields with the loaded configuration values (login, password, hostname, ports, refresh time).
  - Displays an error message if loading fails.

### `private void SaveConfig()`
- **Description**: Saves the user configuration (login credentials, hostname, ports, etc.) to a JSON file.
- **Functionality**: 
  - Creates a `Config` object with values from the input fields.
  - Serializes the object to JSON and writes it to the configuration file.
  - Displays an error message if saving fails.

### `private static string EncryptString(string plainText)`
- **Description**: Encrypts a plain text password using AES encryption.
- **Parameters**:
  - `plainText`: The password to encrypt.
- **Returns**: The encrypted password as a Base64 string.

### `private static string DecryptString(string cipherText)`
- **Description**: Decrypts an encrypted password from the configuration file back to plain text.
- **Parameters**:
  - `cipherText`: The encrypted password to decrypt.
- **Returns**: The decrypted plain text password.

### `private async void LoginButton_Click(object sender, RoutedEventArgs e)`
- **Description**: Handles the login button click event.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: 
  - Retrieves login data from the input fields.
  - Splits the ports input into an array.
  - Creates an empty dictionary for session data and RClient instances.
  - For each port, creates an RClient and verifies the login.
  - If successful, saves the configuration and opens the main application window.

### `private void CloseButton_Click(object sender, RoutedEventArgs e)`
- **Description**: Handles the close button click event.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: Closes the login window.

### `private void SaveConfigButton_Click(object sender, RoutedEventArgs e)`
- **Description**: Handles the save configuration button click event.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: Calls `SaveConfig()` to save the current configuration.

## Example of `LoginWindow.cs`:
```csharp
// Method to load configuration from file
private void LoadConfig()
{
    if (File.Exists(ConfigFilePath))
    {
        try
        {
            string json = File.ReadAllText(ConfigFilePath);
            Config config = JsonConvert.DeserializeObject<Config>(json);
            LoginTextBox.Text = config.Login;
            PasswordBox.Password = DecryptString(config.Password);
            HostName.Text = config.Hostname;
            Ports.Text = config.Ports;
            RefreshTime.Text = config.RefreshTime;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Błąd podczas ładowania konfiguracji: " + ex.Message);
        }
    }
}
```

<hr/>

# `MainWindow.cs`

The `MainWindow.cs` file is responsible for the main user interface of the application, managing session data from multiple RClient instances, and handling user interactions related to session management. The main components include:

## Key Methods:

### `MainWindow(Dictionary<string, string> sessionData, Dictionary<string, RClient> rClients, string refreshTime)`
- **Description**: Constructor that initializes the main window.
- **Parameters**:
  - `sessionData`: A dictionary containing session data.
  - `rClients`: A dictionary of RClient instances, keyed by port.
  - `refreshTime`: A string representing the refresh interval for session updates.
- **Functionality**: Initializes sessions for each port and sets up a timer for refreshing session data.

### `async void InitializeSession(string port)`
- **Description**: Asynchronously initializes a session for a specified port.
- **Parameters**:
  - `port`: The port for which to initialize the session.
- **Functionality**: Retrieves the current session data from the RClient and updates the `_currentSessionIDs` dictionary. Logs any errors encountered during initialization.

### `private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)`
- **Description**: Handles the window closing event.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: Calls `CloseCurrentSession()` to ensure all sessions are properly closed before the application exits.

### `private void CloseCurrentSession()`
- **Description**: Closes all current sessions for each RClient instance.
- **Functionality**: Extracts session IDs from the current session data and closes any sessions that are not currently active. Logs any errors or warnings encountered during the process.

### `private List<string> ExtractSessionIDs(string json)`
- **Description**: Extracts session IDs from a JSON string using regular expressions.
- **Parameters**:
  - `json`: The JSON string containing session data.
- **Returns**: A list of session IDs found in the JSON response.

### `private bool IsValidJson(string json)`
- **Description**: Validates whether a given string is a well-formed JSON.
- **Parameters**:
  - `json`: The JSON string to validate.
- **Returns**: `true` if valid, otherwise logs a warning and returns `false`.

### `private bool IsValidSessionID(string sessionId)`
- **Description**: Checks if a session ID is valid based on specific criteria.
- **Parameters**:
  - `sessionId`: The session ID to validate.
- **Returns**: `true` if the session ID is valid; otherwise, `false`.

### `private void Timer_Tick(object sender, EventArgs e)`
- **Description**: Handles the timer tick event for refreshing session data.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: Calls `RefreshSessionList()` to update the displayed session data.

### `private void RefreshSessionList()`
- **Description**: Refreshes the list of sessions for each port.
- **Functionality**: Saves the currently selected session IDs before refreshing. Restores the selected session IDs after updating the session data.

### `private void ParseAndDisplayData(string json, string port)`
- **Description**: Parses the JSON response containing session data and updates the UI.
- **Parameters**:
  - `json`: The JSON string containing session data.
  - `port`: The port associated with the session data.
- **Functionality**: Deserializes the session data into a list of `Session` objects and calls `DisplaySessionsAsTabs()` to show them in the UI.

### `private void DisplaySessionsAsTabs(List<Session> sessions, string port)`
- **Description**: Displays the session data in a tabbed interface.
- **Parameters**:
  - `sessions`: A list of `Session` objects to display.
  - `port`: The port associated with the sessions.
- **Functionality**: Creates a new tab for each port if it doesn't already exist and updates the DataGrid with the filtered list of sessions, excluding the current session.

### `private void EndAllSessionsForPort(string port)`
- **Description**: Closes all sessions for a specified port, except for the current session.
- **Parameters**:
  - `port`: The port for which to close all sessions.
- **Functionality**: Refreshes the session list after closing the sessions.

### `private DataTemplate CreateEndSessionButtonTemplate()`
- **Description**: Creates a DataTemplate for the "End Session" button in the DataGrid.
- **Returns**: A DataTemplate containing the button for ending a session.

### `private void EndSessionButton_Click(object sender, RoutedEventArgs e)`
- **Description**: Handles the click event for the "End Session" button.
- **Parameters**:
  - `sender`: The source of the event.
  - `e`: The event data.
- **Functionality**: Closes the specified session if it is not the current session.

### `private void EndSession(string sessionId)`
- **Description**: Closes a specific session by calling the `CloseSession` method on each RClient instance.
- **Parameters**:
  - `sessionId`: The ID of the session to close.
- **Functionality**: Refreshes the session list after closing the session.

## Example of `MainWindow.cs`:
```csharp
// Method to initialize a session for a specified port
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
```

<hr/>

# `Config.cs`

The `Config.cs` file defines the `Config` class, which serves as a data model for storing application configuration settings. This class encapsulates the user credentials and other settings required for the application to function properly.

## Class: `Config`

### Properties:

- **`string Login`**
  - **Description**: Stores the username used for logging into the application.

- **`string Password`**
  - **Description**: Stores the encrypted password associated with the user account.

- **`string Hostname`**
  - **Description**: Stores the hostname or IP address of the server to which the application connects.

- **`string Ports`**
  - **Description**: Stores a comma-separated list of ports used for connecting to the server.

- **`string RefreshTime`**
  - **Description**: Stores the refresh interval for updating session data, typically represented as a string (e.g., "5" for 5 seconds).

## Example of `Config.cs`:
```csharp
internal class Config
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Ports { get; set; }
    public string RefreshTime { get; set; }
}
```

<hr/>

# `SessionData.cs`

The `SessionData.cs` file defines the `SessionData` class, which represents the structure of session data returned from a server in JSON format. It also includes a custom JSON converter, `SessionDataConverter`, for serializing and deserializing `SessionData` objects.

## Class: `SessionData`

### Properties:

- **`List<string> Result`**
  - **Description**: A list that stores session IDs returned from the server. This property is mapped to the JSON property named "result".

## Class: `SessionDataConverter`

The `SessionDataConverter` class is a custom JSON converter that handles the serialization and deserialization of `SessionData` objects.

### Methods:

#### `public override bool CanConvert(Type objectType)`
- **Description**: Determines whether the converter can convert the specified object type.
- **Parameters**:
  - `objectType`: The type of the object to check.
- **Returns**: `true` if the object type is `SessionData`; otherwise, `false`.

#### `public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)`
- **Description**: Reads the JSON data and converts it into a `SessionData` object.
- **Parameters**:
  - `reader`: The JSON reader to read from.
  - `objectType`: The type of the object being deserialized.
  - `existingValue`: The existing value of the object being deserialized (can be null).
  - `serializer`: The serializer used for deserialization.
- **Returns**: A `SessionData` object populated with the session IDs from the JSON.

#### `public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)`
- **Description**: Writes a `SessionData` object as JSON.
- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `SessionData` object to serialize.
  - `serializer`: The serializer used for serialization.
- **Functionality**: Writes the `SessionData` object to JSON format, including the "result" property as an array of session IDs.

## Example of `SessionData.cs`:
```csharp
public class SessionData
{
    [JsonProperty("result")]
    public List<string> Result { get; set; }
}

public class SessionDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SessionData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var sessionData = new SessionData
        {
            Result = jsonObject["result"].Select(x => x.ToString()).ToList()
        };
        return sessionData;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var sessionData = (SessionData)value;
        writer.WriteStartObject();
        writer.WritePropertyName("result");
        writer.WriteStartArray();
        foreach (var sessionId in sessionData.Result)
        {
            writer.WriteValue(sessionId);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
```

<hr/>

# `BooleanToVisibilityConverter.cs`

The `BooleanToVisibilityConverter.cs` file defines the `BooleanToVisibilityConverter` class, which implements the `IValueConverter` interface. This converter is used in WPF applications to convert a boolean value to a `Visibility` enumeration value, allowing for dynamic UI element visibility based on boolean conditions.

## Class: `BooleanToVisibilityConverter`

### Methods:

#### `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- **Description**: Converts a boolean value to a `Visibility` value.
- **Parameters**:
  - `value`: The input value to convert (expected to be a boolean).
  - `targetType`: The type of the binding target property (not used in this implementation).
  - `parameter`: An optional parameter (not used in this implementation).
  - `culture`: The culture information (not used in this implementation).
- **Returns**: 
  - `Visibility.Collapsed` if the input value is `true`.
  - `Visibility.Visible` if the input value is `false`.
  - If the input value is not a boolean, it defaults to `Visibility.Collapsed`.

#### `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`
- **Description**: Converts a `Visibility` value back to a boolean value.
- **Functionality**: 
  - This method is not implemented and will throw a `NotImplementedException` if called. It is included to satisfy the `IValueConverter` interface.

## Example of `BooleanToVisibilityConverter.cs`:
```csharp
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return booleanValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

<hr/>

# `RClient.cs`

The `RClient.cs` file defines the `RClient` class, which is responsible for handling communication with a RESTful API. This class provides methods to retrieve connection lists, current sessions, and to close sessions using the RestSharp library.

## Class: `RClient`

### Fields:

- **`RestClient client`**
  - **Description**: The RestSharp client used to make HTTP requests to the REST API.

- **`string pragma`**
  - **Description**: A header value used in requests to maintain session state.

- **`static string pathGetConnectionList`**
  - **Description**: The endpoint path for retrieving the list of connections.

- **`static string pathGetCurrentSession`**
  - **Description**: The endpoint path for retrieving the current session.

- **`static string pathCloseSession`**
  - **Description**: The endpoint path for closing a session.

- **`static string pathPingRest`**
  - **Description**: The endpoint path for pinging the REST service (not used in the current implementation).

### Constructor:

#### `public RClient(string restURL, string login, string passw)`
- **Description**: Initializes a new instance of the `RClient` class.
- **Parameters**:
  - `restURL`: The base URL of the REST API.
  - `login`: The username for authentication.
  - `passw`: The password for authentication.
- **Functionality**: 
  - Creates a new `RestClient` instance and sets up basic authentication.
  - Displays an error message if the provided URL is invalid.

### Methods:

#### `public bool isCorrectlyAddress()`
- **Description**: Checks if the `RestClient` instance was created successfully.
- **Returns**: `true` if the client is not null; otherwise, `false`.

#### `public string GetConnectionList()`
- **Description**: Retrieves the list of connections from the API.
- **Returns**: A JSON string containing the list of connections.
- **Functionality**: 
  - Calls the appropriate method based on whether the `pragma` header has been set.

#### `private string GetFirstTimeSessionsListFromApi()`
- **Description**: Makes the first API call to retrieve the session list.
- **Returns**: A JSON string containing the session list or an error message.
- **Functionality**: 
  - Sends a GET request to the connection list endpoint and retrieves the `Pragma` header.

#### `private string GetSessionsListFromApi()`
- **Description**: Retrieves the session list using the `Pragma` header.
- **Returns**: A JSON string containing the session list.
- **Functionality**: Sends a GET request with the `Pragma` header included.

#### `public string GetCurrentSession()`
- **Description**: Retrieves the current session from the API.
- **Returns**: A JSON string containing the current session or an error message.
- **Functionality**: 
  - Calls the appropriate method based on whether the `pragma` header has been set.

#### `private string GetCurrentSessionWithPragma()`
- **Description**: Retrieves the current session using the `Pragma` header.
- **Returns**: A JSON string containing the current session.
- **Functionality**: Sends a GET request with the `Pragma` header included.

#### `private string GetCurrentSessionFirstConnect()`
- **Description**: Retrieves the current session on the first connection attempt.
- **Returns**: A JSON string containing the current session or an error message.
- **Functionality**: 
  - Sends a GET request to the current session endpoint and retrieves the `Pragma` header.

#### `public void CloseSession(string id)`
- **Description**: Closes a session identified by the given session ID.
- **Parameters**:
  - `id`: The ID of the session to close.
- **Functionality**: 
  - Sends a GET request to the close session endpoint with the session ID and the `Pragma` header.

## Example of `RClient.cs`:
```csharp
public class RClient
{
    private RestClient client;
    private string pragma;
    private static string pathGetConnectionList = "/GetConnectionsList";
    private static string pathGetCurrentSession = "/GetCurrentSession";
    private static string pathCloseSession = "/CloseSession";
    private static string pathPingRest = "/Ping/";

    public RClient(string restURL, string login, string passw)
    {
        try
        {
            client = new RestClient(restURL)
            {
                Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator(login, passw)
            };
        }
        catch
        {
            MessageBox.Show("Niepoprawny adres: " + restURL.Substring(0, restURL.Length - 12));
        }
    }

    public bool isCorrectlyAddress()
    {
        return client != null;
    }

    public string GetConnectionList()
    {
        string sessionListJson;
        if (pragma != null)
        {
            sessionListJson = GetSessionsListFromApi();
        }
        else
        {
            sessionListJson = GetFirstTimeSessionsListFromApi();
        }
        return sessionListJson;
    }

    private string GetFirstTimeSessionsListFromApi()
    {
        var request = new RestRequest(pathGetConnectionList, Method.GET);
        var response = client.Execute(request);
        string content = response.Content ?? string.Empty;
        try
        {
            pragma = response.Headers.FirstOrDefault(x => x.Name == "Pragma")?.Value?.ToString();
        }
        catch
        {
            return "Błąd połączenia z serwerem";
        }

        return content;
    }

    private string GetSessionsListFromApi()
    {
        var request = new RestRequest(pathGetConnectionList, Method.GET);
        request.AddHeader("Pragma", pragma);
        var response = client.Execute(request);
        string content = response.Content ?? string.Empty;

        return content;
    }

    public string GetCurrentSession()
    {
        if (pragma != null)
        {
            return GetCurrentSessionWithPragma();
        }
        return GetCurrentSessionFirstConnect();
    }

    private string GetCurrentSessionWithPragma()
    {
        var request = new RestRequest(pathGetCurrentSession, Method.GET);
        request.AddHeader("Pragma", pragma);
        var response = client.Execute(request);
        string content = response.Content ?? string.Empty;
        if (content.Length > 15) content = content.Substring(12, content.Length - 12 - 3);
        return content;
    }

    private string GetCurrentSessionFirstConnect()
    {
        var request = new RestRequest(pathGetCurrentSession, Method.GET);
        var response = client.Execute(request);
        string content = response.Content ?? string.Empty;
        try
        {
            pragma = response.Headers.FirstOrDefault(x => x.Name == "Pragma")?.Value?.ToString();
        }
        catch
        {
            return "Błąd połączenia z serwerem";
        }
        if (content.Length > 15) content = content.Substring(12, content.Length - 12 - 3);
        return content;
    }

    public void CloseSession(string id)
    {
        var request = new RestRequest(pathCloseSession + "/{id}", Method.GET);
        request.AddUrlSegment("id", id);
        request.AddHeader("Pragma", pragma);
        client.Execute(request);
    }
}
```

<hr/>

## Link for polish version

Polish documentation is avaible [here](documentation_pl.md).


## Author

The application was developed by [absolutecoder01](https://github.com/absolutecoder01)

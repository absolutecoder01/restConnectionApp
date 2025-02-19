# restConnectionApp
Aplikacja umożliwiająca połączenie się z serwerem, wyświetlanie sesji i zarządzanie nimi. Opracowana w ramach stażu w Streamsoft.

## Zastosowane technologie:
- C# .NET 4.6 WPF
- RestSharp (for REST API communication)
- Newtonsoft.Json (for JSON parsing)
- NLog (for logging)
- ILMerge (for merging assemblies)
- Contura Fody (for weaving properties)

## Cechy

### Okno logowania

- **Zarządzanie konfiguracją**: Zapisuje dane konfiguracyjne w pliku JSON dla trwałych ustawień.
- **Szyfrowanie hasła**: Zapewnia bezpieczne przetwarzanie danych uwierzytelniających użytkownika.
- **Logowanie RESTful**: Wykorzystuje interfejs API REST do uwierzytelniania i zarządzania logowaniem użytkownika.

<div align="center">
  <img src="https://github.com/user-attachments/assets/68489513-c964-4943-9ee7-2f68e6498af1" alt="Login Window" style="border-radius: 15px;" width="300" />
</div>

### Okno główne

- **Dynamiczne karty dla portów**: Wyświetla każdy port w nowej karcie, aby ułatwić nawigację.
- **Analiza danych JSON**: Analizuje i wyświetla dane sesji na podstawie unikalnych identyfikatorów sesji.
- **Zarządzanie sesją**:
- Każda sesja ma przycisk „Zakończ sesję” do zarządzania aktywnymi sesjami.
- Aby uniknąć przypadkowego zamknięcia sesji, która jest nadal używana, przycisk dla bieżącej sesji jest ukryty.


<div align="center">
  <img src="https://github.com/user-attachments/assets/17a8e90a-9771-4a1d-b2eb-7343cf909fcd" alt="Main Window" style="border-radius: 15px;" width="500" />
</div>

## Instrukcje konfiguracji

1. **Klonuj repozytorium**:
   
    ```
    git clone https://github.com/absolutecoder01/restConnectionApp.git
    ```


2. **Otwórz rozwiązanie**:
- Otwórz `ListaSessji01.csproj` w Visual Studio.

3. **Zbuduj projekt**:
- Skompiluj projekt i uruchom aplikację.

<hr/>

# restConnectionApp Dokumentacja kodu

# `LoginWindow.cs`

Plik `LoginWindow.cs` odpowiada za okno logowania aplikacji. Obsługuje uwierzytelnianie użytkownika, ładowanie i zapisywanie pliku konfiguracyjnego oraz szyfrowanie/odszyfrowywanie AES danych uwierzytelniających użytkownika. Główne komponenty obejmują:

## Kluczowe metody:

### `LoginWindow()`
- **Opis**: Konstruktor, który inicjuje komponenty okna i ładuje konfigurację z pliku.
- **Funkcjonalność**: Wywołuje `LoadConfig()`, aby wypełnić pola logowania zapisanymi danymi uwierzytelniającymi, jeśli są dostępne.

### `private void LoadConfig()`
- **Opis**: Ładuje konfigurację aplikacji z pliku `config.json`, jeśli istnieje.
- **Funkcjonalność**:
- Sprawdza, czy plik konfiguracyjny istnieje.
- Odczytuje zawartość pliku i deserializuje ją do obiektu `Config`.
- Wypełnia pola formularza logowania załadowanymi wartościami konfiguracji (login, hasło, nazwa hosta, porty, czas odświeżania).
- Wyświetla komunikat o błędzie, jeśli ładowanie się nie powiedzie.

### `private void SaveConfig()`
- **Opis**: Zapisuje konfigurację użytkownika (dane logowania, nazwę hosta, porty itp.) do pliku JSON.
- **Funkcjonalność**:
- Tworzy obiekt `Config` z wartościami z pól wejściowych.
- Serializuje obiekt do JSON i zapisuje go do pliku konfiguracyjnego.
- Wyświetla komunikat o błędzie, jeśli zapisywanie się nie powiedzie.

### `private static string EncryptString(string plainText)`
- **Opis**: Szyfruje hasło w postaci zwykłego tekstu za pomocą szyfrowania AES.
- **Parametry**:
- `plainText`: Hasło do zaszyfrowania.
- **Zwraca**: Zaszyfrowane hasło jako ciąg Base64.

### `private static string DecryptString(string cipherText)`
- **Opis**: Odszyfrowuje zaszyfrowane hasło z pliku konfiguracyjnego z powrotem do zwykłego tekstu.
- **Parametry**:
- `cipherText`: Zaszyfrowane hasło do odszyfrowania.
- **Zwraca**: Odszyfrowane hasło w postaci zwykłego tekstu.

### `private async void LoginButton_Click(object sender, RoutedEventArgs e)`
- **Opis**: Obsługuje zdarzenie kliknięcia przycisku logowania.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**:
- Pobiera dane logowania z pól wejściowych.
- Dzieli dane wejściowe portów na tablicę.
- Tworzy pusty słownik dla danych sesji i instancji RClient.
- Dla każdego portu tworzy RClient i weryfikuje logowanie.
- W przypadku powodzenia zapisuje konfigurację i otwiera główne okno aplikacji.

### `private void CloseButton_Click(object sender, RoutedEventArgs e)`
- **Opis**: Obsługuje zdarzenie kliknięcia przycisku zamykania.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**: Zamyka okno logowania.

### `private void SaveConfigButton_Click(object sender, RoutedEventArgs e)`
- **Opis**: Obsługuje zdarzenie kliknięcia przycisku zapisywania konfiguracji.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**: Wywołuje `SaveConfig()`, aby zapisać bieżącą konfigurację.

## Przykład `LoginWindow.cs`:
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
Plik `MainWindow.cs` odpowiada za główny interfejs użytkownika aplikacji, zarządzanie danymi sesji z wielu instancji RClient i obsługę interakcji użytkownika związanych z zarządzaniem sesją. Główne komponenty obejmują:

## Metody kluczowe:

### `MainWindow(Dictionary<string, string> sessionData, Dictionary<string, RClient> rClients, string refreshTime)`
- **Opis**: Konstruktor inicjujący okno główne.
- **Parametry**:
- `sessionData`: Słownik zawierający dane sesji.
- `rClients`: Słownik instancji RClient, kluczowany przez port.
- `refreshTime`: Ciąg reprezentujący interwał odświeżania dla aktualizacji sesji.
- **Funkcjonalność**: Inicjuje sesje dla każdego portu i ustawia timer odświeżania danych sesji.

### `async void InitializeSession(string port)`
- **Opis**: Asynchronicznie inicjuje sesję dla określonego portu.
- **Parametry**:
- `port`: Port, dla którego ma zostać zainicjowana sesja.
- **Funkcjonalność**: Pobiera bieżące dane sesji z RClient i aktualizuje słownik `_currentSessionIDs`. Rejestruje wszelkie błędy napotkane podczas inicjalizacji.

### `private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)`
- **Opis**: Obsługuje zdarzenie zamknięcia okna.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**: Wywołuje `CloseCurrentSession()`, aby upewnić się, że wszystkie sesje zostaną prawidłowo zamknięte przed zamknięciem aplikacji.

### `private void CloseCurrentSession()`
- **Opis**: Zamyka wszystkie bieżące sesje dla każdej instancji RClient.
- **Funkcjonalność**: Wyodrębnia identyfikatory sesji z bieżących danych sesji i zamyka wszystkie sesje, które nie są obecnie aktywne. Rejestruje wszystkie błędy lub ostrzeżenia napotkane podczas procesu.

### `private List<string> ExtractSessionIDs(string json)`
- **Opis**: Wyodrębnia identyfikatory sesji z ciągu JSON przy użyciu wyrażeń regularnych.
- **Parametry**:
- `json`: Ciąg JSON zawierający dane sesji.
- **Zwraca**: Listę identyfikatorów sesji znalezionych w odpowiedzi JSON.

### `private bool IsValidJson(string json)`
- **Opis**: Sprawdza, czy dany ciąg jest poprawnie sformatowanym ciągiem JSON.
- **Parametry**:
- `json`: ciąg JSON do walidacji.
- **Zwraca**: `true`, jeśli jest prawidłowy, w przeciwnym razie rejestruje ostrzeżenie i zwraca `false`.

### `private bool IsValidSessionID(string sessionId)`
- **Opis**: Sprawdza, czy identyfikator sesji jest prawidłowy na podstawie określonych kryteriów.
- **Parametry**:
- `sessionId`: identyfikator sesji do walidacji.
- **Zwraca**: `true`, jeśli identyfikator sesji jest prawidłowy; w przeciwnym razie `false`.

### `private void Timer_Tick(object sender, EventArgs e)`
- **Opis**: Obsługuje zdarzenie timera tick w celu odświeżenia danych sesji.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**: Wywołuje `RefreshSessionList()` w celu aktualizacji wyświetlanych danych sesji.

### `private void RefreshSessionList()`
- **Opis**: Odświeża listę sesji dla każdego portu.
- **Funkcjonalność**: Zapisuje aktualnie wybrane identyfikatory sesji przed odświeżeniem. Przywraca wybrane identyfikatory sesji po zaktualizowaniu danych sesji.

### `private void ParseAndDisplayData(string json, string port)`
- **Opis**: Analizuje odpowiedź JSON zawierającą dane sesji i aktualizuje interfejs użytkownika.
- **Parametry**:
- `json`: Ciąg JSON zawierający dane sesji.
- `port`: Port skojarzony z danymi sesji.
- **Funkcjonalność**: Deserializuje dane sesji do listy obiektów `Session` i wywołuje `DisplaySessionsAsTabs()`, aby wyświetlić je w interfejsie użytkownika.

### `private void DisplaySessionsAsTabs(List<Session> sessions, string port)`
- **Opis**: Wyświetla dane sesji w interfejsie z kartami.
- **Parametry**:
- `sessions`: Lista obiektów `Session` do wyświetlenia.
- `port`: Port skojarzony z sesjami.
- **Funkcjonalność**: Tworzy nową kartę dla każdego portu, jeśli jeszcze nie istnieje, i aktualizuje DataGrid za pomocą przefiltrowanej listy sesji, z wyłączeniem bieżącej sesji.

### `private void EndAllSessionsForPort(string port)`
- **Opis**: Zamyka wszystkie sesje dla określonego portu, z wyjątkiem bieżącej sesji.
- **Parametry**:
- `port`: Port, dla którego mają zostać zamknięte wszystkie sesje.
- **Funkcjonalność**: Odświeża listę sesji po zamknięciu sesji.

### `private DataTemplate CreateEndSessionButtonTemplate()`
- **Opis**: Tworzy DataTemplate dla przycisku „Zakończ sesję” w DataGrid.
- **Zwraca**: DataTemplate zawierający przycisk kończący sesję.

### `private void EndSessionButton_Click(object sender, RoutedEventArgs e)`
- **Opis**: Obsługuje zdarzenie kliknięcia dla przycisku „Zakończ sesję”.
- **Parametry**:
- `sender`: Źródło zdarzenia.
- `e`: Dane zdarzenia.
- **Funkcjonalność**: Zamyka określoną sesję, jeśli nie jest to bieżąca sesja.

### `private void EndSession(string sessionId)`
- **Opis**: Zamyka określoną sesję, wywołując metodę `CloseSession` na każdej sesji
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

Plik `Config.cs` definiuje klasę `Config`, która służy jako model danych do przechowywania ustawień konfiguracji aplikacji. Ta klasa hermetyzuje dane uwierzytelniające użytkownika i inne ustawienia wymagane do prawidłowego działania aplikacji.

## Klasa: `Config`

### Właściwości:

- **`string Login`**
- **Description**: Przechowuje nazwę użytkownika używaną do logowania się do aplikacji.

- **`string Password`**
- **Description**: Przechowuje zaszyfrowane hasło powiązane z kontem użytkownika.

- **`string Hostname`**
- **Description**: Przechowuje nazwę hosta lub adres IP serwera, z którym łączy się aplikacja.

- **`string Ports`**
- **Description**: Przechowuje rozdzieloną przecinkami listę portów używanych do łączenia się z serwerem.

- **`string RefreshTime`**
- **Opis**: Przechowuje interwał odświeżania w celu aktualizacji danych sesji, zwykle reprezentowany jako ciąg (np. „5” dla 5 sekund).

## Przykład `Config.cs`:
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

Plik `SessionData.cs` definiuje klasę `SessionData`, która reprezentuje strukturę danych sesji zwróconych z serwera w formacie JSON. Zawiera również niestandardowy konwerter JSON, `SessionDataConverter`, do serializacji i deserializacji obiektów `SessionData`.

## Klasa: `SessionData`

### Właściwości:

- **`List<string> Result`**
- **Description**: Lista przechowująca identyfikatory sesji zwrócone z serwera. Ta właściwość jest mapowana na właściwość JSON o nazwie „result”.

## Klasa: `SessionDataConverter`

Klasa `SessionDataConverter` to niestandardowy konwerter JSON, który obsługuje serializację i deserializację obiektów `SessionData`.

### Metody:

#### `public override bool CanConvert(Type objectType)`
- **Opis**: Określa, czy konwerter może konwertować określony typ obiektu.
- **Parametry**:
- `objectType`: Typ obiektu do sprawdzenia.
- **Zwraca**: `true`, jeśli typem obiektu jest `SessionData`; w przeciwnym razie `false`.

#### `public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)`
- **Opis**: Odczytuje dane JSON i konwertuje je na obiekt `SessionData`.
- **Parametry**:
- `reader`: Czytnik JSON, z którego należy odczytać.
- `objectType`: Typ obiektu, który jest deserializowany.
- `existingValue`: Istniejąca wartość obiektu, który jest deserializowany (może być null).
- `serializer`: Serializator używany do deserializacji.
- **Zwraca**: Obiekt `SessionData` wypełniony identyfikatorami sesji z JSON.

#### `public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)`
- **Opis**: Zapisuje obiekt `SessionData` jako JSON.
- **Parametry**:
- `writer`: Writer JSON, do którego ma zostać zapisany.
- `value`: Obiekt `SessionData` do serializacji.
- `serializer`: Serializator używany do serializacji.
- **Funkcjonalność**: Zapisuje obiekt `SessionData` w formacie JSON, w tym właściwość „result” jako tablicę identyfikatorów sesji.

## Przykład `SessionData.cs`:
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

Plik `BooleanToVisibilityConverter.cs` definiuje klasę `BooleanToVisibilityConverter`, która implementuje interfejs `IValueConverter`. Ten konwerter jest używany w aplikacjach WPF do konwersji wartości logicznej na wartość wyliczeniową `Visibility`, umożliwiając dynamiczną widoczność elementów interfejsu użytkownika na podstawie warunków logicznych.

## Klasa: `BooleanToVisibilityConverter`

### Metody:

#### `public object Convert(object value, Type targetType, object parameter, CultureInfo culture)`
- **Opis**: Konwertuje wartość logiczną na wartość `Visibility`.
- **Parametry**:
- `value`: Wartość wejściowa do konwersji (oczekiwana wartość logiczna).
- `targetType`: Typ właściwości docelowej powiązania (nieużywany w tej implementacji).
- `parameter`: Opcjonalny parametr (nieużywany w tej implementacji).
- `kultura`: Informacje o kulturze (nieużywane w tej implementacji).
- **Zwraca**:
- `Visibility.Collapsed` jeśli wartość wejściowa to `true`.
- `Visibility.Visible` jeśli wartość wejściowa to `false`.
- Jeśli wartość wejściowa nie jest wartością logiczną, domyślnie przyjmuje wartość `Visibility.Collapsed`.

#### `public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`
- **Opis**: Konwertuje wartość `Visibility` z powrotem na wartość logiczną.
- **Funkcjonalność**:
- Ta metoda nie jest zaimplementowana i wyrzuci `NotImplementedException` jeśli zostanie wywołana. Jest ona dołączona, aby spełnić wymagania interfejsu `IValueConverter`.

## Przykład `BooleanToVisibilityConverter.cs`:
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

Plik `RClient.cs` definiuje klasę `RClient`, która odpowiada za obsługę komunikacji z interfejsem API RESTful. Ta klasa udostępnia metody pobierania list połączeń, bieżących sesji i zamykania sesji przy użyciu biblioteki RestSharp.

## Klasa: `RClient`

### Pola:

- **`RestClient client`**
- **Description**: Klient RestSharp używany do wysyłania żądań HTTP do interfejsu API REST.

- **`string pragma`**
- **Description**: Wartość nagłówka używana w żądaniach do utrzymywania stanu sesji.

- **`static string pathGetConnectionList`**
- **Description**: Ścieżka punktu końcowego do pobierania listy połączeń.

- **`static string pathGetCurrentSession`**
- **Description**: Ścieżka punktu końcowego do pobierania bieżącej sesji.

- **`static string pathCloseSession`**
- **Opis**: Ścieżka punktu końcowego do zamykania sesji.

- **`static string pathPingRest`**
- **Opis**: Ścieżka punktu końcowego do pingowania usługi REST (nieużywana w bieżącej implementacji).

### Konstruktor:

#### `public RClient(string restURL, string login, string passw)`
- **Opis**: Inicjuje nową instancję klasy `RClient`.
- **Parametry**:
- `restURL`: Podstawowy adres URL interfejsu API REST.
- `login`: Nazwa użytkownika do uwierzytelniania.
- `passw`: Hasło do uwierzytelniania.
- **Funkcjonalność**:
- Tworzy nową instancję `RestClient` i konfiguruje podstawowe uwierzytelnianie.
- Wyświetla komunikat o błędzie, jeśli podany adres URL jest nieprawidłowy.

### Metody:

#### `public bool isCorrectlyAddress()`
- **Opis**: Sprawdza, czy instancja `RestClient` została pomyślnie utworzona.
- **Zwraca**: `true`, jeśli klient nie jest nullem; w przeciwnym razie `false`.

#### `public string GetConnectionList()`
- **Opis**: Pobiera listę połączeń z API.
- **Zwraca**: Ciąg JSON zawierający listę połączeń.
- **Funkcjonalność**:
- Wywołuje odpowiednią metodę w zależności od tego, czy nagłówek `pragma` został ustawiony.

#### `private string GetFirstTimeSessionsListFromApi()`
- **Opis**: Wykonuje pierwsze wywołanie API w celu pobrania listy sesji.
- **Zwraca**: Ciąg JSON zawierający listę sesji lub komunikat o błędzie.
- **Funkcjonalność**:
- Wysyła żądanie GET do punktu końcowego listy połączeń i pobiera nagłówek `Pragma`.

#### `private string GetSessionsListFromApi()`
- **Opis**: Pobiera listę sesji przy użyciu nagłówka `Pragma`.
- **Zwraca**: Ciąg JSON zawierający listę sesji.
- **Funkcjonalność**: Wysyła żądanie GET z dołączonym nagłówkiem `Pragma`.

#### `public string GetCurrentSession()`
- **Opis**: Pobiera bieżącą sesję z API.
- **Zwraca**: Ciąg JSON zawierający bieżącą sesję lub komunikat o błędzie.
- **Funkcjonalność**:
- Wywołuje odpowiednią metodę w zależności od tego, czy nagłówek `pragma` został ustawiony.

#### `private string GetCurrentSessionWithPragma()`
- **Opis**: Pobiera bieżącą sesję przy użyciu nagłówka `Pragma`.
- **Zwraca**: Ciąg JSON zawierający bieżącą sesję.
- **Funkcjonalność**: Wysyła żądanie GET z dołączonym nagłówkiem `Pragma`.

#### `private string GetCurrentSessionFirstConnect()`
- **Opis**: Pobiera bieżącą sesję przy pierwszej próbie połączenia.
- **Zwraca**: Ciąg JSON zawierający bieżącą sesję lub komunikat o błędzie.
- **Funkcjonalność**:
- Wysyła żądanie GET do bieżącego punktu końcowego sesji i pobiera nagłówek `Pragma`.

#### `public void CloseSession(string id)`
- **Opis**: Zamyka sesję zidentyfikowaną przez podany identyfikator sesji.
- **Parametry**:
- `id`: Identyfikator sesji do zamknięcia.
- **Funkcjonalność**:
- Wysyła żądanie GET do punktu końcowego sesji zamknięcia z identyfikatorem sesji i nagłówkiem `Pragma`.

## Przykład `RClient.cs`:
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

## Link do oryginalnej wersji

Oryginalna wersja dokumentacji jest dostępna [tutaj](README.md).


## Autor

Aplikację opracował [absolutecoder01](https://github.com/absolutecoder01)

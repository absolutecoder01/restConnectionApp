using RestSharp;
using System;
using System.Linq;
using System.Windows;

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

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using NLog;
//using RestSharp;
//using RestSharp.Authenticators;

//namespace ListaSessji01
//{
//    public class RClient
//    {
//        private RestClient client;
//        private string pragma;
//        private string login;
//        private string password;
//        private static string pathGetConnectionList = "TstAdminModule/GetConnectionsList";
//        private static string pathGetCurrentSession = "TstAdminModule/GetCurrentSession";
//        private static string pathCloseSession = "TstAdminModule/CloseSession";
//        private static string pathPingRest = "TstBaseMethods/Ping/";
//        private const string SessionTokenFilePath = "sessionToken.txt";
//        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

//        public RClient(string restURL, string login, string passw)
//        {
//            this.login = login;
//            this.password = passw;
//            try
//            {
//                client = new RestClient(restURL)
//                {
//                    Authenticator = new HttpBasicAuthenticator(login, passw)
//                };
//                LoadSessionToken(SessionTokenFilePath);
//            }
//            catch
//            {
//                MessageBox.Show("Niepoprawny adres: " + restURL.Substring(0, restURL.Length - 12));
//            }
//        }

//        public bool isCorrectlyAddress()
//        {
//            return client != null;
//        }

//        public string GetConnectionList()
//        {
//            if (!IsSessionTokenValid())
//            {
//                ReAuthenticate();
//            }

//            string sessionListJson;
//            if (pragma != null)
//            {
//                sessionListJson = GetSessionsListFromApi();
//            }
//            else
//            {
//                sessionListJson = GetFirstTimeSessionsListFromApi();
//            }
//            return sessionListJson;
//        }

//        private string GetFirstTimeSessionsListFromApi()
//        {
//            var request = new RestRequest(pathGetConnectionList, Method.GET);
//            var response = client.Execute(request);
//            string content = response.Content ?? string.Empty;
//            try
//            {
//                pragma = response.Headers.FirstOrDefault(x => x.Name == "Pragma")?.Value?.ToString();
//                SaveSessionToken(SessionTokenFilePath);
//            }
//            catch
//            {
//                return "Błąd połączenia z serwerem";
//            }

//            return content;
//        }

//        private string GetSessionsListFromApi()
//        {
//            var request = new RestRequest(pathGetConnectionList, Method.GET);
//            request.AddHeader("Pragma", pragma);
//            var response = client.Execute(request);
//            string content = response.Content ?? string.Empty;

//            return content;
//        }

//        public string GetCurrentSession()
//        {
//            if (!IsSessionTokenValid())
//            {
//                ReAuthenticate();
//            }

//            if (pragma != null)
//            {
//                return GetCurrentSessionWithPragma();
//            }
//            return GetCurrentSessionFirstConnect();
//        }

//        private string GetCurrentSessionWithPragma()
//        {
//            var request = new RestRequest(pathGetCurrentSession, Method.GET);
//            request.AddHeader("Pragma", pragma);
//            var response = client.Execute(request);
//            string content = response.Content ?? string.Empty;

//            // Log the raw JSON content for debugging purposes
//            logger.Info($"Raw JSON content received: {content}");

//            // Save the raw JSON content to a file for further examination
//            SaveRawJsonToFile(content);

//            // Validate JSON structure before parsing
//            if (!IsValidJson(content))
//            {
//                MessageBox.Show("Invalid JSON format received.");
//                return string.Empty;
//            }

//            // Try parsing the JSON and handle errors
//            try
//            {
//                var json = JToken.Parse(content);
//                var result = json["result"].ToString();

//                // Log and return the result
//                logger.Info($"Parsed result: {result}");
//                return result;
//            }
//            catch (JsonReaderException ex)
//            {
//                logger.Error($"Error parsing JSON: {ex.Message}\nContent: {content}");
//                MessageBox.Show($"Error parsing JSON: {ex.Message}\nJSON content: {content}");
//                return string.Empty;
//            }
//        }

//        private string GetCurrentSessionFirstConnect()
//        {
//            var request = new RestRequest(pathGetCurrentSession, Method.GET);
//            var response = client.Execute(request);
//            string content = response.Content ?? string.Empty;

//            // Log the raw JSON content for debugging purposes
//            logger.Info($"Raw JSON content received: {content}");

//            // Save the raw JSON content to a file for further examination
//            SaveRawJsonToFile(content);

//            // Validate JSON structure before parsing
//            if (!IsValidJson(content))
//            {
//                MessageBox.Show("Invalid JSON format received.");
//                return string.Empty;
//            }

//            // Try parsing the JSON and handle errors
//            try
//            {
//                var json = JToken.Parse(content);
//                var result = json["result"].ToString();

//                // Log and return the result
//                logger.Info($"Parsed result: {result}");
//                return result;
//            }
//            catch (JsonReaderException ex)
//            {
//                logger.Error($"Error parsing JSON: {ex.Message}\nContent: {content}");
//                MessageBox.Show($"Error parsing JSON: {ex.Message}\nJSON content: {content}");
//                return string.Empty;
//            }

//            try
//            {
//                pragma = response.Headers.FirstOrDefault(x => x.Name == "Pragma")?.Value?.ToString();
//                SaveSessionToken(SessionTokenFilePath);
//            }
//            catch
//            {
//                return "Błąd połączenia z serwerem";
//            }
//        }

//        // Method to validate JSON format
//        private bool IsValidJson(string content)
//        {
//            try
//            {
//                JToken.Parse(content);
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private void SaveRawJsonToFile(string content)
//        {
//            string filePath = "raw_json_response.txt";
//            try
//            {
//                using (StreamWriter writer = new StreamWriter(filePath, false))
//                {
//                    writer.Write(content);
//                }
//            }
//            catch (IOException ex)
//            {
//                logger.Error($"Error writing to file {filePath}: {ex.Message}");
//                MessageBox.Show($"Error writing to file {filePath}: {ex.Message}");
//            }
//        }


//        public void CloseSession(string id)
//        {
//            try
//            {
//                var request = new RestRequest(pathCloseSession + "/{id}", Method.GET);
//                request.AddUrlSegment("id", id);

//                // Sprawdź, czy id jest prawidłowym stringiem
//                if (string.IsNullOrEmpty(id))
//                {
//                    throw new FormatException("Nieprawidłowy format SessionID.");
//                }

//                request.AddHeader("Pragma", pragma);
//                client.Execute(request);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Błąd w funkcji zamknięcia sesji: " + ex.Message);
//            }
//        }

//        public void SaveSessionToken(string filePath)
//        {
//            File.WriteAllText(filePath, pragma);
//        }

//        public void LoadSessionToken(string filePath)
//        {
//            if (File.Exists(filePath))
//            {
//                pragma = File.ReadAllText(filePath);
//            }
//        }

//        private bool IsSessionTokenValid()
//        {
//            if (string.IsNullOrEmpty(pragma))
//            {
//                return false;
//            }

//            var request = new RestRequest(pathPingRest, Method.GET);
//            request.AddHeader("Pragma", pragma);
//            var response = client.Execute(request);

//            return response.IsSuccessful;
//        }

//        private void ReAuthenticate()
//        {
//            try
//            {
//                client.Authenticator = new HttpBasicAuthenticator(login, password);
//                var request = new RestRequest(pathGetCurrentSession, Method.GET);
//                var response = client.Execute(request);

//                if (response.IsSuccessful)
//                {
//                    pragma = response.Headers.FirstOrDefault(x => x.Name == "Pragma")?.Value?.ToString();
//                    SaveSessionToken(SessionTokenFilePath);
//                    //logger.Info("Re-authentication successful.");
//                }
//                else
//                {
//                    pragma = null;
//                    MessageBox.Show($"Re-authentication failed. Status Code: {response.StatusCode}, Content: {response.Content}");
//                    MessageBox.Show("Nie udało się ponownie uwierzytelnić. Proszę zalogować się ponownie.");
//                }
//            }
//            catch (Exception ex)
//            {
//                pragma = null;
//                //logger.Error($"Error during re-authentication: {ex.Message}");
//                MessageBox.Show("Błąd podczas ponownego uwierzytelniania: " + ex.Message);
//            }
//        }

//    }
//}

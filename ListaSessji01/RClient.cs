﻿using RestSharp;
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
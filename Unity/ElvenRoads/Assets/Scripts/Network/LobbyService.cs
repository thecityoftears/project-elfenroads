using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Web;
using Newtonsoft.Json.Serialization;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class LoginResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RegisterEntry
{
    public string Name { get; set; }
    public string Password { get; set; }
    public string PreferredColour { get; set; }
    public string Role { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class GameParameters
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Location { get; set; }
    public int MaxSessionPlayers { get; set; }
    public int MinSessionPlayers { get; set; }
    public string WebSupport { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class GameSession
{
    public string Creator { get; set; }
    public GameParameters GameParameters { get; set; }
    public List<string> Players { get; set; }
    public bool Launched { get; set; }
    public string Savegameid { get; set; }
    public Dictionary<string, string> PlayerLocations { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class SaveGame
{
    public string Gamename { get; set; }
    public List<string> Players { get; set; }
    public string SaveGameId { get; set; }
}

public static class LobbyService
{
    private static HttpClient h;
    private static string basicAuthV;
    //private static readonly string gameServiceName = "dummy"; REMOVED NOW THAT WE HAVE MANY GAME SERVICES, SEE GameCreation.cs for info how

    public static void Initialize(string url)
    {
        h = new HttpClient
        {
            BaseAddress = new System.Uri(url)
        };
        basicAuthV =
            "Basic " +
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes("bgp-client-name:bgp-client-pw")
            );
    }

    public static async Task<LoginResponse> Refresh()
    {
        var req = new HttpRequestMessage(
            HttpMethod.Post,
            h.BaseAddress + "oauth/token"
        )
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>() {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", Client.RefreshToken },
                }
            )
        };
        req.Headers.Add(
            "Authorization",
            basicAuthV
        );
        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();
        Debug.Log("login " + body);

        if (resp.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<LoginResponse>(body);
        }
        else
        {
            return null;
        }
    }

    public static async Task<LoginResponse> Login(string username, string password)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Post,
            h.BaseAddress + "oauth/token"
        )
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>() {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                }
            )
        };
        req.Headers.Add(
            "Authorization",
            basicAuthV
        );
        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();
        Debug.Log("login " + body);

        if (resp.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<LoginResponse>(body);
        }
        else
        {
            return null;
        }
    }

    public static async Task<Dictionary<string, GameSession>> GetSessions()
    {
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            h.BaseAddress + "api/sessions"
        );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("sessions " + body);

        if (resp.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<
                Dictionary<string, Dictionary<string, GameSession>>
            >(body).GetValueOrDefault("sessions");
        }
        else
        {
            return new Dictionary<string, GameSession>();
        }
    }

    public static async Task<SaveGame> GetSaveGame(string gamename, string saveGameID)
    {
        var req = new HttpRequestMessage(
                HttpMethod.Get,
                h.BaseAddress + "api/gameservices/" + gamename + "/savegames/" + saveGameID + "?access_token=" + Client.AccessToken
            );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("savegame: " + body);

        if (resp.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<SaveGame>(body);
        }
        else
        {
            Debug.Log("ERROR: could not get info for game: " + gamename);
            return null;
        }
    }

    public static async Task<Dictionary<string, List<SaveGame>>> GetSaveGames()
    {
        List<string> gameServiceNames = new List<string>()
        {
            "bGFuZDozOnRydWU", "bGFuZDozOmZhbHNl", "bGFuZDo0OnRydWU", "bGFuZDo0OmZhbHNl", "Z29sZDo2OnRydWU6dHJ1ZTp0cnVl",
            "Z29sZDo2OnRydWU6dHJ1ZTpmYWxzZQ", "Z29sZDo2OnRydWU6ZmFsc2U6dHJ1ZQ", "Z29sZDo2OnRydWU6ZmFsc2U6ZmFsc2U",
            "Z29sZDo2OmZhbHNlOnRydWU6dHJ1ZQ", "Z29sZDo2OmZhbHNlOnRydWU6ZmFsc2U", "Z29sZDo2OmZhbHNlOmZhbHNlOnRydWU",
            "Z29sZDo2OmZhbHNlOmZhbHNlOmZhbHNl"
        };

        Dictionary<string, List<SaveGame>> saveGames = new Dictionary<string, List<SaveGame>>();
        foreach (string gamename in gameServiceNames)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                h.BaseAddress + "api/gameservices/" + gamename + "/savegames?access_token=" + Client.AccessToken
            );

            var resp = await h.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Debug.Log("savegames " + body);

            if (resp.IsSuccessStatusCode)
            {
                saveGames.Add(gamename, JsonConvert.DeserializeObject<List<SaveGame>>(body));
            }
            else
            {
                Debug.Log("ERROR: could not get info for game: " + gamename);
            }
        }

        return saveGames;
    }


    public static async Task<bool> RegisterAsAdmin(
        string adminToken,
        RegisterEntry entry
    )
    {
        var req = new HttpRequestMessage(
            HttpMethod.Put,
            h.BaseAddress + "/api/users/" + entry.Name
                + "?access_token=" + adminToken
        )
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(entry),
                Encoding.UTF8,
                MediaTypeNames.Application.Json
            )
        };
        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();
        Debug.Log("Register " + body);

        return resp.IsSuccessStatusCode;
    }

    public static async Task<ulong?> CreateGame(string gameServiceName, string saveGameID = "")
    {
        var req = new HttpRequestMessage(
            new HttpMethod("POST"),
            h.BaseAddress + "api/sessions?access_token=" + Client.AccessToken
        )
        {

            // if we want to change savegame variable it is in this line
            Content = new StringContent(
                JsonConvert.SerializeObject(new Dictionary<string, string>() {
                    {"game", gameServiceName },
                    {"creator", Client.Username },
                    {"savegame", saveGameID}
                }),
                Encoding.UTF8,
                MediaTypeNames.Application.Json
            )
        };

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("CreateGame " + body);

        if (resp.IsSuccessStatusCode)
        {
            try
            {
                return ulong.Parse(body);
            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public static async Task<bool> JoinGame(ulong sessionID)
    {
        var req = new HttpRequestMessage(
            new HttpMethod("PUT"),
            h.BaseAddress +
                "api/sessions/" +
                sessionID.ToString() +
                "/players/" +
                Client.Username +
                "?access_token=" +
                Client.AccessToken
        );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("JoinGame " + body);

        return resp.IsSuccessStatusCode;
    }

    public static async Task<bool> LeaveGame()
    {
        var req = new HttpRequestMessage(
            new HttpMethod("DELETE"),
            h.BaseAddress +
                "api/sessions/" +
                Client.ConnectedSessionID.ToString() +
                "/players/" +
                Client.Username +
                "?access_token=" +
                Client.AccessToken
        );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("LeaveGame " + body);

        return resp.IsSuccessStatusCode;
    }

    public static async Task<bool> StartGame()
    {
        var req = new HttpRequestMessage(
            new HttpMethod("POST"),
            h.BaseAddress +
                "api/sessions/" +
                Client.ConnectedSessionID.ToString() +
                "?access_token=" +
                Client.AccessToken
        );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("StartGame " + body);

        return resp.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteSession(ulong sessionID)
    {
        var req = new HttpRequestMessage(
            new HttpMethod("DELETE"),
            h.BaseAddress +
                "api/sessions/" +
                sessionID.ToString() +
                "?access_token=" +
                Client.AccessToken
        );

        var resp = await h.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Debug.Log("DeleteSession " + body);

        return resp.IsSuccessStatusCode;
    }

    public static string AddPadding(string s)
    {
        for (int i = 0; i < s.Length % 4; i++)
        {
            s = s + '=';
        }

        return s;
    }
}

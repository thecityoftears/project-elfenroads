using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using Elfencore.Shared.GameState;
using TMPro;

public class SessionListGetter : MonoBehaviour
{
    public struct ElfenroadsGameParems
    {
        public int numRounds;
        public Variant variant;
        public bool randomDest;
        public bool randomGold;
        public bool witch;
    }

    public GameObject sessionTemplate;

    public GameObject saveGameTemplate;

    public GameObject waitRoomScreen;

    public List<GameObject> createdSessions = new List<GameObject>();

    public float timeBetweenRefreshes = 0.5f;

    public TextMeshProUGUI saveOrSessionDisplay;
    public TextMeshProUGUI UsernameDisplay;

    private float timeSinceLastRefresh;

    private bool searchingSaveGames = false; // whether the session list should be displaying sessions or save games

    public void Start() {
        timeSinceLastRefresh = Time.time;
        UsernameDisplay.text = "";
        UsernameDisplay.text = "Logged in as: " + Client.Username;
    }

    public void Update() {
        if(Time.time - timeSinceLastRefresh >= timeBetweenRefreshes) {
            timeSinceLastRefresh = Time.time;
            RefreshSessionList();
        }
    }

    public async void RefreshSessionList()
    {
        // clear old sessions
        foreach (GameObject oldSession in createdSessions)
        {
            Destroy(oldSession);
        }
        createdSessions.Clear();

        if(searchingSaveGames) {
            Dictionary<string, List<SaveGame>> saveGames = await LobbyService.GetSaveGames();

            saveOrSessionDisplay.text = "SAVES";

            saveGameTemplate.SetActive(true);

            foreach(KeyValuePair<string, List<SaveGame>> service in saveGames) {
                ElfenroadsGameParems parems = GetVariantDetails(service.Key);

                foreach(SaveGame saveGame in service.Value) {
                    bool inGame = false;
                    foreach(string playerName in saveGame.Players) {
                        if(playerName == Client.Username) {
                            inGame = true;
                            break;
                        }
                    }
                    if(!inGame)
                        continue;

                    GameObject newSave = GameObject.Instantiate(saveGameTemplate, saveGameTemplate.transform.parent);

                   
                    newSave.transform.GetChild(0).GetComponent<Text>().text = "Variant: " + parems.variant.ToString();
                    
                    GameObject templatePlayer = newSave.transform.GetChild(1).transform.GetChild(1).gameObject;
                    templatePlayer.SetActive(true);
                    foreach(string player in saveGame.Players) {
                        GameObject newPlayer = GameObject.Instantiate(templatePlayer, templatePlayer.transform.parent);
                        newPlayer.GetComponent<TextMeshProUGUI>().text = player;
                    }
                    templatePlayer.SetActive(false);
        
                    newSave.transform.GetChild(2).GetComponent<Text>().text = "Rounds: " + parems.numRounds;
                    newSave.transform.GetChild(3).GetComponent<Text>().text = "ID: " + saveGame.SaveGameId;
                    if (parems.randomDest)
                        newSave.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
                    if (parems.witch)
                        newSave.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
                    if (parems.randomGold)
                        newSave.transform.GetChild(4).GetChild(2).gameObject.SetActive(true);
                    
                    newSave.transform.GetChild(5).GetComponent<CreateGameFromSave>().gameServiceName = service.Key;
                    newSave.transform.GetChild(5).GetComponent<CreateGameFromSave>().saveGameID = saveGame.SaveGameId;

                    createdSessions.Add(newSave);
                }
            }
            saveGameTemplate.SetActive(false);
        }
        else {
            Dictionary<string, GameSession> sessionList = await LobbyService.GetSessions();

            sessionTemplate.SetActive(true);

            saveOrSessionDisplay.text = "SESSIONS";

            foreach (KeyValuePair<string, GameSession> session in sessionList)
            {
                // make the session entries
                if (session.Value.Launched) // dont display started games
                    continue;

                ElfenroadsGameParems parems = GetVariantDetails(session.Value.GameParameters.Name);
                if(session.Value.Savegameid != null && session.Value.Savegameid != "") {
                    List<string> participants = new List<string>();
                    SaveGame save = await LobbyService.GetSaveGame(session.Value.GameParameters.DisplayName, session.Value.Savegameid);
                    if(save != null && !save.Players.Contains(Client.Username))
                        continue;
                } 

                GameObject newSession = GameObject.Instantiate(sessionTemplate, sessionTemplate.transform.parent);
                newSession.transform.GetChild(0).GetComponent<Text>().text = "GameID: " + session.Key;
                newSession.transform.GetChild(1).GetComponent<Text>().text = "Variant: " + parems.variant.ToString();
                newSession.transform.GetChild(2).GetComponent<Text>().text = "Players: " + session.Value.Players.Count + "/" + session.Value.GameParameters.MaxSessionPlayers;
                newSession.transform.GetChild(3).GetComponent<Text>().text = "Rounds: " + parems.numRounds;
                newSession.transform.GetChild(4).GetComponent<Text>().text = "Host: " + session.Value.Creator;
                if (parems.randomDest)
                    newSession.transform.GetChild(5).GetChild(0).gameObject.SetActive(true);
                if (parems.witch)
                    newSession.transform.GetChild(5).GetChild(1).gameObject.SetActive(true);
                if (parems.randomGold)
                    newSession.transform.GetChild(5).GetChild(2).gameObject.SetActive(true);
                newSession.transform.GetChild(6).GetComponent<JoinGameButton>().joinedGameId = ulong.Parse(session.Key);
                newSession.transform.GetChild(7).GetComponent<DeleteSessionButton>().sessionID = ulong.Parse(session.Key);
                newSession.transform.GetChild(7).GetComponent<DeleteSessionButton>().sessionListGetter = this;
                if (session.Value.Creator == Client.Username)
                    newSession.transform.GetChild(7).gameObject.SetActive(true);
                else
                    newSession.transform.GetChild(7).gameObject.SetActive(false);


                createdSessions.Add(newSession);
            }

            sessionTemplate.SetActive(false);
        }
    }

    /// <summary> Convert the Base64-encoded gameservuce name into corresponding game parameters </summary>
    public static ElfenroadsGameParems GetVariantDetails(string serviceName)
    {
        serviceName = LobbyService.AddPadding(serviceName);
        byte[] data = Convert.FromBase64String(serviceName);
        string decodedString = Encoding.UTF8.GetString(data);

        Client.GameServiceName = serviceName;

        ElfenroadsGameParems parems = new ElfenroadsGameParems();

        string[] splitString = decodedString.Split(':');
        if (splitString[0] == "land")
        {
            parems.variant = Variant.ELFENLAND;
            parems.witch = false;
            parems.randomGold = false;
        }
        else if (splitString[0] == "gold")
        {
            parems.variant = Variant.ELFENGOLD;
            parems.witch = Boolean.Parse(splitString[3]);
            parems.randomGold = Boolean.Parse(splitString[4]);
        }
        else
        {
            Debug.Log("Unexpected game type");
        }

        parems.numRounds = Int32.Parse(splitString[1]);
        parems.randomDest = Boolean.Parse(splitString[2]);

        return parems;
    }

    public async void CreateGame(string gameServiceName)
    {

        ulong? sessionID = await LobbyService.CreateGame(gameServiceName);
        if (sessionID != null)
        {
            // load into game lobby 

            Debug.Log("Create Game Success");

            Client.GameServiceName = gameServiceName;
            Client.ConnectedSessionID = (ulong)sessionID;

            waitRoomScreen.SetActive(true);
            transform.gameObject.SetActive(false);

        }
        else
            Debug.Log("Create Game Failed");
    }

    public void ToggleSessionList() {
        searchingSaveGames = !searchingSaveGames;
        RefreshSessionList();
    }
}

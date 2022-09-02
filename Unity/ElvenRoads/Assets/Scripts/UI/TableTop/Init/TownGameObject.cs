using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;
using TMPro;

public class TownGameObject : MonoBehaviour, MouseDown
{
    /// <summary> Associated town with this gameobject </summary>
    public string townName;

    private float radius;

    public InGameUIController UIManager;

    public Dictionary<string, GameObject> visitationCubes = new Dictionary<string, GameObject>();

    public float cubeScale = 0.15f;

    /// <summary> Where relative to the town gameobject to start placing visitation cubes </summary>
    public Vector3 cubesOffset = new Vector3(0.5f, 0.15f / 2, 0.5f);

    /// <summary> Spacing between each visitation Cube </summary>
    public Vector3 cubeSpacing = new Vector3(0.2f, 0.0f, 0.0f);

    public Vector3 goldOffset = new Vector3(0.5f, 0.0f, -0.5f);

    private GameObject goldDisplay = null;

    void Start()
    {
        radius = gameObject.transform.localScale.x / 2;
    }

    public void OnMouseDown()
    {
        Debug.Log(townName + " has been clicked");
        AudioManager.PlaySound("Door");

        if (Game.currentPlayer.GetName() == Client.Username && Game.phase == GamePhase.MoveBoot)
        {
            if (!UIManager.usingWitch)
            {
                // we need to make a display to choose what cards to play (might be as simple as caravan or no caravan)
                UIManager.DisplayCardSelection(Game.GetTownFromName(townName));
            }
            else
            {
                UseWitchForFlight witchRequest = new UseWitchForFlight();
                witchRequest.Town = Game.GetTownFromName(townName);
                MessageHandler.Message(witchRequest);
                UIManager.usingWitch = false;
            }
        }
    }

    public void UpdateTown()
    {
        UpdateVisitationCubes();
        UpdateCubeColor();
        UpdatePlayersOnTown();
        if (Game.variant == Variant.ELFENGOLD)
            UpdateGoldDisplays();
    }

    private void UpdateVisitationCubes()
    {
        DeleteVisitationCubes();

        foreach (Player p in Game.participants)
        {
            bool hasVisited = false;
            foreach (Town t in p.visited)
            {
                if (t.getName() == townName)
                {
                    hasVisited = true;
                    break;
                }
            }
            if (!hasVisited)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.name = p.GetName();
                cube.transform.parent = transform;
                cube.transform.localScale *= cubeScale;
                cube.transform.position = transform.position + cubesOffset + (Game.getPlayerIndex(p) * cubeSpacing);
                cube.GetComponent<MeshRenderer>().material = UIResources.GetPlayerMaterial();
                visitationCubes.Add(p.GetName(), cube);
            }
        }
    }

    private void UpdateCubeColor()
    {
        foreach (KeyValuePair<string, GameObject> pair in visitationCubes)
        {
            Player p = Game.GetPlayerFromName(pair.Key);
            if (p == null)
            {
                Debug.Log("ERROR: There was a visitation cube for a player that doesnt exist");
                continue;
            }

            if (p.GetColor() != null)
            {
                Elfencore.Shared.GameState.Color c = p.GetColor();
                UnityEngine.Color col = new UnityEngine.Color((float)c.r / 255.0f, (float)c.g / 255.0f, (float)c.b / 255.0f);
                pair.Value.GetComponent<MeshRenderer>().material.color = col;
            }
        }
    }

    private void UpdatePlayersOnTown()
    {
        foreach (Player p in Game.participants)
        {
            if (p.location.getName() == townName)
            {
                // get player gameobject
                GameObject boot = UIManager.bootUI.getPlayerGameObject(p);
                if (boot == null)
                    continue;

                int numOfPlayers = Game.participants.Count;
                if (numOfPlayers == 0)
                    Debug.Log("TownGameObject thinks there are 0 participants for some reason");

                // places the boots around the town in a circular pattern
                boot.transform.position = transform.position + new Vector3(Mathf.Cos(Game.getPlayerIndex(p) * (2 * Mathf.PI) / numOfPlayers), 0.7f, Mathf.Sin(Game.getPlayerIndex(p) * (2 * Mathf.PI) / numOfPlayers));
            }
        }
    }

    private async void UpdateGoldDisplays()
    {
        if (goldDisplay == null)
        {
            GameObject generated = new GameObject("GoldValue");
            generated.transform.parent = gameObject.transform;
            SpriteRenderer sr = generated.AddComponent<SpriteRenderer>();
            sr.sprite = UIResources.GetTownGoldDisplaySprite();
            generated.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            generated.transform.position = transform.position + new Vector3(0.0f, 0.1f, 0.0f) + goldOffset;
            generated.transform.localScale *= 0.05f;

            GameObject text = new GameObject("GoldText");
            text.transform.parent = generated.transform;
            text.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            text.transform.position = generated.transform.position + new Vector3(0.0f, 0.05f, 0.0f);
            TextMeshPro tmp = text.AddComponent<TextMeshPro>();
            tmp.text = Game.GetTownFromName(townName).goldValue.ToString();
            tmp.fontSize = 3;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontMaterial.color = UnityEngine.Color.black;

            goldDisplay = generated;
        }
        else
        {
            goldDisplay.transform.GetChild(0).GetComponent<TextMeshPro>().text = Game.GetTownFromName(townName).goldValue.ToString();
        }
    }

    private void DeleteVisitationCubes()
    {
        foreach (GameObject obj in visitationCubes.Values)
        {
            Destroy(obj);
        }
        visitationCubes.Clear();
    }
}
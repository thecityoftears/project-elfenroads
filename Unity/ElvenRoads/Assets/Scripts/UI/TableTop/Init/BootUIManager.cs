using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

/// <summary> Manages the creation of Boot GameObjects and any method calls to the collection of all players </summary>
public class BootUIManager : MonoBehaviour
{
    /// <summary> Gameobject that you want to store all the player gameobjects inside </summary>
    public GameObject playerContainer;
    public float bootScale = 0.1f;
    public Dictionary<string, GameObject> gamePlayers = new Dictionary<string, GameObject>();

    public InGameUIController UIManager;

    /// <summary> Generates boots for each player and keeps track of them. Gets the Players to add from Game.Participants so must be set prior to calling </summary>
    public void SetupUI(InGameUIController UIController)
    {
        UIManager = UIController;

        Mesh bootMesh = UIResources.GetPlayerMesh();

        foreach (Player p in Game.participants)
        {
            GameObject generated = new GameObject(p.GetName());
            generated.transform.parent = playerContainer.transform;
            MeshFilter mf = generated.AddComponent<MeshFilter>();
            mf.mesh = bootMesh;
            MeshRenderer mr = generated.AddComponent<MeshRenderer>();
            mr.material = UIResources.GetPlayerMaterial();

            gamePlayers.Add(p.GetName(), generated);

            generated.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }
    }

    /// <summary> Combines UpdateBootPositions and UpdateBootColors </summary>
    public void UpdateUI()
    {
        UpdateBootColors();
    }

    public GameObject getPlayerGameObject(Player p) {
        foreach(KeyValuePair<string, GameObject> pair in gamePlayers) {
            if(pair.Key == p.GetName())
                return pair.Value;
        }
        return null;
    }

    private void UpdateBootColors()
    {
        GameObject boot;
        Elfencore.Shared.GameState.Color c;
        UnityEngine.Color col;
        foreach (Player p in Game.participants)
        {
            boot = getPlayerGameObject(p);

            if(boot == null)
                throw new System.Exception("Could not find " + p.GetName() + "'s boot");
            else {
                c = p.GetColor();
                col = new UnityEngine.Color((float)c.r / 255.0f, (float)c.g / 255.0f, (float)c.b / 255.0f);
                Material mat = UIResources.GetPlayerMaterial();
                mat.color = col;
                boot.GetComponent<MeshRenderer>().material = mat;
                boot.GetComponent<MeshRenderer>().materials = new Material[]{mat};
            }
        }
    }
}

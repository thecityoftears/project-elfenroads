using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

/// <summary> Manages the creation of Town GameObjects and any method calls to the collection of all towns </summary>
public class TownUIManager : MonoBehaviour
{
    /// <summary> Gameobject that you want to store all the town gameobjects inside </summary>
    public GameObject townContainer;
    public Dictionary<string, TownGameObject> gameTowns = new Dictionary<string, TownGameObject>();

    private Dictionary<string, Vector3> townPositions = new Dictionary<string, Vector3>() {
        {"Elvenhold", new Vector3(-5.86f, 0.0f, 0.748f)}, {"Rivinia", new Vector3(-4.895f, 0.0f, -1.738f)}, {"Feodor", new Vector3(-0.842f, 0.0f, -0.258f)},
        {"Al'Baran", new Vector3(2.761f, 0.0f, -1.104f)}, {"Dag'Amura", new Vector3(2.789f, 0.0f, 1.751f)}, {"Kihromah", new Vector3(5.889f, 0.0f, 1.058f)},
        {"Lapphalya", new Vector3(-1.091f, 0.0f, 2.732f)}, {"Parundia", new Vector3(5.734f, 0.0f, -2.48f)}, {"Wylhien", new Vector3(5.566f, 0.0f, -6.126f)},
        {"Usselen", new Vector3(9.35f, 0.0f, -4.3f)}, {"Yttar", new Vector3(9.627f, 0.0f, -1.185f)}, {"Grangor", new Vector3(9.011f, 0.0f, 2.169f)},
        {"Mah'Davikia", new Vector3(8.561f, 0.0f, 5.107f)}, {"Ixara", new Vector3(3.359f, 0.0f, 5.269f)}, {"Virst", new Vector3(-2.744f, 0.0f, 5.469f)},
        {"Strykhaven", new Vector3(-6.84f, 0.0f, 4.566f)}, {"Beata", new Vector3(-9.465f, 0.0f, 3.214f)}, {"Erg'Eren", new Vector3(-9.174f, 0.0f, -1.653f)},
        {"Tichih", new Vector3(-6.238f, 0.0f, -5.00f)}, {"Throtmanni", new Vector3(-2.179f, 0.0f, -3.533f)}, {"Jaccaranda", new Vector3(1.82f, 0.0f, -5.373f)}
    };

    /// <summary> Generates town objects for each town and keeps track of them. Gets the Towns to add from Game.towns so must be set prior to calling. </summary>
    public void SetupUI(InGameUIController UIController)
    {
        Mesh townMesh = UIResources.GetTownMesh();
        Material townMat = UIResources.GetTownMaterial();

        foreach (KeyValuePair<string, Town> t in Game.towns)
        {
            Vector3 townPos;
            townPositions.TryGetValue(t.Key, out townPos);
            if (townPos == null)
            {
                Debug.Log("ERROR: TownUIManager does not have position for: " + t.Key);
                continue;
            }

            // create towm gameObject
            GameObject generated = new GameObject(t.Key);
            generated.transform.parent = townContainer.transform;
            MeshFilter mf = generated.AddComponent<MeshFilter>();
            mf.mesh = townMesh;
            MeshRenderer mr = generated.AddComponent<MeshRenderer>();
            mr.material = townMat;
            MeshCollider mc = generated.AddComponent<MeshCollider>(); // for OnMouseDown

            TownGameObject townScript = generated.AddComponent<TownGameObject>();
            townScript.townName = t.Value.getName();
            townScript.UIManager = UIController;


            generated.transform.localPosition = townPos;
            gameTowns.Add(t.Value.getName(), townScript);
        }
    }

    public void UpdateUI()
    {
        UpdateAccessibleTowns();
        UpdateTownGameObjects();
    }

    /// <summary> Used to update the available towns to the local player </summary>
    private void UpdateAccessibleTowns()
    {
        foreach (Town t in Game.towns.Values)
        {
            GetGameObject(t).GetComponent<MeshRenderer>().enabled = false;
            GetGameObject(t).GetComponent<MeshCollider>().enabled = false;
        }
        if (Game.phase == GamePhase.MoveBoot)
        {
            Player localPlayer = Client.GetLocalPlayer();
            foreach (Town t in Game.GetNeighboringTowns(localPlayer.GetLocation()))
            {
                GetGameObject(t).GetComponent<MeshRenderer>().enabled = true;
                GetGameObject(t).GetComponent<MeshCollider>().enabled = true;
            }
        }
    }

    public void UpdateAccessibleTownsForWitch(bool usingWitch)
    {
        foreach (Town t in Game.towns.Values)
        {
            GetGameObject(t).GetComponent<MeshRenderer>().enabled = usingWitch;
            GetGameObject(t).GetComponent<MeshCollider>().enabled = usingWitch;
        }
        if (!usingWitch)
            UpdateAccessibleTowns();
    }

    private void UpdateTownGameObjects()
    {
        foreach (TownGameObject town in gameTowns.Values)
        {
            town.UpdateTown();
        }
    }

    public GameObject GetGameObject(Town t)
    {
        TownGameObject town;
        gameTowns.TryGetValue(t.getName(), out town);
        if (town == null)
        {
            Debug.Log("Could not find: " + t.getName());
        }
        return town.gameObject;
    }
}

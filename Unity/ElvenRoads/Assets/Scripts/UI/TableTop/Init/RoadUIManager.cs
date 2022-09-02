using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

/// <summary> Manages the creation of Road GameObjects and any method calls to the collection of all roads </summary>
public class RoadUIManager : MonoBehaviour
{
    private TownUIManager townManager;

    /// <summary> Gameobject that you want to store all the road gameobjects inside </summary>
    public GameObject roadContainer;

    public InGameUIController UIController;

    public float roadWidth = .25f;

    public Dictionary<Road, GameObject> gameRoads = new Dictionary<Road, GameObject>();

    /// <summary> used for the towns with an extra road (river) </summary>
    private Dictionary<(string, string, Region), Vector3> riverKinks = new Dictionary<(string, string, Region), Vector3>() {
        {("Ixara", "Virst", Region.RIVER), new Vector3(0.0f, 0.0f, 1.5f)}, {("Ixara", "Virst", Region.PLAINS), new Vector3(0.0f, 0.0f, 1.0f)},
        {("Ixara", "Mah'Davikia", Region.RIVER), new Vector3(0.0f, 0.0f, 1.0f)}, {("Virst", "Lapphalya", Region.PLAINS), new Vector3(0.5f, 0.0f, 1.0f)},
        {("Strykhaven", "Virst", Region.MOUNTAIN), new Vector3(0.0f, 0.0f, 1.5f)}, {("Beata", "Elvenhold", Region.RIVER), new Vector3(0.0f, 0.0f, -1.0f)},
        {("Mah'Davikia", "Grangor", Region.MOUNTAIN), new Vector3(1.0f, 0.0f, 0.0f)}, {("Yttar", "Grangor", Region.MOUNTAIN), new Vector3(0.5f, 0.0f, 0.0f)},
        {("Yttar", "Grangor", Region.LAKE), new Vector3(-0.5f, 0.0f, 0.0f)}, {("Yttar", "Usselen", Region.PLAINS), new Vector3(0.1f, 0.0f, -1.0f)},
        {("Strykhaven", "Beata", Region.PLAINS), new Vector3(-1.0f, 0.0f, 1.25f)}, {("Ixara", "Lapphalya", Region.FOREST), new Vector3(-0.25f, 0.0f, 0.3f)},
        {("Elvenhold", "Lapphalya", Region.PLAINS), new Vector3(0.0f, 0.0f, 0.75f)}, {("Usselen", "Wylhien", Region.PLAINS), new Vector3(0.0f, 0.0f, -1.0f)}, 
        {("Usselen", "Wylhien", Region.RIVER), new Vector3(0.0f, 0.0f, 0.5f)}
    };

    private void Start()
    {
        townManager = GameObject.FindGameObjectWithTag("TownUIManager").GetComponent<TownUIManager>();
    }

    /// <summary> Used to determine whether or not we see the roads </summary>
    public void UpdateUI()
    {
        foreach (GameObject road in gameRoads.Values)
        {
            road.SetActive(Game.phase == GamePhase.PlaceCounter);
            RoadGameObject roadScript = road.GetComponent<RoadGameObject>();
            if (roadScript == null)
                Debug.LogError("NO ROAD SCRIPT ATTACHED");
            else
                road.GetComponent<RoadGameObject>().DisplayCounters();

            road.GetComponent<RoadGameObject>().ActivateCounterBorders(UIController.isInExchangeSpell);
        }
    }

    /// <summary> Setups the GameObjects of the roads. Should be called after the TownUIManager has established the towns </summary>
    public void SetupUI(InGameUIController UIController)
    {
        this.UIController = UIController;
        foreach (Road r in Game.roads)
        {
            GameObject src = townManager.GetGameObject(r.source);
            GameObject dst = townManager.GetGameObject(r.dest);
            if (src == null || dst == null)
            {
                Debug.Log("ERROR: TownUIManager does not contain the gameObjects for road between " + r.source.getName() + " and " + r.dest.getName());
            }

            // check if we need a kink in the road
            Vector3 kinkPos;
            bool success = riverKinks.TryGetValue((r.source.getName(), r.dest.getName(), r.region), out kinkPos);
            if (!success)
                success = riverKinks.TryGetValue((r.dest.getName(), r.source.getName(), r.region), out kinkPos);

            // means we need to add a kink in the road
            if (success)
            {
                GameObject parentObject = new GameObject("Road: " + r.source.getName() + " to " + r.dest.getName());
                parentObject.transform.parent = roadContainer.transform;
                Vector3 midPoint = src.transform.position + ((dst.transform.position - src.transform.position) / 2);

                GameObject section1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject section2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                section1.transform.name = "Section 1 of " + parentObject.transform.name;
                section2.transform.name = "Section 2 of " + parentObject.transform.name;
                section1.transform.parent = parentObject.transform;
                section2.transform.parent = parentObject.transform;
                // add kink
                midPoint += kinkPos;
                parentObject.transform.position = midPoint;
                section1.transform.position = src.transform.position + ((midPoint - src.transform.position) / 2);
                section2.transform.position = midPoint + ((dst.transform.position - midPoint) / 2);
                section1.transform.localScale = new Vector3(roadWidth, GetRoadYScale(r.region), Vector3.Distance(src.transform.position, midPoint));
                section2.transform.localScale = new Vector3(roadWidth, GetRoadYScale(r.region), Vector3.Distance(midPoint, dst.transform.position));
                section1.transform.rotation = Quaternion.Euler(0.0f, Mathf.Rad2Deg * Mathf.Atan2(src.transform.position.x - midPoint.x, src.transform.position.z - midPoint.z), 0.0f);
                section2.transform.rotation = Quaternion.Euler(0.0f, Mathf.Rad2Deg * Mathf.Atan2(midPoint.x - dst.transform.position.x, midPoint.z - dst.transform.position.z), 0.0f);
                section1.GetComponent<MeshRenderer>().material = UIResources.GetRoadMaterial();
                section2.GetComponent<MeshRenderer>().material = UIResources.GetRoadMaterial();
                section1.GetComponent<MeshRenderer>().material.color = UIResources.GetRoadColor(r.region);
                section2.GetComponent<MeshRenderer>().material.color = UIResources.GetRoadColor(r.region);

                RoadGameObject roadScript1 = section1.AddComponent<RoadGameObject>();
                roadScript1.roadInfo = r;
                roadScript1.UIController = UIController;
                RoadGameObject roadScript2 = section2.AddComponent<RoadGameObject>();
                roadScript2.roadInfo = r;
                roadScript2.UIController = UIController;

                RoadGameObject parentRoadScript = parentObject.AddComponent<RoadGameObject>();
                parentRoadScript.roadInfo = r;
                parentRoadScript.UIController = UIController;

                gameRoads.Add(r, parentObject);
            }
            else
            {
                GameObject generated = GameObject.CreatePrimitive(PrimitiveType.Cube);
                generated.transform.name = "Road: " + r.source.getName() + " to " + r.dest.getName();
                generated.transform.parent = roadContainer.transform;
                generated.transform.position = src.transform.position + ((dst.transform.position - src.transform.position) / 2);
                generated.transform.rotation = Quaternion.Euler(0.0f, Mathf.Rad2Deg * Mathf.Atan2(src.transform.position.x - dst.transform.position.x, src.transform.position.z - dst.transform.position.z), 0.0f);
                generated.transform.localScale = new Vector3(roadWidth, GetRoadYScale(r.region), Vector3.Distance(src.transform.position, dst.transform.position));
                generated.GetComponent<MeshRenderer>().material = UIResources.GetRoadMaterial();
                generated.GetComponent<MeshRenderer>().material.color = UIResources.GetRoadColor(r.region);

                RoadGameObject roadScript = generated.AddComponent<RoadGameObject>();
                roadScript.roadInfo = r;
                roadScript.UIController = UIController;

                gameRoads.Add(r, generated);
            }
        }
    }

    /// <summary> Jank way to remove z-fighting </summary>
    private float GetRoadYScale(Region r)
    {
        return 0.075f + 0.001f * (int)r;
    }

    public void ActivateCounterBorders() {
        foreach (GameObject road in gameRoads.Values)
        {
            road.GetComponent<RoadGameObject>().ActivateCounterBorders(UIController.isInExchangeSpell);
        }
    }

    public RoadGameObject GetRoadGameObject(Road r)
    {
        foreach (KeyValuePair<Road, GameObject> pair in gameRoads)
        {
            if (pair.Key.source.getName() == r.source.getName() && pair.Key.dest.getName() == r.dest.getName() && pair.Key.region == r.region)
                return pair.Value.GetComponent<RoadGameObject>();
        }
        return null;
    }
}
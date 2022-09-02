using UnityEngine;
using Elfencore.Shared.GameState;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary> MonoBehavior script attached to all road gameObjects will be used to handle inputs to the roads </summary>
public class RoadGameObject : MonoBehaviour, MouseDown
{
    public Road roadInfo; // DO NOT USE THIS AS IF IT CURRENTLY HOLDS STATE

    public InGameUIController UIController;

    private List<GameObject> createdObjects = new List<GameObject>();

    public void OnMouseDown()
    {
        if (Screen.width - Mouse.current.position.ReadValue()[0] < 100)
        {
            Debug.Log("NO CLICKY!");
            return;
        }

        // this object was clicked - do something

        Debug.Log(gameObject.transform.name + " was clicked");

        AudioManager.PlaySound("Stone");

        if (Game.currentPlayer.GetName() == Client.Username && Game.phase == GamePhase.PlaceCounter)
        {
            // we need to make a display to choose the counter
            UIController.RoadSelected(roadInfo);
        }
    }

    /// <summary> Make sprite objects lay flat on the roads </summary>
    public void DisplayCounters()
    {
        foreach (GameObject obj in createdObjects)
        {
            Destroy(obj);
        }
        createdObjects.Clear();

        // update the info in the road
        Road updatedInfo = Game.GetRoad(roadInfo.source.getName(), roadInfo.dest.getName(), roadInfo.region);
        if (updatedInfo == null)
        {
            Debug.Log("Could not find road");
            return;
        }

        roadInfo = updatedInfo;

        int i = 0;
        foreach (Counter c in roadInfo.GetCounters())
        {
            GameObject generated = new GameObject(c.GetType().ToString());
            generated.transform.localScale *= 0.25f; // make a reasonable size
            generated.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f); // make sprite lay flat
            generated.transform.parent = transform.parent;
            SpriteRenderer sr = generated.AddComponent<SpriteRenderer>();
            sr.sprite = UIResources.GetSpriteFor(c);
            generated.AddComponent<BoxCollider>();
            CounterGameObject ctr = generated.AddComponent<CounterGameObject>();
            ctr.c = c;
            ctr.r = roadInfo;
            ctr.UIController = UIController;
            generated.transform.position = (i++ * new Vector3(-0.5f, 0.0f, 0.0f)) + new Vector3(0.0f, 0.21f, 0.0f) + transform.position; // temporary (CAN BE BAD)

            GameObject border = new GameObject("Border");
            border.transform.parent = generated.transform;
            border.transform.position = generated.transform.position;
            border.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            border.transform.localScale = Vector3.one * 2.0f;
            border.AddComponent<SpriteRenderFader>();
            SpriteRenderer srB = border.AddComponent<SpriteRenderer>();
            srB.sprite = UIResources.GetBorderSprite();
            srB.color = new UnityEngine.Color(1.0f, 0.9961556f, 0.4186544f, 1.0f);
            border.SetActive(false);

            createdObjects.Add(generated);
        }
    }

    public void ActivateCounterBorders(bool activated)
    {
        foreach (GameObject obj in createdObjects)
        {
            if(obj.GetComponent<CounterGameObject>().c.IsTrasportCounter()) {
                obj.transform.GetChild(0).gameObject.SetActive(activated);
                obj.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new UnityEngine.Color(1.0f, 0.9961556f, 0.4186544f, 1.0f);
            }
        }
    }
}
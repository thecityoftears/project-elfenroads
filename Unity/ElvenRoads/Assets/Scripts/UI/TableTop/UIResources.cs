using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

public static class UIResources
{
    public static Mesh GetPlayerMesh()
    {
        return Resources.Load<Mesh>("Meshes/Boot");
    }

    public static Material GetPlayerMaterial()
    {
        Shader shader = Resources.Load<Material>("Materials/Game Objects/Boot1").shader;
        Material newMat = new Material(shader);
        return newMat;
    }

    public static Mesh GetTownMesh()
    {
        return Resources.Load<Mesh>("Meshes/City");
    }

    public static Material GetTownMaterial()
    {
        return Resources.Load<Material>("Materials/Game Objects/Valid Movement");
    }

    public static Sprite GetCaravanSprite()
    {
        //TODO Get a better Caravan sprite
        return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.BackBlu");
    }

    public static Material GetRoadMaterial()
    {
        // TODO get a better road material
        return Resources.Load<Material>("Materials/Game Objects/Road");
    }

    public static Sprite GetSpriteForFaceDownCounter()
    {
        return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Back");
    }

    public static Material GetMaterialWithColor(Elfencore.Shared.GameState.Color c)
    {
        UnityEngine.Color col = new UnityEngine.Color((float)c.r / 255.0f, (float)c.g / 255.0f, (float)c.b / 255.0f);
        Material mat = new Material(GetPlayerMaterial());
        mat.color = col;
        return mat;
    }

    public static UnityEngine.Color GetRoadColor(Region r)
    {
        UnityEngine.Color c;
        switch (r)
        {
            case (Region.DESERT):
                c = UnityEngine.Color.yellow;
                c.a = 0.75f;
                return c;

            case (Region.FOREST):
                c = new UnityEngine.Color(0.152f, 0.478f, 0.101f);
                c.a = 0.75f;
                return c;

            case (Region.LAKE):
                c = UnityEngine.Color.blue;
                c.a = 0.75f;
                return c;

            case (Region.MOUNTAIN):
                c = new UnityEngine.Color(0.658f, 0.658f, 0.658f);
                c.a = 0.75f;
                return c;

            case (Region.PLAINS):
                c = UnityEngine.Color.green;
                c.a = 0.75f;
                return c;

            case (Region.RIVER):
                c = UnityEngine.Color.cyan;
                c.a = 0.75f;
                return c;

            default:
                return UnityEngine.Color.clear;
        }
    }

    public static Sprite GetBorderSprite() {
        return Resources.Load<Sprite>("Textures/Menu Textures/BlurBorder");
    }

    public static Sprite GetSpriteFor(Counter counter)
    {
        switch(counter.type) {
            case(Counter.CounterType.MAGICCLOUD):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Cloud");
            case(Counter.CounterType.ELFCYCLE):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Cycle");
            case(Counter.CounterType.DRAGON):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Dragon");
            case(Counter.CounterType.GIANTPIG):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Pig");
            case(Counter.CounterType.RAFT):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Raft");
            case(Counter.CounterType.TROLLWAGON):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Troll");
            case(Counter.CounterType.UNICORN):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Unicorn");
            case(Counter.CounterType.SEAOBS):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.SeaMonster");
            case(Counter.CounterType.TREEOBS):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Obstacle");
            case(Counter.CounterType.DOUBLESPELL):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Double");
            case(Counter.CounterType.EXCHANGESPELL):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Bounce");
            case(Counter.CounterType.GOLD):
                return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Gold");
            default:
                Debug.Log("ERROR: Can't get sprite for counter: Counter type is not handled");
                return null;
        }
    }

    public static Sprite GetSpriteForDest(string townName) {
        Sprite sprite;
        switch(townName) {
            case ("Beata"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Beata");
                break;
            case ("Erg'Eren"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Erg_Eren");
                break;
            case ("Grangor"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Grangor");
                break;
            case ("Ixara"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Ixara");
                break;
            case ("Jaccaranda"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Jaccaranda");
                break;
            case ("Mah'Davikia"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Mah_Davikia");
                break;
            case ("Strykhaven"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Strykhaven");
                break;
            case ("Tichih"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Tichih");
                break;
            case ("Usselen"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Usselen");
                break;
            case ("Virst"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Virst");
                break;
            case ("Wylhien"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Wylhien");
                break;
            case ("Yttar"):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/C.Yttar");
                break;
            default:
                sprite = null;
                break;
        }
        return sprite;
    }

    private static Sprite GetSpriteForObstacle(Counter obs)
    {
        if (obs.type == Counter.CounterType.SEAOBS)
            return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.SeaMonster");
        else if (obs.type == Counter.CounterType.TREEOBS)
            return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCt.Obstacle");
        else
        {
            Debug.Log("Cant get Obstacle sprite: Obstacle type has not been handled");
            return null;
        }
    }

    /// <summary>
    /// Get sprite for a travel card
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static Sprite GetSpriteFor(Card card)
    {
        if (card.IsTravelCard())
        {
            return GetSpriteForTravelCard(card.GetTransportType());
        }
        else if (card.IsGoldCard())
            return GetGoldCardSprite();
        // Commented out until WitchCard is implemented in GameState
        
        else if(card.IsWitchCard()) {
            return GetWitchCardSprite();
        }
        
        else
        {
            Debug.Log("Travel Card Type not Handled in UI resources");
            return null;
        }
    }

    private static Sprite GetWitchCardSprite()
    { return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Witch"); }

    private static Sprite GetGoldCardSprite()
    { return Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Gold"); }

    private static Sprite GetSpriteForTravelCard(TransportType t)
    {
        Sprite sprite;
        switch (t)
        {
            case (TransportType.MAGICCLOUD):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Cloud");
                break;

            case (TransportType.ELFCYCLE):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Cycle");
                break;

            case (TransportType.DRAGON):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Dragon");
                break;

            case (TransportType.GIANTPIG):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Pig");
                break;

            case (TransportType.RAFT):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Raft");
                break;

            case (TransportType.TROLLWAGON):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Troll");
                break;

            case (TransportType.UNICORN):
                sprite = Resources.Load<Sprite>("Textures/elfenroads-sprites/TCd.Unicorn");
                break;

            default:
                sprite = null;
                break;
        }

        return sprite;
    }

    public static Sprite GetTownGoldDisplaySprite() {
        return Resources.Load<Sprite>("Textures/elfenroads-sprites/gold");
    }
}
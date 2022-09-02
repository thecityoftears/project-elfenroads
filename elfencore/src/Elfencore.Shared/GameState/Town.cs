using System.Collections;
using System.Collections.Generic;

namespace Elfencore.Shared.GameState
{

    /// <summary> A town in Elfenroads </summary>
    public class Town
    {
        public string townName;
        public int goldValue;

        public Town() { }

        // General constructor
        public Town(string pTownName, int townGold)
        {
            townName = pTownName;
            goldValue = townGold;
        }

        // For Elfenland constructor
        public Town(string pName)
        {
            townName = pName;
            goldValue = 0;
        }

        public int getValue() { return goldValue; }
        public string getName() { return townName; }
    }
};
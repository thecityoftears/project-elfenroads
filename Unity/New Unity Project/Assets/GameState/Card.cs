using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Triumph.Game
{
    public class Card
    {
        public int id;
        public string cardName;
        public int cost;
        public int power;
        public string description;

        public Card()
        {

        }

        public Card(int Id, string name, int Cost, int Power, string desc)
        {
            id = Id;
            cardName = name;
            cost = Cost;
            power = Power;
            description = desc;
        }
    }
}

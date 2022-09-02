using System.Collections;
using System.Collections.Generic;

public class Ext
{
    public static List<T> Shuffle<T>(List<T> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            Random r = new Random();
            int randomIndex = r.Next(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }
}
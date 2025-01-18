using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public static class Utils
{
    public static T GetRandomEnumValue<T>() where T : System.Enum
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));
        int randomIndex = Random.Range(0, values.Length);
        return values[randomIndex];
    }
}
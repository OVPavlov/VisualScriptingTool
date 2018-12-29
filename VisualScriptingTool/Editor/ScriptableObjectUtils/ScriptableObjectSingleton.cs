using UnityEngine;

/// <summary> T is ScriptableObject itself </summary>
public class ScriptableObjectSingleton<T>: ScriptableObject where T: ScriptableObjectSingleton<T>
{
    [SetDefault]
    public bool IsDefaut;

    static T _instance;
    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                T[] singletons = Resources.LoadAll<T>("");
                if (singletons.Length == 0)
                {
                    Debug.LogError("There's no \'" + typeof (T) + "\' in Resources folder");
                }
                else
                {
                    for (int i = 0; i < singletons.Length; i++)
                    {
                        if (!singletons[i].IsDefaut) continue;
                        _instance = singletons[i];
                        break;
                    }
                }
            }
            return _instance;
        }
    }
}
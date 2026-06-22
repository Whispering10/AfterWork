using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class Factory
{
    public static GameObject Create(GameObject objectPrefab, CMSEntity modelPrefab)
    {
        if (!objectPrefab.GetComponent<Initializer>())
        {
            Debug.LogError($"{objectPrefab.name} hasn't component {typeof(Initializer)}");
            return null;
        }

        GameObject o = Object.Instantiate(objectPrefab);
        Initializer t = o.GetComponent<Initializer>();
        t.Init(modelPrefab);
        return o;
    }
}

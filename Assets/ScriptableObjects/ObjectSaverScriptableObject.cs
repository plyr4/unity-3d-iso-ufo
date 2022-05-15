using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[System.Serializable]
[CreateAssetMenu(menuName = "Object Saver", fileName = "New Object Saver")]
public class ObjectSaverScriptableObject : ScriptableObject
{
    public void RemoveAndApplyBeamProperties(GameObject obj)
    {
#if UNITY_EDITOR
        // remove the properties scripts
        ApplyBeamableProperties[] scripts = obj.GetComponents<ApplyBeamableProperties>();
        for (int i = scripts.Length - 1; i >= 0; i--) DestroyImmediate(scripts[i]);

        // save prefab
        SavePrefabInstance(obj);
#endif
    }

    public void SavePrefabInstance(GameObject obj)
    {
#if UNITY_EDITOR
        // get object path from selected object
        string assetPath = AssetDatabase.GetAssetPath(obj);

        // the asset path will be empty if the object is a prefab instance
        if (string.IsNullOrEmpty(assetPath))
        {
            // save the prefab using the instance
            PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.AutomatedAction);
        }
        else
        {
            // save the prefab by creating an instance
            GameObject objToSave = Instantiate(obj);
            PrefabUtility.SaveAsPrefabAsset(objToSave, assetPath);
            DestroyImmediate(objToSave);
        }
#endif
    }
}
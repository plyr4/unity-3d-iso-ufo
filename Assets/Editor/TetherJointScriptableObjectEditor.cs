using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(TetherJointScriptableObject))]
public class TetherJointScriptableObjectEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var monoImporter = AssetImporter.GetAtPath("Assets/ScriptableObjects/TetherJointScriptableObject.cs") as MonoImporter;
        var monoScript = monoImporter.GetScript();
        var jointIcon = (Texture2D)EditorGUIUtility.ObjectContent(null, typeof(ConfigurableJoint)).image;
        EditorGUIUtility.SetIconForObject(monoScript, jointIcon);
        EditorGUIUtility.SetIconForObject(this, jointIcon);
    }
}
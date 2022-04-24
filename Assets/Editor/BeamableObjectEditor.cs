using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BeamableObject))]
public class BeamableObjectEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BeamableObject _beamableObject = (BeamableObject)target;
        if (GUILayout.Button("Apply Random Spin"))
        {
            _beamableObject.ApplyRandomSpin();
        }
    }
}

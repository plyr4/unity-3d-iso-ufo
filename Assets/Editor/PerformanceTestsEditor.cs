using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerformanceTests))]
public class PerformanceTestsEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PerformanceTests _performanceTests = (PerformanceTests)target;
        if (GUILayout.Button(string.Format("{0} beam fire", _performanceTests.BeamIsLocked() ? "Unlock" : "Lock")))
        {
            _performanceTests.ToggleBeamLock();
        }
        if (GUILayout.Button(string.Format("Tether {0} objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherObjects(_performanceTests.NumberToTether);
        }
    }
}

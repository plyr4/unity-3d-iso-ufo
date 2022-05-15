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
        if (GUILayout.Button(string.Format("Tether {0} small objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherSmallObjects(_performanceTests.NumberToTether);
        }
        if (GUILayout.Button(string.Format("Tether {0} medium objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherMediumObjects(_performanceTests.NumberToTether);
        }
        if (GUILayout.Button(string.Format("Tether {0} large objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherLargeObjects(_performanceTests.NumberToTether);
        }
        if (GUILayout.Button(string.Format("Tether {0} random sized objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherRandomSizedObjects(_performanceTests.NumberToTether);
        }
        if (GUILayout.Button(string.Format("Tether {0} off center pivot objects", _performanceTests.NumberToTether)))
        {
            _performanceTests.TetherOffCenterPivotObjects(_performanceTests.NumberToTether);
        }
        if (GUILayout.Button(string.Format("Retract tethered objects")))
        {
            _performanceTests.RetractObjects();
        }
    }
}

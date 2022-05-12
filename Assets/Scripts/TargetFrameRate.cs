using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    void Start()
    {
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }
}

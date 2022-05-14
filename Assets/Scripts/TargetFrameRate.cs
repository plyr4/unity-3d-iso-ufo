using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    // TODO: refactor and apply framerates for different platforms (maybe)
    void Start()
    {
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }
}

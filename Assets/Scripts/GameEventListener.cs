using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEventBeamable : UnityEvent<GameObject, Beamable>{}

public class GameEventListener : MonoBehaviour
{
    [SerializeField] protected GameEvent _gameEvent;
    [SerializeField] protected UnityEvent _unityEvent;
    [SerializeField] protected UnityEventBeamable _unityEventBeamable;

    void Awake() => _gameEvent?.Register(this);
    void OnDisable() => _gameEvent?.Deregister(this);
    public virtual void RaiseEvent(GameObject obj = null, Beamable beamable = null)
    {
        _unityEvent?.Invoke();
        _unityEventBeamable?.Invoke(obj, beamable);
    }
}

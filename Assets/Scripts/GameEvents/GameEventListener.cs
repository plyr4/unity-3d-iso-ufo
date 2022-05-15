using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class UnityEventBeam : UnityEvent<TractorBeam>{}

[System.Serializable]
public class UnityEventBeamable : UnityEvent<TractorBeam, Beamable>{}
[System.Serializable]
public class UnityEventBeamables : UnityEvent<TractorBeam, List<Beamable>>{}

public class GameEventListener : MonoBehaviour
{
    [SerializeField] protected GameEvent _gameEvent;
    [SerializeField] protected UnityEvent _unityEvent;
    [SerializeField] protected UnityEventBeam _unityEventBeam;
    [SerializeField] protected UnityEventBeamable _unityEventBeamable;
    [SerializeField] protected UnityEventBeamables _unityEventBeamables;
    void Awake() => _gameEvent?.Register(this);
    void OnDisable() => _gameEvent?.Deregister(this);
    public virtual void RaiseEvent(TractorBeam beam = null, Beamable beamable = null, List<Beamable> beamables = null)
    {
        _unityEvent?.Invoke();
        _unityEventBeam?.Invoke(beam);
        _unityEventBeamable?.Invoke(beam, beamable);
        _unityEventBeamables?.Invoke(beam, beamables);
    }
}

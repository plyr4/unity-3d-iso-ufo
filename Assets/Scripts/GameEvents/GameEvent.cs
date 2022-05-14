using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Event", fileName = "New Game Event")]
public class GameEvent : ScriptableObject {
    HashSet<GameEventListener> _listeners = new HashSet<GameEventListener>();

    public void Invoke(TractorBeam beam = null, Beamable beamable = null, List<Beamable> beamables = null){
        foreach (GameEventListener listener in _listeners) listener?.RaiseEvent(beam, beamable, beamables);
    }

    public void Deregister(GameEventListener listener) => _listeners.Remove(listener);
    public void Register(GameEventListener listener) => _listeners.Add(listener);
}
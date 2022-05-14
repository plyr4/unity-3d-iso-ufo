using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIBeamDetails : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro _capacity;

    [SerializeField]
    private TextMeshPro _pickupPower;

    public void HandleTetherEvent(TractorBeam beam, Beamable beamable)
    {
        UpdateCapacityUI(beam, beamable);
    }


    public void HandleUntetherEvent(TractorBeam beam, Beamable beamable)
    {
        UpdateCapacityUI(beam, beamable);
    }


    public void HandleAbsorbEvent(TractorBeam beam, Beamable beamable)
    {
        _pickupPower.text = string.Format("{0}m", beam.GetPickupPowerRounded());
    }

    public void UpdateCapacityUI(TractorBeam beam, Beamable beamable)
    {
        _capacity.text = string.Format("{0}/{1}", beam._tetheredBeamables.Keys.Count, beam.MaxBeamableObjects);
        if (beam._tetheredBeamables.Keys.Count >= beam.MaxBeamableObjects)
        {
            _capacity.color = Color.red;
        }
        else
        {
            _capacity.color = Color.white;
        }
    }
}

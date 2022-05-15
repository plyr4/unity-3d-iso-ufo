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
    [SerializeField]
    private CanvasShake _fullShake;

    public void HandleBeamInitializeEvent(TractorBeam beam)
    {
        InitializeCapacityUI(beam);
        InitializePickupPowerUI(beam);
    }

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
        UpdatePickupPowerUI(beam);
    }


    private void InitializeCapacityUI(TractorBeam beam)
    {
        UpdateCapacityUI(beam, null);
    }


    private void InitializePickupPowerUI(TractorBeam beam)
    {
        UpdatePickupPowerUI(beam);
    }

    private void UpdatePickupPowerUI(TractorBeam beam)
    {
        _pickupPower.text = string.Format("{0}m", beam.GetPickupPowerRounded());
    }

    private void UpdateCapacityUI(TractorBeam beam, Beamable beamable)
    {
        _capacity.text = string.Format("{0}/{1}", beam._tetheredBeamables.Keys.Count, beam.MaxBeamableObjects);
        if (beam._tetheredBeamables.Keys.Count >= beam.MaxBeamableObjects)
        {
            _capacity.color = Color.red;
            _capacity.text += "<sprite=\"yellow-warning\" name=\"yellow-warning\">";
            _fullShake.Shake(3f);
        }
        else
        {
            _capacity.color = Color.white;
            _fullShake.Stop();
        }
    }
}

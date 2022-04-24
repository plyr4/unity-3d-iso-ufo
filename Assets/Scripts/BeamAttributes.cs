using UnityEngine;

public class BeamAttributes : MonoBehaviour
{
    [SerializeField]
    public Color BeamGrabOutlineColor;
    [SerializeField]
    public Color BeamGrabSpotLightColor;

    public static BeamAttributes DefaultBeamAttributes()
    {
        BeamAttributes beamAttributes = new BeamAttributes();

        // beam up grab outline
        beamAttributes.BeamGrabOutlineColor = Color.yellow;

        // beam up ground spotlight
        beamAttributes.BeamGrabSpotLightColor = Color.blue;
        return beamAttributes;
    }
}


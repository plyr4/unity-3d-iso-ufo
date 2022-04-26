using UnityEngine;

public class BeamAttributes : MonoBehaviour
{
    [SerializeField]
    public Color BeamGrabOutlineColor = Color.yellow;
    [SerializeField]
    public Color BeamGrabSpotLightColor = Color.blue;

    [SerializeField]
    public float BeamRetractionSpeed = 1f;
    
    [SerializeField]
    public float BeamGrabLinearLimit = 2f;
    
    [SerializeField]
    public float BeamGrabAnchorDepthCoefficient = 0.5f;

    [SerializeField]
    public ConfigurableJoint BeamableObjectJoint;

    
    [SerializeField]
    public bool BeamDrawJoints = false;

    public BeamAttributes InitializeDefaults()
    {
        // beam up grab outline
        BeamGrabOutlineColor = Color.yellow;

        // beam up ground spotlight
        BeamGrabSpotLightColor = Color.blue;

        // beam retraction speed
        BeamRetractionSpeed = 1f;
        
        // beam grab joint leeway
        BeamGrabLinearLimit = 2f;
    
        // beam grab depth ratio
        BeamGrabAnchorDepthCoefficient = 0.5f;

        // beam draw the joints
        BeamDrawJoints = false;
        return this;
    }
}

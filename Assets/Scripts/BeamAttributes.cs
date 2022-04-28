using UnityEngine;

public class BeamAttributes : MonoBehaviour
{
    [Header("Beam Attributes")]
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

    [SerializeField]
    public Color BeamGrabJointRenderColor = new Color(0f, 248f, 255f, 0.5f);
    
    [SerializeField]
    public float BeamGrabJointRenderWidth = 0.1f;

    [SerializeField]
    public Material BeamGrabJointRenderMaterial;

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

        // beam up joint renderer color
        BeamGrabJointRenderColor = new Color(0f, 248f, 255f, 0.5f);
        
        // beam up joint renderer width
        BeamGrabJointRenderWidth = 0.1f;
        return this;
    }
}

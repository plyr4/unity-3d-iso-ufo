using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SerializedJointDrive
{
    private JointDrive struc = new JointDrive();

    public static implicit operator JointDrive(SerializedJointDrive c)
    {
        return new JointDrive() { positionSpring = c._positionSpring, positionDamper = c._positionDamper, maximumForce = c._maximumForce };
    }
    public static explicit operator SerializedJointDrive(JointDrive c)
    {
        return new SerializedJointDrive(c);
    }

    public SerializedJointDrive() { }
    private SerializedJointDrive(JointDrive _data)
    {
        this.positionDamper = _data.positionDamper;
        this.positionSpring = _data.positionSpring;
    }

    [SerializeField]
    private float _positionSpring = 0;
    [SerializeField]
    private float _positionDamper = 0;
    [SerializeField]
    private float _maximumForce = float.PositiveInfinity;
    public float positionDamper { get { return struc.positionDamper; } set { _positionDamper = struc.positionDamper = value; } }
    public float positionSpring { get { return struc.positionSpring; } set { _positionSpring = struc.positionSpring = value; } }
    public float maximumForce { get { return struc.maximumForce; } set { _maximumForce = struc.maximumForce = value; } }
}

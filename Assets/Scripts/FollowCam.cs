using UnityEngine;
using System.Collections;

public class FollowCam : MonoBehaviour {
    [SerializeField]
	public Transform target;

	Vector3 _offset;

	void Start () {
		_offset = transform.position - target.position;
	}
	
	void LateUpdate () {
		Vector3 targetCamPos = target.position + _offset;
		transform.position =  targetCamPos ;
	}
}
using System;
using UnityEngine;

public class AutoRotateUnscaled : MonoBehaviour {

	[Serializable]
	enum rotationAxis
	{
		forward,
		right,
		up
	}

	[SerializeField] private rotationAxis axis = rotationAxis.forward;
    [SerializeField]
    float speed = 360;
	// Update is called once per frame
	void Update () {
		switch (axis)
		{
			case rotationAxis.forward:
				transform.Rotate(Vector3.forward, speed * Time.unscaledDeltaTime);
				break;
			case rotationAxis.right:
				transform.Rotate(Vector3.right, speed * Time.unscaledDeltaTime);
				break;
			case rotationAxis.up:
				transform.Rotate(Vector3.up, speed * Time.unscaledDeltaTime);
				break;
			default:
				transform.Rotate(Vector3.forward, speed * Time.unscaledDeltaTime);
				break;
		}
        
		
	}
}

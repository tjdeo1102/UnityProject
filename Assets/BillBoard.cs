using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera mainCam;
	void Awake()
	{
		mainCam = Camera.main;
	}
	void LateUpdate()
	{
		transform.LookAt(mainCam.transform.position);
	}
}

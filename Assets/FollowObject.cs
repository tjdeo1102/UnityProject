using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] Transform target;

	// Update is called once per frame
    float initY;
	void Start()
	{
		initY = transform.position.y;
	}
	void Update()
    {
        transform.position = new Vector3(target.position.x, initY + target.position.y, target.position.z);
        transform.rotation = Quaternion.Euler(transform.rotation.x, target.rotation.y, transform.rotation.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] Transform target;

    // Update is called once per frame
    void Update()
    {
        var pos = target.position;
        transform.position = new Vector3(pos.x, transform.position.y, pos.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [SerializeField] private GravityAttractor attractor;
    private Transform bodyTransform;


    private void Start()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GetComponent<Rigidbody>().useGravity = false;
        bodyTransform = transform;
    }


    private void Update()
    {
        attractor.Attract(bodyTransform);
    }

    public void ChangeGravityAttractor(GravityAttractor newAttracter)
    {
        attractor = newAttracter;
    }
}

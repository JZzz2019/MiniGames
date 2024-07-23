using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBody : MonoBehaviour
{
    [SerializeField] private GravityAttractor attractor;
    private Transform bodyTransform;


    void Start()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GetComponent<Rigidbody>().useGravity = false;
        bodyTransform = transform;
    }


    void Update()
    {
        attractor.Attract(bodyTransform);
    }
}

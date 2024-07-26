using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    private Vector3 moveDirection;
    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0).normalized;
    }

    private void FixedUpdate()
    {
        playerRigidbody.MovePosition(playerRigidbody.position + transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }
}

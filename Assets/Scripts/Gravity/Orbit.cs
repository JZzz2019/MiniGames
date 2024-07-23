using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{

    [SerializeField] private Transform satellite, pivotPoint;
    [SerializeField] private int count = 10;
    [SerializeField] private float distanceFromCenter = 2;
    [SerializeField] private int rotationSpeed = 5;

    void Start() => PlaceSatellitesAround();

    private void PlaceSatellitesAround()
    {
        for (int i = 0; i < count; i++)
        {
            var angle = i * (360f / count);
            var direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            var position = transform.position + direction * distanceFromCenter;
            Instantiate(satellite, position, Quaternion.identity, transform);
        }
    }

    void Update() => RotateAround();

    private void RotateAround()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        transform.localPosition = pivotPoint.localPosition;
    }
}

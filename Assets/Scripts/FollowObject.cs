using UnityEngine;

namespace SweetHomeStudios.Common.Utilities.Others
{
    [RequireComponent(typeof(Rigidbody))]
    public class FollowObject : MonoBehaviour
    {
        [SerializeField] private Transform target;
        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            rb.MovePosition(target.transform.position);
            rb.MoveRotation(target.transform.rotation);
        }
    }
}

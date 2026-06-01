using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
   [SerializeField] private Transform target;
   [SerializeField] private float damping;
    [SerializeField] private Vector3 offset;


   private Vector3 vel = Vector3.zero;

   private void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, damping);

    }

}

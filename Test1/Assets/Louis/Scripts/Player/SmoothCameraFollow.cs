using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
   [SerializeField] private Transform target;
   [SerializeField] private float damping;
    [SerializeField] private Vector3 offset;


   private Vector3 vel = Vector3.zero;

private void LateUpdate()
{
    Vector3 targetPosition = target.position + offset;
    Debug.Log("Target Y: " + target.position.y + " | Camera Y: " + transform.position.y);
    transform.position = Vector3.SmoothDamp(
        transform.position, targetPosition, ref vel, damping);
}

}

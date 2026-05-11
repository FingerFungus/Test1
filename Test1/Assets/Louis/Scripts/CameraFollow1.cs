using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothSpeed = 5f;
    public LayerMask collisionLayers;

    void LateUpdate()
    {
        Vector3 desiredPos = target.position + offset; // offset is now world-space, not player-relative
        Vector3 direction = desiredPos - target.position;

        if (Physics.SphereCast(target.position, 0.3f, direction.normalized, out RaycastHit hit, direction.magnitude, collisionLayers))
            desiredPos = target.position + direction.normalized * hit.distance;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(20f, 0f, 0f); // fixed rotation, tweak the X angle to taste
    }
}
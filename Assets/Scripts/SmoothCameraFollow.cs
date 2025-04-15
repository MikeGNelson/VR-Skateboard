using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; // The object the camera will follow
    public float followDistance = 10f; // Distance behind the target
    public float followHeight = 5f; // Height above the target
    public float followSpeed = 5f; // Speed at which the camera follows the target
    public float rotationSpeed = 5f; // Speed at which the camera rotates to face the target
    public bool smoothLookAt = true; // Whether the camera smoothly rotates to face the target


    private Vector3 lastPosition;

    void Start()
    {
        if (target != null)
        {
            lastPosition = target.position; // Initialize last position
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position behind the target
        Vector3 targetPosition = target.position - target.forward * followDistance + Vector3.up * followHeight;

        // Smoothly move the camera to the desired position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Smoothly rotate the camera to look at the target
        if (smoothLookAt)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.LookAt(target);
        }
    }
}

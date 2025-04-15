using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SmoothFollowUI : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public float distanceAhead = 5f;  // Distance ahead of the player
    public float smoothTime = 0.3f;  // Time for the UI to smooth follow

    public Vector3 targetPosition = Vector3.zero;

    private Vector3 velocity = Vector3.zero;

    public bool lookat = false;

    void Update()
    {
        // Calculate the target position N meters ahead of the player
        targetPosition = player.position + player.forward * distanceAhead;
        

        // Smoothly move the UI element to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        if (transform.position.y < 1)
        {
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        }
        if (targetPosition.y > 3)
        {
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
        }

        // Optionally, make the UI element always face the player
        if (lookat) {
            transform.LookAt(player);
        }
            
    }
}

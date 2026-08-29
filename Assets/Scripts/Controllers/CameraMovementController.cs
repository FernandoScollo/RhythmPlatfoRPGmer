using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    //Player movement cam Controller 
    [Header("Variables de seguimiento del jugador")]
    public bool followPlayerMovement;
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance; 
    [SerializeField] private float cameraSpeed;
    [SerializeField] private float lookAhead;

    //Room movement cam Controller
    [Header("Variables de cambio de lugar")]
    public bool followRoomChanges;
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;

    

    private void Update()
    {
        if(followPlayerMovement && followRoomChanges)
        {
            Debug.Log("Camera Can't follow Room and player at the same time");
            return;
        }
         
        if(followPlayerMovement)
        {
            transform.position = new Vector3(player.position.x + lookAhead, player.position.y, transform.position.z);
            lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);
        }

        if (followRoomChanges)
        {
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z),
            ref velocity, speed * Time.deltaTime);
        }
        

    }

    public void MoveToNewRoom(Transform _newRoom)
    {
        currentPosX = _newRoom.position.x;
    }
}

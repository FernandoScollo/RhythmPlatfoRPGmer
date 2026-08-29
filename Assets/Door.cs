using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform previusRoom;
    [SerializeField] private Transform nextRoom;
    [SerializeField] private CameraMovementController cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if (collision.transform.position.x < transform.position.x)
                cam.MoveToNewRoom(nextRoom);
            else
                cam.MoveToNewRoom(previusRoom);
        }
        
    }
}

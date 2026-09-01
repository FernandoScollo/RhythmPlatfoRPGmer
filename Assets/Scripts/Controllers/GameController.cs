using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform initialPosition;
    public GameObject player;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            player.transform.position = initialPosition.position;
        }
    }
}

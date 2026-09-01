using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAddController : MonoBehaviour
{
    [SerializeField] private bool APlatform;
    [SerializeField] private bool BPlatform;
    [SerializeField] private bool CPlatform;

    private void Start()
    {
        AddPlatformToRhythmController();
    }

    private void AddPlatformToRhythmController()
    {
        if (APlatform)
        {
            RhythmPlatformController.Instance.AddAPlatform(gameObject);
        }

        if (BPlatform)
        {
            RhythmPlatformController.Instance.AddBPlatform(gameObject);
        }

        if (CPlatform)
        {
            RhythmPlatformController.Instance.AddCPlatform(gameObject);
        }
    }
}

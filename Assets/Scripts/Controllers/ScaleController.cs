using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleController : MonoBehaviour
{
    public FrecuencyMusicManager bandFq;
    float initialScale;
    public float Scale;

    private void Start()
    {
        initialScale = transform.localScale.y;
    }

    private void Update()
    {
        ScaleY();
    }
    void ScaleY()
    {
        float bandsValue = 0;
        for (int i = 0; i< bandFq.frequencyBandsValue.Count; i++)
        {
            bandsValue += bandFq.frequencyBandsValue[i];
        }
        transform.localScale = new Vector3(transform.localScale.x, (initialScale +(bandsValue/Scale)), transform.localScale.z);
    }
}

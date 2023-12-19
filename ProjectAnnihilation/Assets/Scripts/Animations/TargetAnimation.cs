using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAnimation : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed;
    [SerializeField]
    private float baseScale;
    [SerializeField]
    private float scaleDepth;
    [SerializeField]
    private float scaleSpeed;


    private void Update()
    {
        Turn();
        Scale();
    }


    private void Turn()
    {
        transform.rotation = Quaternion.Euler(90, Time.time * spinSpeed, 0);
    }

    private void Scale()
    {
        transform.localScale = (baseScale + Mathf.Cos(Time.time * scaleSpeed) * scaleDepth) * Vector3.one;
    }

}

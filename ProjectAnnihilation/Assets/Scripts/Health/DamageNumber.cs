using System.Collections;
using TMPro;
using UnityEngine;


public class DamageNumber : MonoBehaviour
{
    [SerializeField]
    private float duration;
    [SerializeField, Range(0f,1f)]
    private float timeThreshold;
    [SerializeField]
    private TMP_Text textNumber;


    private void Start()
    {
        
    }

    private IEnumerator Animate()
    {
        float timer = 0;

        while (timer < duration)
        {


            timer += Time.deltaTime;
            yield return null;
        }
    }
}

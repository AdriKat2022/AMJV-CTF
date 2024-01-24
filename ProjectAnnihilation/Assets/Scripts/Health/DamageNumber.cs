using System.Collections;
using TMPro;
using UnityEngine;


public class DamageNumber : MonoBehaviour
{
    [SerializeField]
    private float duration;
    [SerializeField]
    private float speed;
    [SerializeField, Range(0f,1f)]
    private float timeThreshold;
    [SerializeField]
    private TMP_Text textNumber;


    public void Initialize(float number)
    {
        textNumber.text = number.ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float timer = 0;

        while (timer < duration)
        {
            transform.position += speed * Time.deltaTime * Vector3.up;

            if(timer/duration > timeThreshold)
            {
                //float value = Mathf.Clamp01(1 - ((timer - timer * timeThreshold) / (duration * timeThreshold)));
                float value = Time.deltaTime / (duration * (1-timeThreshold));
                textNumber.alpha -= value;
                textNumber.transform.localScale += Vector3.up * value;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Time")]
    [SerializeField]
    private bool bypassTimescale;
    [SerializeField]
    private bool activateOnAwake = true;
    [SerializeField]
    private bool isActive;

    [Header("Wave Rotation")]
    [SerializeField]
    private bool enableRotation;
    [SerializeField]
    private float rotationDepth, rotationSpeed;

    private float _time;


    [Header("Grow on hover")]
    [SerializeField]
    private bool enableHoverScale;
    [SerializeField]
    private float hoverDepth, hoverSpeed;

    private Vector3 scaleOnHover;

    [Header("Click animation")]
    [SerializeField]
    private bool enableClickAnimation;
    [SerializeField]
    private float clickDepth, clickSpeed;

    private Vector3 scaleOnClick;


    [Header("Sound")]
    [SerializeField]
    private bool playSoundOnHover;
    [SerializeField]
    private AudioClip soundOnHover;
    [SerializeField]
    private bool playSoundOnClick;
    [SerializeField]
    private AudioClip soundOnClick;
    [Space]
    [SerializeField]
    private bool onUpClickInstead;
    [SerializeField]
    private bool useFunctionInstead;



    private bool isMouseOver;
    private bool isMouseOverFirstFrame;

    private Vector3 baseScale;


#if UNITY_EDITOR

    private void OnValidate()
    {
        scaleOnHover = Vector3.one * (1 + hoverDepth);
        scaleOnClick = Vector3.one * (1 - clickDepth);
    }

#endif


    private void OnEnable()
    {
        baseScale = transform.localScale;
        _time = 0;

        if (bypassTimescale && !isActive)
        {
            isActive = true;
            StartCoroutine(BypassTimescale());
        }
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
        isMouseOver = false;
        isActive = false;
    }

    private void Start()
    {
        baseScale = transform.localScale;

        isActive = activateOnAwake;


        scaleOnHover = baseScale * (1 + hoverDepth);
        scaleOnClick = baseScale * (1 - clickDepth);
        _time = 0;

        isMouseOver = false;
        isMouseOverFirstFrame = false;
    }


    private IEnumerator BypassTimescale()
    {
        while (isActive)
        {
            if (enableRotation)
                HandleWaveMotion();

            isMouseOverFirstFrame &= isMouseOver;


            if (enableHoverScale)
                HandleGrowthOnHover(Time.unscaledDeltaTime);

            if (playSoundOnClick && isMouseOver && Input.GetMouseButtonUp(0) && !useFunctionInstead && (Input.GetMouseButtonUp(0) && onUpClickInstead || Input.GetMouseButtonDown(0) && !onUpClickInstead))
                PlayButtonSound();
            if (playSoundOnHover)
            {
                if (isMouseOver && !isMouseOverFirstFrame)
                {
                    SoundManager.Instance.PlaySound(soundOnHover);
                    isMouseOverFirstFrame = true;
                }
            }
            _time += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    public void PlayButtonSound() => SoundManager.Instance.PlaySound(soundOnClick);

    private void Update()
    {
        if (bypassTimescale && isActive)
            return;

        if (enableRotation)
            HandleWaveMotion();

        isMouseOverFirstFrame &= isMouseOver;

        if (enableHoverScale)
            HandleGrowthOnHover(Time.deltaTime);

        if (playSoundOnClick && isMouseOver && !useFunctionInstead && (Input.GetMouseButtonUp(0) && onUpClickInstead || Input.GetMouseButtonDown(0) && !onUpClickInstead))
            PlayButtonSound();

        if (playSoundOnHover)
        {
            if (isMouseOver && !isMouseOverFirstFrame)
            {
                SoundManager.Instance.PlaySound(soundOnHover);
                isMouseOverFirstFrame = true;
            }
        }
        _time += Time.deltaTime;
    }

    private void HandleGrowthOnHover(float dTime)
    {
        if (isMouseOver)
        {
            if (enableClickAnimation && Input.GetMouseButton(0))
                transform.localScale = Vector3.Lerp(transform.localScale, scaleOnClick, dTime * clickSpeed);
            else
                transform.localScale = Vector3.Lerp(transform.localScale, scaleOnHover, dTime * hoverSpeed);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, dTime * hoverSpeed);
        }
    }

    private void HandleWaveMotion()
    {
        float rotationAngle = Mathf.Sin(_time * rotationSpeed) * rotationDepth;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

    }


    //private bool IsMouseOver()
    //{
    //    // Check if the mouse is over the button
    //    //RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out Vector2 localPoint);

    //    //isMouseOver = rectTransform.rect.Contains(localPoint);

    //    return isMouseOver;
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
}

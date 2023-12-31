using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualTargetUnit : MonoBehaviour
{
    [SerializeField]
    private GameObject spriteTargetTo;

    public Color simpleMoveColor;
    public Color attackUnitColor;


    private SpriteRenderer spriteRenderer;

    /// The targetTo will be moved towards the unit's destination 
    /// It will be red if it's attached to another unit
    /// or green for simple position

    private void Start()
    {
        spriteTargetTo.SetActive(false);
        spriteTargetTo.transform.localPosition = 0.1f * Vector3.up;

        spriteRenderer = spriteTargetTo.GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    #region Manage functions

    public void ShowTarget(bool show = true)
    {
        spriteTargetTo.SetActive(show);
    }
    public void AttachTargetTo(Unit unit, bool activate = true)
    {
        spriteTargetTo.transform.SetParent(unit.transform, false);
        spriteTargetTo.transform.localPosition = 0.1f * Vector3.up;
        spriteTargetTo.SetActive(activate);
    }

    public void PlaceTargetAt(Vector3 targetPosition, bool activate = true)
    {
        spriteTargetTo.SetActive(activate);

        targetPosition.y = spriteTargetTo.transform.position.y;
        spriteTargetTo.transform.position = targetPosition;
    }

    public void DetachTarget(bool activate = false)
    {
        spriteTargetTo.SetActive(activate);
        spriteTargetTo.transform.SetParent(null, false);
    }
    #endregion

}

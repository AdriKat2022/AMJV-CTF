using UnityEngine;


public class VisualTargetUnit : MonoBehaviour
{
    [SerializeField]
    private GameObject spriteTargetTo;

    public Color simpleMoveColor;
    public Color attackUnitColor;

    private Transform attachedGo = null;
    private SpriteRenderer spriteRenderer;

    private float baseYPos;
    
    private const float HEIGHT = .1f;


    private void OnDisable()
    {
        Destroy(spriteTargetTo);
    }

    /// The targetTo will be moved towards the unit's destination 
    /// It will be red if it's attached to another unit
    /// or green for simple position

    private void Start()
    {
        baseYPos = spriteTargetTo.transform.position.y;

        spriteTargetTo.transform.SetParent(null);

        ShowTarget(false);
        spriteRenderer = spriteTargetTo.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!spriteTargetTo.activeInHierarchy || attachedGo == null)
            return;

        PlaceTargetAt(attachedGo);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    #region Manage functions

    public void ShowTarget(bool show)
    {
        spriteTargetTo.SetActive(show);
    }
    public void LockTarget(Unit unit)
    {
        attachedGo = unit.transform;
    }
    public void UnlockTarget()
    {
        attachedGo = null;
    }
    public void PlaceTargetAt(Transform target)
    {
        Vector3 position = target.position;

        //position.y = HEIGHT;
        position.y = baseYPos + HEIGHT;

        spriteTargetTo.transform.position = position;
    }
    public void PlaceTargetAt(Vector3 position)
    {
        //position.y = HEIGHT;
        position.y = baseYPos + HEIGHT;

        spriteTargetTo.transform.position = position;
    }

    #endregion

}

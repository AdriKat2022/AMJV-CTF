using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Takes care of the display of the HPBar
/// NEEDS A HEALTH MODULE TO WORK
/// </summary>
public class HealthBarModule : MonoBehaviour
{
    //private readonly Color BG_Default_Color = Color.red;
    //private readonly Color HP_Default_Color = Color.green;

    [SerializeField]
    private Image hpImage;
    [SerializeField]
    private Image hpBgImage;

    private UnitData unitData;
    private float currentHP;

    private Animator animator;
    private RectTransform rectTransform;

    private const float maxWidth = 3f;
    private const float minWidth = 1.2f;
    private const float maxWidthHP = 40f;


    private Camera cameraMain;

    private void Awake()
    {
        TryGetComponent(out rectTransform);
        cameraMain = Camera.main;
    }

    private void Update()
    {
        //transform.LookAt(cameraMain.transform);
        transform.rotation = cameraMain.transform.rotation;
    }

    public void Initialize(UnitData unitData, Color bg, Color hp, bool adaptativeBar = false)
    {
        this.unitData = unitData;
        currentHP = unitData.maxHP;

        hpImage.color = hp;
        hpBgImage.color = bg;

        if(adaptativeBar && rectTransform != null)
        {
            float multiplier = Mathf.Lerp(minWidth, maxWidth, unitData.maxHP/maxWidthHP);
            rectTransform.sizeDelta = new Vector2(multiplier, rectTransform.sizeDelta.y);
        }
    }
    public void SoftInitialize(Color bg, Color hp)
    {
        hpImage.color = hp;
        hpBgImage.color = bg;
    }

    public void SetCurrentHP(float currentHP)
    {
        this.currentHP = currentHP;
        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if(unitData.maxHP != 0)
            hpImage.fillAmount = currentHP/unitData.maxHP;
    }
}

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

    private RectTransform rectTransform;

    private const float maxWidth = 4f;
    private const float minWidth = 1.2f;
    private const float maxWidthHP = 50f;


    private void Awake()
    {
        TryGetComponent(out rectTransform);
    }


    public void Initialize(UnitData unitData, Color bg, Color hp, bool adaptativeBar = false)
    {
        this.unitData = unitData;
        currentHP = unitData.MaxHP;

        hpImage.color = hp;
        hpBgImage.color = bg;

        if(adaptativeBar && rectTransform != null)
        {
            float multiplier = Mathf.Lerp(minWidth, maxWidth, unitData.MaxHP/maxWidthHP);
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
        if(unitData.MaxHP != 0)
            hpImage.fillAmount = currentHP/unitData.MaxHP;
    }
}

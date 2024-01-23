using UnityEngine;
using UnityEngine.UI;

public class SpecialAttackUI : MonoBehaviour
{
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private Image specialAttackIcon;
    [SerializeField]
    private Animator animator;

    private void Start()
    {
        Hide();
    }

    public void Hide()
    {
        canvas.SetActive(false);
    }

    public void Show()
    {
        canvas.SetActive(true);
    }

    public void Press()
    {
        animator.SetTrigger("pressed");
    }

    public void UpdateAttackRecharge(float amount)
    {
        specialAttackIcon.fillAmount = Mathf.Clamp01(amount);
    }
}

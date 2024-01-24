using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUIManager : MonoBehaviour
{
    #region Variables
    [Header("Unit display & status")]
    [SerializeField]
    private GameObject statusObject;
    [SerializeField]
    private TMP_Text unitText;


    [Header("Special attack UI")]
    [SerializeField]
    private GameObject sa_canvas;
    [SerializeField]
    private Image sa_icon;
    [SerializeField]
    private Animator sa_animator;


    [Header("Particles effects")]
    [SerializeField]
    private ParticleSystem flamethrowerParticles;
    [SerializeField]
    private ParticleSystem selfDestructParticles;
    [SerializeField]
    private ParticleSystem powerUpParticles;
    [SerializeField]
    private ParticleSystem speedUpParticles;
    [SerializeField]
    private ParticleSystem defenseUpParticles;


    [Header("Display status effects")]
    [SerializeField]
    private Image hiddenIcon;


    [Header("Stun effects")]
    [SerializeField]
    private GameObject unitVisual;


    private Unit unit;
    private SelectModule selectModule;

    #endregion


    private void Start()
    {
        selectModule = SelectModule.Instance;
        TryGetComponent(out unit);

        ResetAllParticles();
        UpdateUIStatus();
        HideSpecialAttackIcon();
        UpdateStatusEffects();
        UpdateStateCircle();
    }

    private void Update()
    {
        UpdateStatusEffects();
        UpdateStateCircle();
        UpdateSpecialAttackUI();
    }

    #region Particles effects

    #region Public functions
    public void TooglePowerUpParticles(bool activate)
    {
        ParticleSystem.EmissionModule emission = powerUpParticles.emission;
        emission.enabled = activate;
    }
    public void ToogleSpeedUpParticles(bool activate)
    {
        ParticleSystem.EmissionModule emission = speedUpParticles.emission;
        emission.enabled = activate;
    }
    public void ToogleDefenseUpParticles(bool activate)
    {
        ParticleSystem.EmissionModule emission = defenseUpParticles.emission;
        emission.enabled = activate;
    }
    public void SetUpFlameThrowerParticles(float duration, float maxDistance, float angle)
    {
        float _SPEED = 15;

        if (flamethrowerParticles == null)
        {
            Debug.LogError("No flamethrower particles");
            return;
        }
        ParticleSystem.MainModule main = flamethrowerParticles.main;
        ParticleSystem.ShapeModule shape = flamethrowerParticles.shape;
        main.duration = duration;
        shape.angle = angle;
        main.startLifetime = maxDistance / _SPEED;
        main.startSpeed = _SPEED;

        flamethrowerParticles.Play();
    }
    public void SetUpSelfDestructParticles(float duration, float radius)
    {
        float _SPEED = 15;

        if (selfDestructParticles == null)
        {
            Debug.LogError("No selfdestruct particles");
            return;
        }
        ParticleSystem.MainModule main = selfDestructParticles.main;
        main.duration = duration;
        main.startLifetime = radius / _SPEED;
        main.startSpeed = _SPEED;

        selfDestructParticles.Play();
    }
    #endregion

    #region Private functions
    private void ResetAllParticles()
    {
        ToogleDefenseUpParticles(false);
        ToogleSpeedUpParticles(false);
        TooglePowerUpParticles(false);
    }
    #endregion

    #endregion

    #region Special Attack UI

    private void HideSpecialAttackIcon()
    {
        sa_canvas.SetActive(false);
    }

    private void ShowSpecialAttackIcon()
    {
        sa_canvas.SetActive(true);
    }

    private void SetAttackRecharge(float amount)
    {
        sa_icon.fillAmount = Mathf.Clamp01(amount);
    }

    private void UpdateSpecialAttackUI()
    {
        if (unit.IsSelected && selectModule.IsSelectionNotMultiple)
        {
            if (unit.UnitData.IsSpecialAttackPassive || unit.UnitData.SpecialAttackRechargeTime == 0)
                SetAttackRecharge(1);
            else
                SetAttackRecharge(1 - unit.SpecialActionCooldown / unit.UnitData.SpecialAttackRechargeTime);

            ShowSpecialAttackIcon();
        }
        else
            HideSpecialAttackIcon();
    }

    public void Press()
    {
        sa_animator.SetTrigger("pressed");
    }

    public void ShowInability()
    {
        sa_animator.SetTrigger("showInability");
    }

    public void Impatient()
    {
        sa_animator.SetTrigger("blocked");
    }

    #endregion

    #region Unit Display & status effects

    private void UpdateUIStatus()
    {
        unitText.color = unit.IsAttacker ? unit.UnitData.AttackerColor : unit.UnitData.DefenserColor;
        unitText.text = unit.UnitData.UnitName;
    }
    private void UpdateStateCircle()
    {
        if (statusObject == null || !statusObject.TryGetComponent(out Renderer rend))
            return;

        switch (unit.CurrentOrder)
        {
            case UnitState.NOTHING:
                rend.material.color = unit.UnitData.NothingColor;
                break;

            case UnitState.IDLE:
                rend.material.color = unit.UnitData.IdleColor;
                break;

            case UnitState.MOVING:
                rend.material.color = unit.UnitData.MovingColor;
                break;

            case UnitState.MOVING_ALERT:
                rend.material.color = unit.UnitData.MovingFocusedColor;
                break;

            case UnitState.MOVENATTACK:
                rend.material.color = unit.UnitData.ChaseColor;
                break;

            case UnitState.FOLLOWING:
                //rend.material.color = unitData.color;
                break;

            case UnitState.PATROLLING:
                //rend.material.color = unitData.nothingColor;
                break;
        }
    }
    private void UpdateStatusEffects()
    {
        hiddenIcon.gameObject.SetActive(unit.IsInvisible);
    }
    private IEnumerator BlinkIfSelected()
    {
        //statusObject.TryGetComponent(out Renderer rend);
        statusObject.transform.localScale = Vector3.one * 2;

        float blinkTimer = unit.UnitData.BlinkSpeed;
        bool state = false;

        yield return new WaitForSeconds(.1f);

        while (unit.IsSelected)
        {

            if (state && blinkTimer >= unit.UnitData.BlinkSpeed)
            {
                blinkTimer = 0;
                state = !state;
                //rend.material.color = unitData.SelectedColor;

                unitText.color = unit.IsAttacker ? unit.UnitData.AttackerColor : unit.UnitData.DefenserColor;

                if (unit.UnitData.UseFullBlink)
                    statusObject.SetActive(false);
            }
            else if (!state && blinkTimer >= unit.UnitData.BlinkSpeed)
            {
                blinkTimer = 0;
                state = !state;
                //rend.material.color = unitData.color;

                unitText.color = unit.UnitData.SelectedColor;

                if (unit.UnitData.UseFullBlink)
                    statusObject.SetActive(true);
            }

            blinkTimer += Time.deltaTime;

            yield return null;
        }
        statusObject.transform.localScale = Vector3.one;
        statusObject.SetActive(true);
        unitText.color = unit.IsAttacker ? unit.UnitData.AttackerColor : unit.UnitData.DefenserColor;
    }
    public void BlinkSelected()
    {
        StartCoroutine(BlinkIfSelected());
    }
    #endregion

    #region Unit Animations
    public void StartStunAnimation()
    {
        StartCoroutine(StunnedAnimation());
    }
    private IEnumerator StunnedAnimation()
    {
        Vector3 basePosition = unitVisual.transform.localPosition;

        float t = 0;
        bool state = false;

        while (unit.IsStunned)
        {
            if (state)
            {
                unitVisual.transform.localPosition = basePosition + Vector3.right * .1f;
                t -= Time.deltaTime;
                if (t < 0)
                    state = false;
            }
            else
            {
                unitVisual.transform.localPosition = basePosition - Vector3.right * .1f;
                t += Time.deltaTime;
                if (t > .01f)
                    state = true;
            }

            yield return null;
        }
    }

    #endregion

}

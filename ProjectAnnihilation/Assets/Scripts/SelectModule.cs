using System.Collections.Generic;
using UnityEngine;

public class SelectModule : MonoBehaviour
{
    public KeyCode KeepSelectionKey;
    [SerializeField]
    private KeyCode deselectionKey;

    private List<Unit> selectedUnits;
    private List<Unit> unitsList;

    private Camera mainCamera;
    private GameManager gameManager;

    #region Singleton instance

    public static SelectModule Instance => instance;
    private static SelectModule instance;

    public bool IsSelectionNotMultiple => selectedUnits.Count < 2;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        unitsList = new List<Unit>();
    }

    #endregion


    #region Unit list
    public List<Unit> GetAllUnits() => unitsList;

    public void Register(Unit unit)
    {
        unitsList.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        if(selectedUnits.Contains(unit))
            selectedUnits.Remove(unit);
        unitsList.Remove(unit);
    }

    #endregion

    private void Start()
    {
        gameManager = GameManager.Instance;

        mainCamera = Camera.main;
        selectedUnits = new List<Unit>();
    }

    private void Update()
    {
        if (!gameManager.GameStarted)
            return;

        CheckSelection();
        CheckDeselection();
    }

    private void CheckDeselection()
    {
        if (selectedUnits.Count > 0 && Input.GetKeyDown(deselectionKey))
            DeselectAllUnits();
    }

    private void CheckSelection()
    {
        if (!Input.GetMouseButtonDown(0) || (selectedUnits.Count > 0 && !Input.GetKey(KeepSelectionKey)))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (!Input.GetKey(KeepSelectionKey))
                DeselectAllUnits();

            if (hit.collider.gameObject.TryGetComponent(out UserInput other) && other.CanBeSelected)
                SelectUnit(other.Unit);
        }
    }

    #region Select functions

    private void SelectUnit(Unit unit)
    {
        if(unit == null) return;

        if (!selectedUnits.Contains(unit))
        {
            unit.Select();
            selectedUnits.Add(unit);
        }
    }
    public void DeselectUnit(Unit unit)
    {
        if(unit == null) return;

        if (selectedUnits.Contains(unit))
        {
            unit.Deselect();
            selectedUnits.Remove(unit);
        }
    }
    private void DeselectAllUnits()
    {
        foreach(Unit unit in selectedUnits) {
            unit.Deselect();
        }

        selectedUnits.Clear();
    }

    #endregion
}

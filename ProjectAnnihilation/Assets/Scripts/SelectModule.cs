using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectModule : MonoBehaviour
{
    [Header("Drag")]
    [SerializeField]
    private float minimumDragLength;
    [SerializeField]
    private Image selectionImage;

    public KeyCode KeepSelectionKey;
    [SerializeField]
    private KeyCode deselectionKey;

    private List<Unit> selectedUnits;
    private List<Unit> unitsList;

    private int nAllies;
    private int nEnemies;

    private Camera mainCamera;
    private GameManager gameManager;

    private Vector3 point1;
    private Vector3 point2;
    private bool isDragging;
    private RectTransform rectSelectImage;


    public int NAllies => nAllies;
    public int NEnemies => nEnemies;


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
        if(unit.IsAttacker)
            nAllies++;
        else
            nEnemies++;
    }

    public void Unregister(Unit unit)
    {
        if (unit.IsAttacker)
            nAllies--;
        else
            nEnemies--;

        if (selectedUnits.Contains(unit))
            selectedUnits.Remove(unit);
        unitsList.Remove(unit);
    }

    #endregion

    private void Start()
    {
        gameManager = GameManager.Instance;
        mainCamera = Camera.main;

        selectionImage.TryGetComponent(out rectSelectImage);
        Color c = selectionImage.color;
        c.a = 0;
        selectionImage.color = c;

        selectedUnits = new List<Unit>();
    }

    private void Update()
    {
        if (!gameManager.GameStarted)
            return;

        CheckSelection();
        CheckDeselection();
        CheckDrag();
    }

    #region Selection

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

    private void CheckDrag()
    {
        if (Input.GetMouseButtonDown(0))
            point1 = Input.mousePosition;

        else if (Input.GetMouseButton(0))
        {
            point2 = Input.mousePosition;

            bool hasReachedMinimumDrag = (point2 - point1).magnitude > minimumDragLength;

            if (hasReachedMinimumDrag && !isDragging){
                isDragging = true;
                StartCoroutine(AnimateSelectionArea());
            }
            else if (!hasReachedMinimumDrag)
            {
                isDragging = false;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (!Input.GetKey(KeepSelectionKey))
                DeselectAllUnits();

            foreach (Unit unit in unitsList)
            {
                if (!unit.IsAttacker)
                    continue;

                Vector2 unitScreenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (IsPointInSelection(unitScreenPos))
                    SelectUnit(unit);
            }
        }
    }

    private IEnumerator AnimateSelectionArea()
    {
        Color color = selectionImage.color;

        color.a = .3f;
        selectionImage.color = color;

        yield return new WaitUntil(() => (Input.GetMouseButtonUp(0) || !isDragging));

        if (!isDragging)
        {
            color.a = 0f;
            selectionImage.color = color;
            yield break;
        }

        isDragging = false;
        color.a = .5f;
        selectionImage.color = color;

        while(color.a > 0)
        {
            if (isDragging)
                yield break;

            color.a -= Time.deltaTime;
            selectionImage.color = color;

            yield return null;
        }
    }

    private (Vector2,Vector2) GetNormalizedPoints(Vector2 point1, Vector2 point2)
    {
        if (!(point1.x < point2.x))
        {
            (point1.x, point2.x) = (point2.x, point1.x);
        }
        if (!(point1.y > point2.y))
        {
            (point1.y, point2.y) = (point2.y, point1.y);
        }
        return (point1, point2);
    }

    private bool IsPointInSelection(Vector2 point)
    {
        (Vector2 p1, Vector2 p2) = GetNormalizedPoints(point1, point2);


        //Debug.Log(point1);
        //Debug.Log(point2);
        //Debug.Log(p1);
        //Debug.Log(p2);

        return 
            point.x > p1.x && point.x < p2.x &&
            point.y < p1.y && point.y > p2.y;
            ;
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            (Vector2 p1, Vector2 p2) = GetNormalizedPoints(point1, point2);

            p1.y = Screen.height - p1.y;
            p2.y = Screen.height - p2.y;

            float xBorder1 = p1.x; //
            float xBorder2 = - Screen.width + p2.x;
            float yBorder1 = - p1.y;
            float yBorder2 = (Screen.height - p2.y);

            rectSelectImage.offsetMax = new Vector2(xBorder2, yBorder1);
            rectSelectImage.offsetMin = new Vector2(xBorder1, yBorder2);
        }
    }

    #endregion

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

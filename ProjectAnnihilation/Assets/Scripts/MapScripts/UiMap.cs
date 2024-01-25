using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiMap : MonoBehaviour
{
    #region variables
    public static UiMap Instance { get; private set; }

    [SerializeField] private Button backToMenu;
    [SerializeField] private TMPro.TextMeshProUGUI timer;
    [SerializeField] private TMPro.TextMeshProUGUI enemies;
    [SerializeField] private TMPro.TextMeshProUGUI finalTimer;
    [SerializeField] private TMPro.TextMeshProUGUI finalEnemies;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject victory;
    [SerializeField] private GameObject defeat;
    private bool isGameOver = false;
    private bool flag = false;
    private bool isKingDead = false;
    private int enemyNumber;
    private int allyNumber;
    private float elapsedTime = 0f;
    private int minute;
    private int second;

    private GameManager gameManager;
    private SelectModule selectModule;

    #endregion
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameManager);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        selectModule = SelectModule.Instance;

        Time.timeScale = 1;

        background.SetActive(false);

        ScanUnits();

        gameManager.onFinalMoove.AddListener(SetFlag);
        gameManager.onDeathOfTheKing.AddListener(OnDeathOfTheKing);

        backToMenu.onClick.AddListener(OnClick);

        enemies.text = "";
        timer.text = "";
    }
    void Update()
    {
        if (!gameManager.GameStarted)
            return;

        if (!isGameOver)
        {
            UpdateTime();
            UpdateEnemiesText();
        }
        else
        {
            ManageGameOver();
            finalTimer.text = string.Format("{0}m {1}s", minute, second);
            finalEnemies.text = string.Format("{0} Enemies left", selectModule.NEnemies);
            enemies.gameObject.SetActive(false);
            finalEnemies.gameObject.SetActive(true);
            timer.gameObject.SetActive(false);
            finalTimer.gameObject.SetActive(true);
        }
    }


    private void ScanUnits()
    {
        allyNumber = 0;
        enemyNumber = 0;

        selectModule.GetAllUnits().ForEach((unit) =>
        {
            if (unit.IsAttacker)
                allyNumber++;
            else
                enemyNumber++;
        });
    }
    private void UpdateTime()
    {
        elapsedTime += Time.deltaTime;

        //Calcul of minutes and seconds

        minute = Mathf.FloorToInt(elapsedTime / 60f);
        second = Mathf.FloorToInt(elapsedTime % 60f);

        //update meshText in the specified format
        timer.text = string.Format("{0}m {1}s", minute, second);
    }
    private void UpdateEnemiesText()
    {
        if(selectModule.NEnemies < 2)
            enemies.text = "One left";
        else
            enemies.text = string.Format("{0} enemies left", selectModule.NEnemies);
    }

    private void OnDeathOfTheKing()
    {
        isKingDead = true;
        SetGameOver();
    }
    public void OnClick()
    {
        //Debug.Log("ButtonClicked");
        SceneManager.MoveGameObjectToScene(GameManager.Instance.gameObject, SceneManager.GetActiveScene());
        SceneManager.LoadSceneAsync(0);
    }
    public void SetGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0;
    }
    public void SetFlag()
    {
        flag = true;
        SetGameOver();
    }
    private void ManageGameOver()
    {
        if(enemyNumber == 0 || flag == true)
        {
            Victory();
        }
        else
        {
            Defeat();
        }
    }
    private void Victory()
    {
        background.SetActive(true);
        defeat.SetActive(false);
    }
    private void Defeat()
    {
        background.SetActive(true);
        victory.SetActive(false);
    }
}

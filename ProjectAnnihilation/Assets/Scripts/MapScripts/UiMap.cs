using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiMap : MonoBehaviour
{
    #region variables
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

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        Time.timeScale = 1;

        background.SetActive(false);

        GameObject[] entities = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");

        enemyNumber = entities.Length;
        allyNumber = allies.Length;

        gameManager.onEnemyDeath.AddListener(OnEnemyDeath);
        gameManager.onFinalMoove.AddListener(SetFlag);
        gameManager.onAllyDeath.AddListener(OnAllyDeath);
        gameManager.onDeathOfTheKing.AddListener(OnDeathOfTheKing);

        backToMenu.onClick.AddListener(OnClick);

        enemies.text = "";
        timer.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.GameStarted)
            return;

        if (isGameOver == false)
        {
            UpdateTime();
            UpdateEnemies();
        }
        else
        {
            ManageGameOver();
            finalTimer.text = string.Format("{0}m {1}s", minute, second);
            finalEnemies.text = string.Format("{0} Enemies left", enemyNumber);
            enemies.gameObject.SetActive(false);
            finalEnemies.gameObject.SetActive(true);
            timer.gameObject.SetActive(false);
            finalTimer.gameObject.SetActive(true);
        }
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

    private void UpdateEnemies()
    {
        enemies.text = string.Format("{0} Enemies left", enemyNumber);
    }

    private void OnEnemyDeath()
    {
        enemyNumber--;
        if(enemyNumber == 0)
        {
            UpdateEnemies();
            SetGameOver();
        }
    }
    private void OnAllyDeath()
    {
        allyNumber--;
        if(allyNumber == 0)
        {
            SetGameOver();
        }
    }

    private void OnDeathOfTheKing()
    {
        isKingDead = true;
        SetGameOver();
    }

    public void OnClick()
    {
        Debug.Log("ButtonClicked");
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

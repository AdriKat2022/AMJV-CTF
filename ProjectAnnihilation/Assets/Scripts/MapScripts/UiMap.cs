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

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        background.SetActive(false);
        GameObject[] entities = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        enemyNumber = entities.Length;
        allyNumber = allies.Length;
        GameManager.Instance.onEnemyDeath.AddListener(OnEnemyDeath);
        GameManager.Instance.onFinalMoove.AddListener(setFlag);
        GameManager.Instance.onAllyDeath.AddListener(OnAllyDeath);
        GameManager.Instance.onDeathOfTheKing.AddListener(OnDeathOfTheKing);
        backToMenu.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
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
            setGameOver();
        }
    }
    private void OnAllyDeath()
    {
        allyNumber--;
        if(allyNumber == 0)
        {
            setGameOver();
        }
    }

    private void OnDeathOfTheKing()
    {
        isKingDead = true;
        setGameOver();
    }

    public void OnClick()
    {
        Debug.Log("ButtonClicked");
        SceneManager.MoveGameObjectToScene(GameManager.Instance.gameObject, SceneManager.GetActiveScene());
        SceneManager.LoadSceneAsync(0);
    }

    public void setGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0;
    }
    public void setFlag()
    {
        flag = true;
        setGameOver();
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

using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool PlayerIsAttacker => playerIsAttacker;
    private bool playerIsAttacker;
    public UnityEvent onEnemyDeath;
    public UnityEvent onAllyDeath;
    public UnityEvent onFinalMoove;
    public UnityEvent onDeathOfTheKing;

    [Header("UI")]
    [SerializeField]
    private Animator spaceButtonAnimator;

    private SoundManager soundManager;


    static public GameManager Instance => instance;
    static private GameManager instance;


    public bool GameStarted => gameStarted;
    private bool gameStarted;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Cannot have multiple instances of GameManager !");
            Destroy(gameObject);
        }

        gameStarted = false;
    }

    private void Start()
    {
        soundManager = SoundManager.Instance;
        if (soundManager != null)
            soundManager.StopMusic();
    }

    private void Update()
    {
        if(!gameStarted)
            CheckGameStart();
    }

    private void CheckGameStart()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            spaceButtonAnimator.SetTrigger("press");

        if (Input.GetKeyUp(KeyCode.Space))
        {
            spaceButtonAnimator.SetTrigger("release");
            StartGame();
        }
    }
    private void StartGame()
    {
        gameStarted = true;
        soundManager.PlayMusic(Random.value < .5f ? soundManager.music1 : soundManager.music2);
    }

    public void TriggerEnemyDeath()
    {
        onEnemyDeath?.Invoke();
    }
    public void TriggerAllyDeath()
    {
        onAllyDeath?.Invoke();
    }

    public void TriggerFinalMoove()
    {
        onFinalMoove?.Invoke();
    }

    public void TriggerDeathOfTheKing()
    {
        onDeathOfTheKing?.Invoke();
    }
}

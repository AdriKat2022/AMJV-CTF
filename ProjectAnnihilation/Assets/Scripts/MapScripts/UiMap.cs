using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class UiMap : MonoBehaviour
{
    #region variables
    [SerializeField] private TMPro.TextMeshProUGUI timer;
    [SerializeField] private TMPro.TextMeshProUGUI enemys;
    [SerializeField] private GameObject victory;
    [SerializeField] private GameObject defeat;
    private bool isGameOver = false;
    private bool flag = false;
    private int enemyNumber;
    private int allyNumber;
    private float elapsedTime = 0f;
    private int minute;
    private int second;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] entities = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        enemyNumber = entities.Length;
        allyNumber = allies.Length;
        GameManager.Instance.onEnemyDeath.AddListener(OnEnemyDeath);
        GameManager.Instance.onFinalMoove.AddListener(setFlag);
        GameManager.Instance.onAllyDeath.AddListener(OnAllyDeath);
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver == false)
        {
            UpdateTime();
            UpdateEnemys();
        }
        else
        {
            ManageGameOver();
            timer.rectTransform.anchoredPosition = new Vector3(-398, -133, 0);
            enemys.rectTransform.anchoredPosition = new Vector3(398, -253, 0);
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

    private void UpdateEnemys()
    {
        enemys.text = string.Format("{0} Enemys left", enemyNumber);
    }

    private void OnEnemyDeath()
    {
        enemyNumber--;
        if(enemyNumber == 0)
        {
            UpdateEnemys();
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
    public void setGameOver()
    {
        isGameOver = true;
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
        victory.SetActive(true);
    }
    private void Defeat()
    {
        defeat.SetActive(true);
    }
}

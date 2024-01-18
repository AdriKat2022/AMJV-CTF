using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class UiMap : MonoBehaviour
{
    #region variables
    [SerializeField] private TMPro.TextMeshProUGUI timer;
    [SerializeField] private TMPro.TextMeshProUGUI enemys;
    private int enemyNumber;
    private float elapsedTime = 0f;
    private int minute;
    private int second;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] entities = GameObject.FindGameObjectsWithTag("Enemy");
        enemyNumber = entities.Length;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        UpdateEnemys();
    }
    void UpdateTime()
    {
        elapsedTime += Time.deltaTime;

        //Calcul of minutes and seconds

        minute = Mathf.FloorToInt(elapsedTime / 60f);
        second = Mathf.FloorToInt(elapsedTime % 60f);

        //Update MeshText in the specified format
        timer.text = string.Format("{0}m {1}s", minute, second);
    }

    void UpdateEnemys()
    {
        enemys.text = string.Format("{0} Enemys left", enemyNumber);
        GameManager.Instance.onEnemyDeath.AddListener(OnEnemyDeath);
    }

    void OnEnemyDeath()
    {
        enemyNumber--;
    }
}

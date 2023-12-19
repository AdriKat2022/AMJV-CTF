using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    //Useful variables

    [SerializeField] private Button mainStart;
    [SerializeField] private Button mainParameters;
    [SerializeField] private Button mainQuit;
    [SerializeField] private Button parametersStart;
    [SerializeField] private Button parametersReturn;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject parametersMenu;
    [SerializeField] private GameObject selectionMenu;

    // Start is called before the first frame update
    void Start()
    {
        parametersMenu.SetActive(false);
        selectionMenu.SetActive(false);
        mainMenu.SetActive(true);
        mainStart.onClick.AddListener(OnMainStartClick);
        mainParameters.onClick.AddListener(OnMainParametersClick);
        mainQuit.onClick.AddListener(OnMainQuitClick);
        parametersStart.onClick.AddListener(OnParametersStartClick);
        parametersReturn.onClick.AddListener(OnParametersReturnClick);

    }
    public void OnSelectionReturnClicked()
    {
        mainMenu.SetActive(true);
        selectionMenu.SetActive(false);
    }
    public void StartLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    private void OnMainStartClick()
    {
        selectionMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    private void OnMainParametersClick()
    {
        parametersMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    private void OnMainQuitClick()
    {
        Application.Quit();
    }
    private void OnParametersStartClick()
    {
        selectionMenu.SetActive(true);
        parametersMenu.SetActive(false);
    }
    private void OnParametersReturnClick()
    {
        mainMenu.SetActive(true);
        parametersMenu.SetActive(false);
    }
}

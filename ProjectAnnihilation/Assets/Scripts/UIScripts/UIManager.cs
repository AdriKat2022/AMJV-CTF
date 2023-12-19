
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Singleton instance

    public UIManager Instance => instance;
    private UIManager instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

}

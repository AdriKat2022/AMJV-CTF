
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Singleton instance

    public static UIManager Instance => instance;
    private static UIManager instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

}

using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.ChangeMasterVolume(0.0276625f);
        slider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeMasterVolume(val));
    }

}

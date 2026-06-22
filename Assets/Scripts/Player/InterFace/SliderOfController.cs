using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderOfController : MonoBehaviour 
{
    [SerializeField] private Controller controller;

    private Slider slider;

    private void Awake()
    {
        enabled = false;
    }
    public void Init(Controller controller)
    {
        slider = GetComponent<Slider>();
        slider.interactable = false;

        this.controller = controller;

        slider.minValue = 0.0f;
        slider.maxValue = controller.MaxValue;

        enabled = true;
    }
    
    private void Update()
    {
        slider.value =
            controller.Value / controller.MaxValue * slider.maxValue;
    }
}

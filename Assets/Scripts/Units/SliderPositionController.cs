using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SliderPositionController : NetworkBehaviour
{
    public Slider healthSlider;

    private RectTransform sliderRectTransform;

    private Camera _camera = null;

    private void Start()
    {
        // Get the RectTransform component of the slider
        sliderRectTransform = healthSlider.GetComponent<RectTransform>();
        healthSlider.maxValue = gameObject.GetComponent<InfoOfUnit>().StartHealthValue.Value;
    }

    private void LateUpdate()
    {
        if (Camera.current != null)
        {
            _camera = Camera.current;
        }

        if (Camera.allCamerasCount == 1)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            // Update the position of the slider to match the game object's position
            Vector3 objectPosition = transform.position + new Vector3(0, 2, 0);
            Vector3 sliderPosition = _camera.WorldToScreenPoint(objectPosition);

            // Set the position of the slider's RectTransform
            sliderRectTransform.position = sliderPosition;

            //Update Health Value
            UpdateHealthBarSlider();
        }
    }

    public void UpdateHealthBarSlider()
    {
        healthSlider.value = gameObject.GetComponent<InfoOfUnit>().HealthValue.Value;
    }
}

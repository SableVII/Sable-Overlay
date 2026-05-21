using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardSlider : MonoBehaviour, IModuleSettingUI
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _sliderText = null;
    public TMPro.TextMeshProUGUI SliderText { get { return _sliderText; } }

    [SerializeField]
    private Slider _slider = null;
    public Slider Slider { get { return _slider; } }

    [SerializeField]
    private TMPro.TextMeshProUGUI _sliderValueText = null;
    //public TMPro.TextMeshProUGUI SliderValueText { get { return _sliderValueText; } }

    private bool _useWholeNumbers = false;

    private float _defaultValue = 0.0f;
    public float DefaultValue { get { return _defaultValue; } }

    //// TEMP
    //public void Start()
    //{
    //    Initailize("Temp Test: ", 50.0f, temp, true, -5, 100);

    //}

    //private void temp(float sliderValue)
    //{

    //}

    public void Initailize(string sliderText, float startingSliderValue, UnityEngine.Events.UnityAction<float> sliderChangedAction, bool useWholeNumbers = false, float minSliderValue = 0.0f, float maxSliderValue = 100.0f, float defaultValue = 0.0f)
    {
        _slider.onValueChanged.AddListener(UpdateSliderValueText);

        _slider.wholeNumbers = useWholeNumbers;
        _useWholeNumbers = useWholeNumbers;

        _slider.minValue = minSliderValue;
        _slider.maxValue = maxSliderValue;
        _slider.value = startingSliderValue;
        _defaultValue = defaultValue;

        _slider.onValueChanged.AddListener(sliderChangedAction);

        _sliderText.text = sliderText;

        UpdateSliderValueText(_slider.value);
    }

    private void UpdateSliderValueText(float sliderValue)
    {
        if (_useWholeNumbers)
        {
            _sliderValueText.text = ((int)_slider.value).ToString();
        }
        else
        {
            _sliderValueText.text = _slider.value.ToString("n2");
        }
    }

    public void Reset()
    {
        _slider.value = _defaultValue;
    }

    public void SetEnabled(bool enabled)
    {
        _slider.enabled = enabled;
    }
}

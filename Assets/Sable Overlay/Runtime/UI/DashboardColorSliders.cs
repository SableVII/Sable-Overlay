using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardColorSliders : MonoBehaviour, IModuleSettingUI
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _text = null;

    [SerializeField]
    private Slider _redSlider = null;
    //public Slider RedSlider { get { return _redSlider; } }

    [SerializeField]
    private Slider _greenSlider = null;
    //public Slider GreenSlider { get { return _greenSlider; } }

    [SerializeField]
    private Slider _blueSlider = null;
    //public Slider BlueSlider { get { return _blueSlider; } }

    [SerializeField]
    private Slider _alphaSlider = null;
    //public Slider AlphaSlider { get { return _alphaSlider; } }

    [SerializeField]
    private TMPro.TextMeshProUGUI _redSliderValueText = null;

    [SerializeField]
    private TMPro.TextMeshProUGUI _greenSliderValueText = null;

    [SerializeField]
    private TMPro.TextMeshProUGUI _blueSliderValueText = null;

    [SerializeField]
    private TMPro.TextMeshProUGUI _alphaSliderValueText = null;

    [SerializeField]
    private Image _colorPreview = null;

    private Color _color = Color.white;
    public Color Color { get { return _color; } }

    private bool _callActions = false;

    private UnityEngine.Events.UnityEvent<Color> colorChangedEvent = new UnityEngine.Events.UnityEvent<Color>();

    private Color _defaultColor = Color.white;
    public Color DefaultColor { get { return _defaultColor; } }

    private bool _freezeColorUpdates = false;


    // TEMP
    //public void Start()
    //{
    //    Initailize("Temp Test: ", Color.gray, temp);
    //}

    //private void temp(Color sliderValue)
    //{

    //}

    public void Initailize(string colorSliderText, Color startingColor, UnityEngine.Events.UnityAction<Color> colorChangedAction, Color defaultColor = new Color())
    {
        _text.text = colorSliderText;

        _color = startingColor;
        _defaultColor = defaultColor;

        _redSlider.SetValueWithoutNotify(startingColor.r * 255);
        _greenSlider.SetValueWithoutNotify(startingColor.g * 255);
        _blueSlider.SetValueWithoutNotify(startingColor.b * 255);
        _alphaSlider.SetValueWithoutNotify(startingColor.a * 255);

        _redSliderValueText.text = ((int)_redSlider.value).ToString();
        _greenSliderValueText.text = ((int)_greenSlider.value).ToString();
        _blueSliderValueText.text = ((int)_blueSlider.value).ToString();
        _alphaSliderValueText.text = ((int)_alphaSlider.value).ToString();

        _redSlider.onValueChanged.AddListener(UpdateRedSlider);
        _greenSlider.onValueChanged.AddListener(UpdateGreenSlider);
        _blueSlider.onValueChanged.AddListener(UpdateBlueSlider);
        _alphaSlider.onValueChanged.AddListener(UpdateAlphaSlider);

        colorChangedEvent.AddListener(colorChangedAction);

        _colorPreview.color = _color;
    }

    public void SetColor(Color color)
    {
        // Freezing to avoid multiple color update calls being sent out
        _freezeColorUpdates = true;

        _redSlider.value = _defaultColor.r * 255;
        _greenSlider.value = _defaultColor.g * 255;
        _blueSlider.value = _defaultColor.b * 255;
        _alphaSlider.value = _defaultColor.a * 255;

        _freezeColorUpdates = false;

        UpdateColorValue();
    }

    private void UpdateRedSlider(float sliderValue)
    {
        _redSliderValueText.text = ((int)sliderValue).ToString();

        if (_freezeColorUpdates == false)
        {
            UpdateColorValue();
        }
    }

    private void UpdateGreenSlider(float sliderValue)
    {
        _greenSliderValueText.text = ((int)sliderValue).ToString();

        if (_freezeColorUpdates == false)
        {
            UpdateColorValue();
        }
    }

    private void UpdateBlueSlider(float sliderValue)
    {
        _blueSliderValueText.text = ((int)sliderValue).ToString();

        if (_freezeColorUpdates == false)
        {
            UpdateColorValue();
        }
    }

    private void UpdateAlphaSlider(float sliderValue)
    {
        _alphaSliderValueText.text = ((int)sliderValue).ToString();

        if (_freezeColorUpdates == false)
        {
            UpdateColorValue();
        }
    }

    private void UpdateColorValue(bool callActions = false)
    {
        Color previousColor = _color;

        _color.r = _redSlider.value/255.0f;
        _color.g = _greenSlider.value/255.0f;
        _color.b = _blueSlider.value/255.0f;
        _color.a = _alphaSlider.value/255.0f;

        if (previousColor != _color)
        {
            _colorPreview.color = _color;
            colorChangedEvent.Invoke(_color);
        }
    }

    public void Reset()
    {
        SetColor(_defaultColor);
    }

    public void SetEnabled(bool enabled)
    {
        _redSlider.enabled = enabled;
        _greenSlider.enabled = enabled;
        _blueSlider.enabled = enabled;
        _alphaSlider.enabled = enabled;
    }
}

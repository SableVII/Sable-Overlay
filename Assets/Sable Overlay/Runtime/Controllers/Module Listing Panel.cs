using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleListingPanel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _text = null;
    public TMPro.TextMeshProUGUI Text { get { return _text; } }

    [SerializeField]
    private Button _button = null;
    public Button Button { get { return _button; } }

    [SerializeField]
    private Image _selectedArrow = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Select()
    {
        _selectedArrow.gameObject.SetActive(true);
        _button.gameObject.SetActive(false);
        //_button.enabled = false;
    }

    public void Unselect()
    {
        _selectedArrow.gameObject.SetActive(false);
        _button.gameObject.SetActive(true);
        //_button.enabled = true;
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEnabled(bool enabled)
    {

    }
}

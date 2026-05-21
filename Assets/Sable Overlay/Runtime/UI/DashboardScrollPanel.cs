using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardScrollPanel : MonoBehaviour
{
    [SerializeField]
    private RectTransform _content = null;
    public RectTransform Content { get { return _content; } }

    private RectTransform _rectTransform = null;

    public void Initailize(float height = 400.0f)
    {
        _rectTransform = transform as RectTransform;

        ChangeHeight(height);
    }

    public void ChangeHeight(float height = 400.0f)
    {
        if (_rectTransform == null)
        {
            return;
        }

        Vector2 sizeDelta = _rectTransform.sizeDelta;
        sizeDelta.y = height;
        _rectTransform.sizeDelta = sizeDelta;
    }
}

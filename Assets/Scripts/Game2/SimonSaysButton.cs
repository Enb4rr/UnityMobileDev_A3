using UnityEngine;
using UnityEngine.UI;

public class SimonSaysButton : MonoBehaviour
{
    [Header("Button Info")]
    [SerializeField] private int _buttonIndex;

    [Header("Visuals")]
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightColor;

    public int ButtonIndex => _buttonIndex;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_buttonImage == null)
            _buttonImage = GetComponent<Image>();

        _button.onClick.AddListener(OnButtonClicked);

        SetNormalColor();
    }

    private void OnButtonClicked()
    {
        SimonSaysGameManager.Instance.PlayerPressedButton(_buttonIndex);
    }

    public void SetInteractable(bool value)
    {
        _button.interactable = value;
    }

    public void SetNormalColor()
    {
        _buttonImage.color = _normalColor;
    }

    public void SetHighlightColor()
    {
        _buttonImage.color = _highlightColor;
    }
}

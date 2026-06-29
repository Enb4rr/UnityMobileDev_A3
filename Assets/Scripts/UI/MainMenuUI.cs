using Managers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameOFTheDaySO _gameOfTheDay;
    [SerializeField] private WinnerOfTheDay _winnerOfTheDay;
    [SerializeField] private GameObject _winNotificationCanvas;
    [SerializeField] private GameObject _winNotificationPanel;
    [SerializeField] private GameObject _header;
    [SerializeField] private GameObject _subheader;
    [SerializeField] private GameObject _badgeImage;
    [SerializeField] private GameObject _inputField;
    [SerializeField] private GameObject _cancelButton;

    private void OnEnable()
    {
        if (!_winnerOfTheDay.viewedWinNotification)
        {
            EnableBadgeAlert();           
        }
    }

    private async void EnableBadgeAlert()
    {
        _badgeImage.GetComponent<UnityEngine.UI.Image>().sprite = await UserDataManager.Instance.DownloadSprite(_winnerOfTheDay.imageURL);

        _header.GetComponent<TMPro.TextMeshProUGUI>().text = "NEW BADGE EARNED!";
        _subheader.GetComponent<TMPro.TextMeshProUGUI>().text = "Yesterday you had the top score, here is your new badge!";
        _winNotificationCanvas.SetActive(true);
        _winNotificationPanel.SetActive(true);
        _badgeImage.SetActive(true);
        _inputField.SetActive(false);
        _cancelButton.SetActive(false);

        _winnerOfTheDay.viewedWinNotification = true;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGameOfTheDay()
    {
        SceneManager.LoadScene(_gameOfTheDay.GameName);
    }
}

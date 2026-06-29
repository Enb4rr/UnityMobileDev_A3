using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Badges;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UserDataUIController : MonoBehaviour
{
    #region Variables
    [Header("UI User Information")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Image profilePhoto;

    [Header("Badges")]
    [SerializeField] private List<GameObject> badges = new List<GameObject>();
    [SerializeField] private GameObject badgePrefab;
    [SerializeField] private Transform badgesParent;

    [Header("UI Change Username")]
    [SerializeField] private AlertPanel alertPanel;
    #endregion

    private void OnEnable()
    {
        if (UserDataManager.Instance == null) return;

        UserDataManager.Instance.UsernameChanged += OnUsernameChanged;
        UserDataManager.Instance.ProfilePhotoChanged += OnProfilePhotoChanged;
        UserDataManager.Instance.BadgesChanged += OnBadgesChanged;
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance == null) return;

        UserDataManager.Instance.UsernameChanged -= OnUsernameChanged;
        UserDataManager.Instance.ProfilePhotoChanged -= OnProfilePhotoChanged;
        UserDataManager.Instance.BadgesChanged -= OnBadgesChanged;
    }

    private void Start()
    {
        RefreshFromCachedData();
    }

    private void RefreshFromCachedData()
    {
        if (UserDataManager.Instance == null) return;

        if (!string.IsNullOrEmpty(UserDataManager.Instance.CurrentUsername))
        {
            OnUsernameChanged(UserDataManager.Instance.CurrentUsername);
        }

        if (UserDataManager.Instance.CurrentProfilePhoto != null)
        {
            OnProfilePhotoChanged(UserDataManager.Instance.CurrentProfilePhoto);
        }

        if (UserDataManager.Instance.CurrentBadges != null)
        {
            OnBadgesChanged(UserDataManager.Instance.CurrentBadges);
        }
    }

    private void OnUsernameChanged(string newUsername)
    {
        if (usernameText != null)
        {
            usernameText.text = newUsername;
        }
    }

    private void OnProfilePhotoChanged(Sprite newProfilePhoto)
    {
        if (profilePhoto != null && newProfilePhoto != null)
        {
            profilePhoto.sprite = newProfilePhoto;
        }
    }

    private async void OnBadgesChanged(List<BadgeData> badgesFromFirestore)
    {
        ClearBadges();

        if (badgesFromFirestore == null) return;
        if (badgePrefab == null || badgesParent == null) return;

        foreach (BadgeData badgeData in badgesFromFirestore)
        {
            Debug.Log("here");
            GameObject newBadge = Instantiate(badgePrefab, badgesParent);
            badges.Add(newBadge);

            if (newBadge.TryGetComponent(out Badge badge))
            {
                badge.UpdateBadgeInfo(badgeData.BadgeName);

                if (!string.IsNullOrEmpty(badgeData.BadgeImgURL))
                {
                    badge.BadgeImage.sprite = await UserDataManager.Instance.DownloadSprite(badgeData.BadgeImgURL);
                }
            }
        }
    }

    private void ClearBadges()
    {
        foreach (GameObject badge in badges)
        {
            if (badge != null)
            {
                Destroy(badge);
            }
        }

        badges.Clear();
    }

    public void ChangeUsernameButton()
    {
        if (alertPanel == null) return;

        alertPanel.ShowAlert("Change Username", "Type a new username!", () =>
        {
            _ = ChangeUsernameAsync(alertPanel.InputField.text);
        });
    }

    private async Task ChangeUsernameAsync(string newUsername)
    {
        if (string.IsNullOrEmpty(newUsername))
        {
            Debug.LogError("Username is required.");
            return;
        }

        try
        {
            await UserDataManager.Instance.SetUsernameInFirestore(newUsername);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error changing username: {e.Message}");
        }
    }
}
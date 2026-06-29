using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Leaderboard UI")]
    [SerializeField] private GameObject _leaderboardItemPrefab;
    [SerializeField] private Transform _leaderboardItemParent;

    private readonly List<GameObject> _spawnedItems = new List<GameObject>();
    private bool _isSubscribed;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenManagerIsReady());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator SubscribeWhenManagerIsReady()
    {
        while (LeaderboardManager.Instance == null)
        {
            yield return null;
        }

        if (_isSubscribed) yield break;

        LeaderboardManager.Instance.LeaderboardUpdated += PopulateLeaderboard;
        _isSubscribed = true;

        Debug.Log("LeaderboardUIController subscribed to LeaderboardUpdated.");

        if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
        {
            _ = LeaderboardManager.Instance.GetLeaderboardAsync();
        }
        else
        {
            Debug.Log("Leaderboard UI subscribed, waiting for Unity Authentication sign in.");
        }
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;
        if (LeaderboardManager.Instance == null) return;

        LeaderboardManager.Instance.LeaderboardUpdated -= PopulateLeaderboard;
        _isSubscribed = false;

        Debug.Log("LeaderboardUIController unsubscribed from LeaderboardUpdated.");
    }

    public async void RefreshLeaderboardButton()
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardManager.Instance is null.");
            return;
        }

        await LeaderboardManager.Instance.GetLeaderboardAsync();
    }

    public void PopulateLeaderboard(List<LeaderboardManager.LeaderboardEntryData> entries)
    {
        ClearLeaderboard();

        if (_leaderboardItemPrefab == null || _leaderboardItemParent == null)
        {
            Debug.LogError("Leaderboard UI references are missing.");
            return;
        }

        Debug.Log($"Populating leaderboard with {entries.Count} entries.");

        foreach (LeaderboardManager.LeaderboardEntryData entry in entries)
        {
            GameObject leaderboardItem = Instantiate(_leaderboardItemPrefab, _leaderboardItemParent);
            _spawnedItems.Add(leaderboardItem);

            TMP_Text[] texts = leaderboardItem.GetComponentsInChildren<TMP_Text>();

            if (texts.Length < 3)
            {
                Debug.LogError("Leaderboard item prefab needs at least 3 TMP_Text components.");
                continue;
            }

            texts[0].SetText((entry.Rank + 1).ToString());
            texts[1].SetText(entry.PlayerName);
            texts[2].SetText(entry.Score.ToString());

            if (entry.IsPlayerRecord)
            {
                texts[0].color = Color.green;
                texts[1].color = Color.green;
                texts[2].color = Color.green;
            }
        }
    }

    private void ClearLeaderboard()
    {
        foreach (GameObject item in _spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        _spawnedItems.Clear();

        if (_leaderboardItemParent == null) return;

        for (int i = _leaderboardItemParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_leaderboardItemParent.GetChild(i).gameObject);
        }
    }
}
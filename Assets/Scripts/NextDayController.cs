using Managers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class NextDayController : MonoBehaviour
{
    [SerializeField] private GameObject _leaderboardContainer;
    [SerializeField] private SceneAsset[] _gameScenes;
    [SerializeField] private GameOFTheDaySO _GameOfTheDaySO;
    [SerializeField] private WinnerOfTheDay _WinnerOfTheDaySO;

    private string projectId = "b8126043-1e53-48d5-a05b-b94f5b83d0d7";
    private string environmentId = "7a5a02d1-0822-439e-a700-7f35b1288e15";
    private string leaderboardId = "MDA3_Leaderboard";

    private string serviceAccountKeyId = "788fe1e8-bf1c-402d-beb4-01999959778a";
    private string serviceAccountKeySecret = "Snq337etW2dBNJ5O6zUEKA6yb-d8Ogu2";

    public void TriggerResetLeaderboard()
    {
        StartCoroutine(ResetLeaderboard());
    }

    private IEnumerator ResetLeaderboard()
    {
        UnityWebRequest www = UnityWebRequest.Delete($"https://services.api.unity.com/leaderboards/v1/projects/{projectId}/environments/{environmentId}/leaderboards/{leaderboardId}/scores");

        string authInfo = $"{serviceAccountKeyId}:{serviceAccountKeySecret}";
        string encodedAuthInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));

        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", $"Basic {encodedAuthInfo}");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            DeleteLeaderboardRecords();
            Debug.Log($"Leaderboard '{leaderboardId}' successfully reset. Response: {www.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Failed to reset leaderboard: {www.error}");
            Debug.LogError($"Error details: {www.downloadHandler.text}");
        }
    }

    private void DeleteLeaderboardRecords()
    {
        Transform parentTransform = _leaderboardContainer.transform;

        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            GameObject child = parentTransform.GetChild(i).gameObject;
            if(i == 0)
            {
                GetWinnerEmail(child);
            }
            Destroy(child);
        }

        ChangeGameOfTheDay();
    }

    private void GetWinnerEmail(GameObject winner)
    {
        Transform parentTransform = winner.transform;
        GameObject emailObject = parentTransform.GetChild(1).gameObject;

        _WinnerOfTheDaySO.email = emailObject.GetComponent<TextMeshProUGUI>().text;
        _WinnerOfTheDaySO.viewedWinNotification = false;

        StartCoroutine(GetBadges());
    }

    public void ChangeGameOfTheDay()
    {
        string gameName = _GameOfTheDaySO.GameName;

        for (int i = 0; i<_gameScenes.Length; i++)
        {
            if (gameName == _gameScenes[i].name)
            {
                if (i<_gameScenes.Length -1)
                {
                    _GameOfTheDaySO.GameName = _gameScenes[i+1].name;
                    break;

                }
                else
                    _GameOfTheDaySO.GameName = _gameScenes[0].name;
            }
        }

    }

    private IEnumerator GetBadges()
    {
        int imageIndex = UnityEngine.Random.Range(1, 200);
        UnityWebRequest www = UnityWebRequest.Get($"https://rickandmortyapi.com/api/character/{imageIndex}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }

        BadgeMapper badge = JsonConvert.DeserializeObject<BadgeMapper>(www.downloadHandler.text);


        BadgeData badgeData = new BadgeData
        {
            BadgeID = badge.Id,
            BadgeName = badge.Name,
            BadgeImgURL = badge.ImgURL
        };
        Debug.Log(badgeData.BadgeImgURL);
        _WinnerOfTheDaySO.imageURL = badgeData.BadgeImgURL;

        UserDataManager.Instance.SetBadgeInFirestore(badgeData);
    }

    [Serializable]
    public class BadgeMapper
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("image")]
        public string ImgURL { get; set; }
    }
}

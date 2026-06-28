using Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [SerializeField] private GameObject _leaderboardItemPrefab;
    [SerializeField] private GameObject _leaderboardItemParent;
    [SerializeField] private string _leaderboardId;
    private bool _eventsInitialized = false;

    //Credentials for testing

    //Score to push
    [SerializeField] private float _scoreToPush;

    [Serializable]
    public class ScoreMetadata
    {
        public string playerName;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartUnityServices();
    }

    public async void StartUnityServices()
    {

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                //var options = new InitializationOptions();
                //options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
            }
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("CLOUD SERVICES INITIALIZED");
            }

            if (!_eventsInitialized)
            {
                SetupEvents();
            }

        }
        catch (Exception e)
        {

        }


    }

    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
            await AddScore(username);

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            await AddScore(username);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            // Check if the error indicates the user already exists
            if (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                Debug.LogError("Sign up failed: This username is already taken.");
                await SignInWithUsernamePasswordAsync(username, password);
            }
            else
            {
                Debug.LogError($"Sign up failed: {ex.Message}");
            }
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Awaitable AddScore(string username)
    {
        try
        {
            var scoreMetadata = new ScoreMetadata { playerName = username };
            var playerEntry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(
                _leaderboardId, 
                _scoreToPush,
                new AddPlayerScoreOptions { Metadata = scoreMetadata }
                );

            await GetLeaderboard();
        }
        catch (Exception e)
        {
            Debug.Log("Failed to add score: "+e);
        }

    }

    public async Awaitable GetLeaderboard()
    {
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                _leaderboardId,
                new GetScoresOptions { IncludeMetadata = true});
            var scoresResponseResultsJSON = JsonConvert.SerializeObject(scoresResponse);
            Debug.Log("LEADERBOARD");
            Debug.Log(scoresResponseResultsJSON);

            foreach (var item in scoresResponse.Results)
            {
                PopulateLeaderboard(item.Rank, JObject.Parse(item.Metadata)["playerName"].ToString(), item.Score, AuthenticationService.Instance.PlayerId == item.PlayerId);
            }

            await GetPlayerScore();

        }
        catch (Exception e)
        {
            Debug.Log("Failed to retrieve leaderboard: "+e);

        }
    }

    private void PopulateLeaderboard(int rank, string playerName, double score, bool isPlayerRecord)
    {
        GameObject leaderboardItem = Instantiate(_leaderboardItemPrefab, _leaderboardItemParent.transform);

        leaderboardItem.GetComponentsInChildren<TMP_Text>()[0].SetText((rank + 1).ToString());
        if(isPlayerRecord) leaderboardItem.GetComponentsInChildren<TMP_Text>()[0].color = Color.green;
        leaderboardItem.GetComponentsInChildren<TMP_Text>()[1].SetText(playerName);
        leaderboardItem.GetComponentsInChildren<TMP_Text>()[2].SetText(score.ToString());

    }

    public async Awaitable GetPlayerScore()
    {
        var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
    }

    public async void TriggerAddScore()
    {
        await AddScore(AuthManager.Instance.Auth.CurrentUser.Email);
    }
    public async void TriggerGetLeaderboard()
    {
        await GetLeaderboard();
    }

    private void SetupEvents()
    {
        _eventsInitialized = true;

        AuthenticationService.Instance.SignedIn += () =>
        {

        };
        AuthenticationService.Instance.SignedOut += () =>
        {

        };
        AuthenticationService.Instance.Expired += () =>
        {

        };
    }
}

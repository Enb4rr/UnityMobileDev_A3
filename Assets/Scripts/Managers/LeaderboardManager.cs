using Managers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Leaderboard Settings")]
    [SerializeField] private string _leaderboardId;

    [Header("Testing")]
    [SerializeField] private float _scoreToPush;

    private bool _eventsInitialized;
    private bool _servicesInitialized;

    private Task _unityServicesInitTask;

    public event Action<List<LeaderboardEntryData>> LeaderboardUpdated;

    [Serializable]
    public class ScoreMetadata
    {
        public string playerName;
    }

    public class LeaderboardEntryData
    {
        public int Rank;
        public string PlayerName;
        public double Score;
        public bool IsPlayerRecord;

        public LeaderboardEntryData(int rank, string playerName, double score, bool isPlayerRecord)
        {
            Rank = rank;
            PlayerName = playerName;
            Score = score;
            IsPlayerRecord = isPlayerRecord;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }


    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        //await StartUnityServicesAsync();
        StartUnityServices();

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            }

            Debug.Log("Unity Authentication sign in successful.");

            //await AddScoreAsync(username, _scoreToPush);
            await GetLeaderboardAsync();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        //await StartUnityServicesAsync();
        StartUnityServices();

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            }

            Debug.Log("Unity Authentication sign up successful.");

            //await AddScoreAsync(username, _scoreToPush);
            await GetLeaderboardAsync();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign up failed: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task AddScoreAsync(string username, double score)
    {
        //await StartUnityServicesAsync();
        StartUnityServices();

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Cannot add score because Unity Authentication is not signed in.");
                return;
            }

            ScoreMetadata scoreMetadata = new ScoreMetadata
            {
                playerName = username
            };

            await LeaderboardsService.Instance.AddPlayerScoreAsync(
                _leaderboardId,
                score,
                new AddPlayerScoreOptions
                {
                    Metadata = scoreMetadata
                }
            );

            //await GetLeaderboardAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add score: {e.Message}");
        }
    }

    public async Task GetLeaderboardAsync()
    {
        //await StartUnityServicesAsync();
        StartUnityServices();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Cannot get leaderboard because Unity Authentication is not signed in.");
            return;
        }

        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                _leaderboardId,
                new GetScoresOptions
                {
                    IncludeMetadata = true
                }
            );

            List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>();

            foreach (var item in scoresResponse.Results)
            {
                string playerName = GetPlayerNameFromMetadata(item.Metadata);
                bool isPlayerRecord = AuthenticationService.Instance.PlayerId == item.PlayerId;

                entries.Add(new LeaderboardEntryData(
                    item.Rank,
                    playerName,
                    item.Score,
                    isPlayerRecord
                ));
            }

            Debug.Log($"Retrieved {entries.Count} leaderboard entries.");
            LeaderboardUpdated?.Invoke(entries);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to retrieve leaderboard: {e.Message}");
        }
    }

    private string GetPlayerNameFromMetadata(string metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return "Unknown";

        try
        {
            JObject metadataObject = JObject.Parse(metadata);

            if (metadataObject["playerName"] != null)
                return metadataObject["playerName"].ToString();
        }
        catch
        {
            Debug.LogWarning("Could not parse leaderboard metadata.");
        }

        return "Unknown";
    }

    public async Task GetPlayerScoreAsync()
    {
        //await StartUnityServicesAsync();
        StartUnityServices();

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Cannot get player score because Unity Authentication is not signed in.");
                return;
            }

            var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get player score: {e.Message}");
        }
    }

    //public async void TriggerAddScore()
    //{
    //    string email = AuthManager.Instance != null
    //        ? AuthManager.Instance.CurrentUserEmail
    //        : "Unknown";

    //    await AddScoreAsync(email, _scoreToPush);
    //}

    public async void TriggerGetLeaderboard()
    {
        await GetLeaderboardAsync();
    }

    private void SetupEvents()
    {
        _eventsInitialized = true;

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Unity Authentication Signed In");
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Unity Authentication Signed Out");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Unity Authentication Session Expired");
        };
    }
}
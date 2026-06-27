using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{

    [SerializeField] private GameObject _leaderboardItemPrefab;
    [SerializeField] private GameObject _leaderboardItemParent;
    [SerializeField] private string _leaderboardId;
    //private string _leaderboardId;
    private bool _eventsInitialized = false;

    //Credentials for testing
    [SerializeField] private string _username;
    [SerializeField] private string _password;

    //Score to push
    [SerializeField] private float _scoreToPush;

    private void Start()
    {
        StartUnityServices();
        //_leaderboardId = LevelManager.Instance.CurrentLevel.id;
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
            //if (!AuthenticationService.Instance.SessionTokenExists)
            //{
            await SignInWithUsernamePasswordAsync(_username, _password);
            //}

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
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await SignUpWithUsernamePasswordAsync(username, password);
            }
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
    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
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

    //public async void SignInAnonymously()
    //{

    //    try
    //    {
    //        await AuthenticationService.Instance.SignInAnonymouslyAsync(new SignInOptions() { CreateAccount = true });
    //        Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
    //    }
    //    catch (AuthenticationException e)
    //    {
    //        Debug.Log("AUTHENTICATION ERROR: " + e);
    //    }
    //    catch (RequestFailedException e)
    //    {
    //        Debug.Log("REQUEST ERROR: " + e);
    //    }

    //}

    public async Awaitable AddScore()
    {
        try
        {
            Debug.Log($"LEADERBOARD ID: {_leaderboardId} SCORE: {_scoreToPush}");
            var playerEntry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(_leaderboardId, _scoreToPush);

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
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
            var scoresResponseResultsJSON = JsonConvert.SerializeObject(scoresResponse);
            Debug.Log("LEADERBOARD");
            Debug.Log(scoresResponseResultsJSON);

            foreach (var item in scoresResponse.Results)
            {
                PopulateLeaderboard(item.Rank, item.PlayerName, item.Score, AuthenticationService.Instance.PlayerId == item.PlayerId);
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
        //TextMeshPro[] leaderboardItemTexts = leaderboardItem.GetComponentsInChildren<TextMeshPro>();

        leaderboardItem.GetComponentsInChildren<TMP_Text>()[0].SetText((rank + 1).ToString());
        leaderboardItem.GetComponentsInChildren<TMP_Text>()[0].color = isPlayerRecord ? Color.green : Color.black;
        leaderboardItem.GetComponentsInChildren<TMP_Text>()[1].SetText(playerName);
        //leaderboardItem.GetComponentsInChildren<TMP_Text>()[1].SetText("");
        leaderboardItem.GetComponentsInChildren<TMP_Text>()[2].SetText(score.ToString());

    }

    public async Awaitable GetPlayerScore()
    {
        var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
    }

    public async void TriggerAddScore()
    {
        await AddScore();
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

using TMPro;
using UnityEngine;

public class CatchGameManager : MonoBehaviour
{
    #region  variables
    public static CatchGameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int _startingLives = 3;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private GameObject _losePanel;

    [Header("Player")]
    [SerializeField] private PlayerCredentials _playerCredentials;

    private int _score;
    private int _lives;
    private bool _gameEnded;

    public bool GameEnded => _gameEnded;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Initialize game with starting values
        _score = 0;
        _lives = _startingLives;
        _gameEnded = false;

        if (_losePanel != null) _losePanel.SetActive(false); // Deactivate Lose panel

        UpdateUI(); // Update UI
    }

    public void AddScore(int amount)
    {
        if (_gameEnded) return;

        _score += amount; // increase score
        UpdateUI(); // Update UI
    }

    public void LoseLife()
    {
        if (_gameEnded) return;

        _lives--; // decrease lives
        UpdateUI(); // update UI

        if (_lives <= 0) EndGame();
    }

    private async void EndGame()
    {
        _gameEnded = true;
    
        if (_finalScoreText != null) _finalScoreText.text = $"Final Score: {_score}"; // Update final score text
        if (_losePanel != null) _losePanel.SetActive(true); // Activate Lose panel
        await LeaderboardManager.Instance.AddScoreAsync(_playerCredentials.email, _score); // Submit score to leaderboard
    }

    private void UpdateUI()
    {
        _scoreText.text = $"Score: {_score}"; // Update score text
        _livesText.text = $"Lives: {_lives}"; // Update lives text
    }
}
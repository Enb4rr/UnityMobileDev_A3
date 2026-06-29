using TMPro;
using UnityEngine;

public class ReactionGameManager : MonoBehaviour
{
    public static ReactionGameManager Instance { get; private set; }

    #region Variables
    [Header("Game Settings")]
    [SerializeField] private int _initialLives = 3;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private GameObject _losePanel;

    private int _score;
    private int _lives;
    private bool _gameEnded;

    #endregion

    public bool GameEnded => _gameEnded;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        // Initialize game state
        _score = 0;
        _lives = _initialLives;
        _gameEnded = false;

        _losePanel.SetActive(false);

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (_gameEnded) return;

        // Update score
        _score += amount;
        UpdateUI();
    }

    public void ReduceLives()
    {
        if (_gameEnded) return;

        // Decrease lives
        _lives--;
        UpdateUI();

        if (_lives <= 0) EndGame(); // End game 
    }

    private void EndGame()
    {
        _gameEnded = true;
    
        if (_finalScoreText != null) _finalScoreText.text = $"Final Score: {_score}"; // Update final score text
        if (_losePanel != null) _losePanel.SetActive(true); // Activate Lose panel
    }

    private void UpdateUI()
    {
        if (_scoreText != null) _scoreText.text = $"Score: {_score}"; // Update score text
        if (_livesText != null) _livesText.text = $"Lives: {_lives}"; // Update lives text
    }
}

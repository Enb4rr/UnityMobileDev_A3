using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimonSaysGameManager : MonoBehaviour
{
    public static SimonSaysGameManager Instance { get; private set; }

    #region Variables
    [Header("Game Settings")]
    [SerializeField] private float _flashDuration = 0.4f;
    [SerializeField] private float _timeBetweenFlashes = 0.2f;
    [SerializeField] private float _timeBeforeNextRound = 0.8f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _instructionText;
    [SerializeField] private SimonSaysButton[] _simonButtons;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private GameObject _buttonsPanel;

    private List<int> _sequence = new List<int>();
    
    private int _score;
    private int _playerIndex;
    private bool _canPlayerPress;
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
        _playerIndex = 0;
        _canPlayerPress = false;
        _gameEnded = false;

        _losePanel.SetActive(false); // Hide the lose panel

        UpdateUI();
        SetButtonsInteractable(false); // Disable buttons at the start
        StartCoroutine(StartNextRound());
    }

    private IEnumerator StartNextRound()
    {
        _canPlayerPress = false;
        SetButtonsInteractable(false);

        _instructionText.text = "Memorize the sequence"; // Change instruction text 

        yield return new WaitForSeconds(_timeBeforeNextRound);

        AddRandomButtonToSequence(); // Add a new button to the sequence

        yield return StartCoroutine(ShowSequence()); // Show sequence

        _instructionText.text = "Repeat the sequence"; // Change instruction text
        _playerIndex = 0;

        // Allow player to press buttons
        _canPlayerPress = true;
        SetButtonsInteractable(true);
    }

    private void AddRandomButtonToSequence()
    {
        int randomIndex = Random.Range(0, _simonButtons.Length); // Get a random button index
        _sequence.Add(randomIndex); // Add a random button index to the sequence
    }

    private IEnumerator ShowSequence()
    {
        for (int i = 0; i < _sequence.Count; i++)
        {
            int buttonIndex = _sequence[i]; // Get the button
            SimonSaysButton button = _simonButtons[buttonIndex];

            button.SetHighlightColor(); // Highlight the button

            yield return new WaitForSeconds(_flashDuration);

            button.SetNormalColor(); // Return to normal color

            yield return new WaitForSeconds(_timeBetweenFlashes);
        }
    }

    public void PlayerPressedButton(int buttonIndex)
    {
        if (!_canPlayerPress || _gameEnded) return;

        StartCoroutine(FlashPlayerButton(buttonIndex));

        int expectedButton = _sequence[_playerIndex]; // Get expected button

        if (buttonIndex != expectedButton) // If the wrong button was pressed
        {
            EndGame(); // Finish game
            return;
        }

        _playerIndex++; // Move to the next button in the sequence

        if (_playerIndex >= _sequence.Count) // If the player has completed the sequence
        {
            CompleteRound(); // Complete the round
        }
    }

    private IEnumerator FlashPlayerButton(int buttonIndex)
    {
        SimonSaysButton button = _simonButtons[buttonIndex];

        button.SetHighlightColor();

        yield return new WaitForSeconds(0.15f);

        button.SetNormalColor();
    }

    private void CompleteRound()
    {
        // Increase score
        _score++;
        UpdateUI();

        // Prepare for the next round
        _canPlayerPress = false;
        SetButtonsInteractable(false);

        StartCoroutine(StartNextRound()); // Start the next round
    }

    private void EndGame()
    {
        // Set game state to ended
        _gameEnded = true;
        _canPlayerPress = false;
        SetButtonsInteractable(false);

        if (_instructionText != null) _instructionText.text = "Game Over";
        if (_finalScoreText != null) _finalScoreText.text = $"Final Score: {_score}";

        if (_losePanel != null) _losePanel.SetActive(true);

        if (_buttonsPanel != null) _buttonsPanel.SetActive(false);
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (SimonSaysButton button in _simonButtons)
        {
            button.SetInteractable(value);
        }
    }

    private void UpdateUI()
    {
        if (_scoreText != null) _scoreText.text = $"Score: {_score}";
    }
}

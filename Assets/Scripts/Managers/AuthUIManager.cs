using System;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager Instance { get; private set; }

    #region Variables
    [Header("UI Login References")]
    [SerializeField] private TMP_InputField emailLoginInputField;
    [SerializeField] private TMP_InputField passwordLoginInputField;
    [SerializeField] private Button loginButton;

    [Header("UI Register References")]
    [SerializeField] private TMP_InputField emailRegisterInputField;
    [SerializeField] private TMP_InputField passwordRegisterInputField;
    [SerializeField] private Button registerButton;

    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject userPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Change Password")]
    [SerializeField] private AlertPanel alertPanel;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            LoadMenuPanel();
        }
        else
        {
            LoadLoginPanel();
        }
    }

    #region Button Functions

    public async void LoginButton()
    {
        if (AuthManager.Instance == null) return;

        string email = emailLoginInputField.text;
        string password = passwordLoginInputField.text;

        if (!ValidateInputFields(email, password)) return;

        SetAuthButtonsInteractable(false);

        try
        {
            await AuthManager.Instance.LoginEmailPasswordAsync(email, password);
            RefreshUI();
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
        finally
        {
            SetAuthButtonsInteractable(true);
        }
    }

    public async void RegisterButton()
    {
        if (AuthManager.Instance == null) return;

        string email = emailRegisterInputField.text;
        string password = passwordRegisterInputField.text;

        if (!ValidateInputFields(email, password, true)) return;

        SetAuthButtonsInteractable(false);

        try
        {
            await AuthManager.Instance.RegisterEmailPasswordAsync(email, password);
            RefreshUI();
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
        finally
        {
            SetAuthButtonsInteractable(true);
        }
    }

    public async void GoogleLoginButton()
    {
        if (AuthManager.Instance == null) return;

#if UNITY_EDITOR
        Debug.LogError("Google Login Button Not Supported in Unity Editor");
        return;
#else
        try
        {
            await AuthManager.Instance.LoginWithGoogleAsync();
            RefreshUI();
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
#endif
    }

    public void LogoutButton()
    {
        if (AuthManager.Instance == null) return;

        AuthManager.Instance.Logout();
        RefreshUI();
    }

    public void UpdatePasswordButton()
    {
        if (AuthManager.Instance == null) return;

        alertPanel.ShowAlert("Change Password", "Type your new password", async () =>
        {
            string newPassword = alertPanel.InputField.text;

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowError("Password is required");
                return;
            }

            try
            {
                await AuthManager.Instance.UpdatePasswordAsync(newPassword);
            }
            catch (Exception e)
            {
                ShowError(e.Message);
            }
        });
    }

    #endregion

    #region Panel Functions

    private void LoadLoginPanel()
    {
        introPanel?.SetActive(true);
        menuPanel?.SetActive(false);
        userPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
    }

    private void LoadMenuPanel()
    {
        introPanel?.SetActive(false);
        menuPanel?.SetActive(true);
        userPanel?.SetActive(false);
    }

    #endregion

    #region Validation

    private bool ValidateInputFields(string email, string password, bool isRegistering = false)
    {
        if (string.IsNullOrEmpty(email))
        {
            ShowError("Email is required");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Password is required");
            return false;
        }

        if (isRegistering && password.Length < 8)
        {
            ShowError("Password is too short, has to be at least 8 characters long");
            return false;
        }

        return true;
    }

    private void SetAuthButtonsInteractable(bool interactable)
    {
        if (loginButton != null)
            loginButton.interactable = interactable;

        if (registerButton != null)
            registerButton.interactable = interactable;
    }

    private void ShowError(string error)
    {
        Debug.LogError(error);
    }

    #endregion
}
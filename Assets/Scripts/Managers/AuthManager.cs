using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Google;

namespace Managers
{
    public class AuthManager : BasePersistentManager<AuthManager>
    {
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

        [Header("UI Change Password")] 
        [SerializeField] private TMP_InputField changePasswordInputField;

        public FirebaseAuth Auth { get; private set; }
        private GoogleSignInConfiguration googleConfig;

        private async void Start()
        {
            try
            {
                await InitializeFirebaseAsync();
                InitializeGoogleSignIn();

                if (CheckCurrentUser())
                {
                    if (!await UserDataManager.Instance.IsUserInFirestore(Auth.CurrentUser.UserId))
                    {
                        await UserDataManager.Instance.CreatePlayerProfileAsync(Auth.CurrentUser.Email, GetUsernameFromEmail(Auth.CurrentUser.Email));
                    }
                    UserDataManager.Instance.InitializeListeners(Auth.CurrentUser.UserId);
                }
                else
                {
                    LoadLoginPanel();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        #region Auth Services Initialization

        // Firebase Auth Initialization
        private async Task InitializeFirebaseAsync()
        {
            DependencyStatus dependenciesStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependenciesStatus == DependencyStatus.Available) Auth = FirebaseAuth.DefaultInstance;
            else Debug.LogError($"Could not resolve all Firebase dependencies: {dependenciesStatus}");
        }

        // Google Sign In Services Initialization
        private void InitializeGoogleSignIn()
        {
            googleConfig = new GoogleSignInConfiguration()
            {
                WebClientId = "1018292918451-t9uvltu2dsnspclpmml7h58cfaes1csn.apps.googleusercontent.com",
                RequestIdToken = true,
                RequestEmail = true,
            };

            GoogleSignIn.Configuration = googleConfig;
        }

        #endregion
        
        #region Button Functions
        
        public async void LoginButton()
        {
            if (!ValidateInputFields(emailLoginInputField.text, passwordLoginInputField.text)) return;
            
            try
            {
                await LoginEmailPasswordAsync(emailLoginInputField.text, passwordLoginInputField.text);
                
                if (!await UserDataManager.Instance.IsUserInFirestore(Auth.CurrentUser.UserId))
                {
                    await UserDataManager.Instance.CreatePlayerProfileAsync(Auth.CurrentUser.Email, GetUsernameFromEmail(Auth.CurrentUser.Email));
                }
            }
            catch (Exception e)
            {
                ShowError(e.Message);
            }
        }

        public async void RegisterButton()
        {
            if (!ValidateInputFields(emailRegisterInputField.text, passwordRegisterInputField.text, true)) return;
            
            try
            {
                await RegisterEmailPasswordAsync(emailRegisterInputField.text, passwordRegisterInputField.text);
                await UserDataManager.Instance.CreatePlayerProfileAsync(Auth.CurrentUser.Email, GetUsernameFromEmail(Auth.CurrentUser.Email));
            }
            catch (Exception e)
            {
                ShowError(e.Message);
            }
        }
        
        public async Task GoogleLoginButton()
        {
#if UNITY_EDITOR
            Debug.LogError("Google Login Button Not Supported");
            return;
#endif

            await LoginWithGoogleAsync();
        }

        public async Task UpdatePasswordButton()
        {
            FirebaseUser user = Auth.CurrentUser;
            
            if (user == null) return;
            
            await user.UpdatePasswordAsync(changePasswordInputField.text).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Google Login Button Cancelled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"Error changing the password, {task.Exception}");
                    return;
                }
                
                Debug.Log("Password changed successfully!");
            });
        }
        
        #endregion

        private async Task LoginWithGoogleAsync()
        {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            FirebaseUser firebaseUser = await Auth.SignInWithCredentialAsync(credential);
        }

        private async Task LoginEmailPasswordAsync(string email, string password)
        {
            try
            {
                AuthResult result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
                UserDataManager.Instance.InitializeListeners(Auth.CurrentUser.Email);
                LoadMenuPanel();
            }
            catch (Exception e)
            {
                ShowError(e.Message);
            }
        }

        private async Task RegisterEmailPasswordAsync(string email, string password, string username = "")
        {
            try
            {
                AuthResult result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
                UserDataManager.Instance.InitializeListeners(Auth.CurrentUser.Email);
                LoadMenuPanel();
            }
            catch (Exception e)
            {
                ShowError(e.Message);
            }
        }

        public void Logout()
        {
            Auth.SignOut();
            LoadLoginPanel();
        }

        private bool CheckCurrentUser()
        {
            return Auth.CurrentUser != null;
        }

        private void ShowError(string error)
        {
            Debug.LogError(error);
        }

        private void ExecuteLoginLoading(bool active)
        {
            loginButton.interactable = active;
            registerButton.interactable = active;
        }

        private bool ValidateInputFields(string email, string password, bool isRegistering = false, bool checkUsername = false, string username = "")
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

            if (isRegistering && password.Length < 6)
            {
                ShowError("Password is too short");
                return false;
            }

            if (checkUsername && string.IsNullOrEmpty(username))
            {
                ShowError("Username is required");
                return false;
            }

            return true;
        }

        private string GetUsernameFromEmail(string email)
        {
            return email.Contains("@") ? email.Split('@')[0] : null;
        }

        private void LoadLoginPanel()
        {
            introPanel?.SetActive(true);
            menuPanel?.SetActive(false);
            userPanel?.SetActive(false);
        }

        private void LoadMenuPanel()
        {
            introPanel?.SetActive(false);
            menuPanel?.SetActive(true);
        }
    }
}
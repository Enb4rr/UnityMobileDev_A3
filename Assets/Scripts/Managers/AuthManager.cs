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
        [Header("UI Login References")] [SerializeField]
        private TMP_InputField emailLoginInputField;

        [SerializeField] private TMP_InputField passwordLoginInputField;
        [SerializeField] private Button loginButton;

        [Header("UI Register References")] [SerializeField]
        private TMP_InputField emailRegisterInputField;

        [SerializeField] private TMP_InputField passwordRegisterInputField;
        [SerializeField] private Button registerButton;

        private FirebaseAuth auth;
        private GoogleSignInConfiguration googleConfig;

        private async void Start()
        {
            try
            {
                await InitializeFirebaseAsync();
                ConfigureGoogleSignIn();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private async Task InitializeFirebaseAsync()
        {
            DependencyStatus dependenciesStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependenciesStatus == DependencyStatus.Available) auth = FirebaseAuth.DefaultInstance;
            else Debug.LogError($"Could not resolve all Firebase dependencies: {dependenciesStatus}");
        }

        private void ConfigureGoogleSignIn()
        {
            googleConfig = new GoogleSignInConfiguration()
            {
                WebClientId = "1018292918451-t9uvltu2dsnspclpmml7h58cfaes1csn.apps.googleusercontent.com",
                RequestIdToken = true,
                RequestEmail = true,
            };

            GoogleSignIn.Configuration = googleConfig;
        }

        public async void GoogleLoginButton()
        {
#if UNITY_EDITOR
            Debug.LogError("Google Login Button Not Supported");
            return;
#endif

            await LoginWithGoogleAsync();
        }

        private async Task LoginWithGoogleAsync()
        {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            FirebaseUser firebaseUser = await auth.SignInWithCredentialAsync(credential);
            Debug.Log("Google Login Success: " + firebaseUser.Email);
        }

        public async void LoginButton()
        {
            try
            {
                await LoginEmailPasswordAsync(emailLoginInputField.text, passwordLoginInputField.text);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public async void RegisterButton()
        {
            try
            {
                await RegisterEmailPasswordAsync(emailRegisterInputField.text, passwordRegisterInputField.text);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private async Task LoginEmailPasswordAsync(string email, string password)
        {
            try
            {
                await auth.SignInWithEmailAndPasswordAsync(email, password);
                Debug.Log("Login Success");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private async Task RegisterEmailPasswordAsync(string email, string password, string username = "")
        {
            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                Debug.Log("Register Success");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public void Logout()
        {
            auth.SignOut();
        }

        public bool CheckCurrentUser()
        {
            return auth.CurrentUser != null;
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

        public bool ValidateInputFields(string email, string password, bool checkUsername = false, string username = "")
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

            if (password.Length < 6)
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
    }
}
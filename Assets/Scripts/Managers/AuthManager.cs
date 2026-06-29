using Firebase;
using Firebase.Auth;
using Google;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Managers
{
    public class AuthManager : BasePersistentManager<AuthManager>
    {
        public FirebaseAuth Auth { get; private set; }

        private GoogleSignInConfiguration _googleConfig;

        public bool IsInitialized { get; private set; }
        public bool IsLoggedIn => Auth != null && Auth.CurrentUser != null;
        public string CurrentUserEmail => Auth?.CurrentUser?.Email;

        public event Action AuthStateChanged;

        private async void Start()
        {
            try
            {
                await EnsureInitializedAsync();

                if (IsLoggedIn)
                {
                    await InitializeCurrentUserSessionAsync();
                }

                NotifyAuthStateChanged();
            }
            catch (Exception e)
            {
                Debug.LogError($"Auth initialization error: {e.Message}");
            }
        }

        #region Initialization

        private async Task EnsureInitializedAsync()
        {
            if (IsInitialized) return;

            await InitializeFirebaseAsync();
            InitializeGoogleSignIn();

            IsInitialized = true;
        }

        private async Task InitializeFirebaseAsync()
        {
            DependencyStatus dependenciesStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependenciesStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                throw new Exception($"Could not resolve all Firebase dependencies: {dependenciesStatus}");
            }
        }

        private void InitializeGoogleSignIn()
        {
            _googleConfig = new GoogleSignInConfiguration()
            {
                WebClientId = "1018292918451-t9uvltu2dsnspclpmml7h58cfaes1csn.apps.googleusercontent.com",
                RequestIdToken = true,
                RequestEmail = true,
            };

            GoogleSignIn.Configuration = _googleConfig;
        }

        #endregion

        #region Auth Methods

        public async Task LoginEmailPasswordAsync(string email, string password)
        {
            await EnsureInitializedAsync();

            await Auth.SignInWithEmailAndPasswordAsync(email, password);

            await InitializeCurrentUserSessionAsync();

            await LeaderboardManager.Instance.SignInWithUsernamePasswordAsync(email, password);

            NotifyAuthStateChanged();
        }

        public async Task RegisterEmailPasswordAsync(string email, string password, string username = "")
        {
            await EnsureInitializedAsync();

            await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            await InitializeCurrentUserSessionAsync();

            await LeaderboardManager.Instance.SignUpWithUsernamePasswordAsync(email, password);

            NotifyAuthStateChanged();
        }

        public async Task LoginWithGoogleAsync()
        {
#if UNITY_EDITOR
            Debug.LogError("Google Login Button Not Supported in Unity Editor");
            return;
#else
            await EnsureInitializedAsync();

            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            await Auth.SignInWithCredentialAsync(credential);

            await InitializeCurrentUserSessionAsync();

            NotifyAuthStateChanged();
#endif
        }

        public async Task UpdatePasswordAsync(string newPassword)
        {
            await EnsureInitializedAsync();

            FirebaseUser user = Auth.CurrentUser;

            if (user == null)
            {
                throw new Exception("No user is currently logged in.");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new Exception("Password is required.");
            }

            await user.UpdatePasswordAsync(newPassword);
        }

        public void Logout()
        {
            if (Auth != null)
            {
                Auth.SignOut();
            }

            NotifyAuthStateChanged();
            AuthUIManager.Instance.RefreshUI();
        }

        #endregion

        #region User Session

        private async Task InitializeCurrentUserSessionAsync()
        {
            FirebaseUser user = Auth.CurrentUser;

            if (user == null) return;

            string email = user.Email;

            if (string.IsNullOrEmpty(email)) return;

            if (!await UserDataManager.Instance.IsUserInFirestore(email))
            {
                await UserDataManager.Instance.CreatePlayerProfileAsync(
                    email,
                    GetUsernameFromEmail(email)
                );
            }

            await UserDataManager.Instance.InitializeListenersAsync(email);
        }

        private string GetUsernameFromEmail(string email)
        {
            return email.Contains("@") ? email.Split('@')[0] : email;
        }

        private void NotifyAuthStateChanged()
        {
            AuthStateChanged?.Invoke();
        }

        #endregion
    }
}
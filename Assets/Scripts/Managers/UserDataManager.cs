using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Badges;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    public class UserDataManager : BasePersistentManager<UserDataManager>
    {
        [Header("Default User Data")]
        [SerializeField] private Sprite defaultProfilePhoto;

        private FirebaseFirestore _db;
        private ListenerRegistration _userListener;

        public string CurrentUsername { get; private set; }
        public Sprite CurrentProfilePhoto { get; private set; }
        public List<BadgeData> CurrentBadges { get; private set; } = new List<BadgeData>();

        public event Action<string> UsernameChanged;
        public event Action<Sprite> ProfilePhotoChanged;
        public event Action<List<BadgeData>> BadgesChanged;

        private bool _isInitialized;

        // private async void Start()
        // {
        //     try
        //     {
        //         await EnsureInitializedAsync();
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"UserDataManager initialization error: {e.Message}");
        //     }
        // }

        public async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;

            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status == DependencyStatus.Available)
            {
                _db = FirebaseFirestore.DefaultInstance;
                _isInitialized = true;
            }
            else
            {
                throw new Exception($"Could not resolve Firebase dependencies: {status}");
            }
        }

        public async Task CreatePlayerProfileAsync(string email, string username)
        {
            await EnsureInitializedAsync();

            UserData newUser = new UserData
            {
                Username = username,
                ProfilePhoto = GetDefaultProfilePhotoAsString(),
                Badges = new List<BadgeData>(),
                MaxOwnGameScore = 0,
            };

            DocumentReference userRef = _db.Collection("Users").Document(email);
            await userRef.SetAsync(newUser);
        }

        public async Task<bool> IsUserInFirestore(string email)
        {
            await EnsureInitializedAsync();

            DocumentReference docRef = _db.Collection("Users").Document(email);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            return snapshot.Exists;
        }

        public async Task InitializeListenersAsync(string email)
        {
            await EnsureInitializedAsync();

            StopListeners();

            DocumentReference userRef = _db.Collection("Users").Document(email);

            _userListener = userRef.Listen(snapshot =>
            {
                if (!snapshot.Exists) return;

                if (snapshot.TryGetValue<string>("Username", out string username))
                {
                    CurrentUsername = username;
                    UsernameChanged?.Invoke(CurrentUsername);
                }

                if (snapshot.TryGetValue<string>("ProfilePhoto", out string profilePhotoString))
                {
                    if (!string.IsNullOrEmpty(profilePhotoString))
                    {
                        Texture2D texture = FromStringToTextureConverter(profilePhotoString);

                        CurrentProfilePhoto = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f)
                        );

                        ProfilePhotoChanged?.Invoke(CurrentProfilePhoto);
                    }
                }

                if (snapshot.TryGetValue<List<BadgeData>>("Badges", out List<BadgeData> badgesFromFirestore))
                {
                    CurrentBadges = badgesFromFirestore ?? new List<BadgeData>();
                    BadgesChanged?.Invoke(CurrentBadges);
                }
            });
        }

        public void StopListeners()
        {
            _userListener?.Stop();
            _userListener = null;
        }

        public async Task SetUsernameInFirestore(string newUsername)
        {
            await EnsureInitializedAsync();

            string email = AuthManager.Instance.CurrentUserEmail;

            if (string.IsNullOrEmpty(email)) return;

            DocumentReference userRef = _db.Collection("Users").Document(email);
            await userRef.UpdateAsync("Username", newUsername);
        }

        public async Task SetProfilePhotoInFirestore(Texture2D newProfilePhoto)
        {
            await EnsureInitializedAsync();

            string email = AuthManager.Instance.CurrentUserEmail;

            if (string.IsNullOrEmpty(email)) return;
            if (newProfilePhoto == null) return;

            DocumentReference userRef = _db.Collection("Users").Document(email);
            await userRef.UpdateAsync("ProfilePhoto", FromTextureToStringConverter(newProfilePhoto));
        }

        public async Task SetBadgeInFirestore(BadgeData badgeData)
        {
            await EnsureInitializedAsync();

            string email = AuthManager.Instance.CurrentUserEmail;

            if (string.IsNullOrEmpty(email)) return;

            DocumentReference badgesRef = _db.Collection("Users").Document(email);

            await _db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(badgesRef);

                if (!snapshot.Exists) return;

                List<BadgeData> badges = snapshot.GetValue<List<BadgeData>>("Badges");
                badges ??= new List<BadgeData>();

                badges.Add(badgeData);

                transaction.Update(badgesRef, "Badges", badges);
            });
        }

        public async Task SetMaxScoreInFirestore(int maxOwnGameScore)
        {
            await EnsureInitializedAsync();

            string email = AuthManager.Instance.CurrentUserEmail;

            if (string.IsNullOrEmpty(email)) return;

            DocumentReference userRef = _db.Collection("Users").Document(email);
            await userRef.UpdateAsync("MaxOwnGameScore", maxOwnGameScore);
        }

        public async Task<Sprite> DownloadSprite(string url)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        private string GetDefaultProfilePhotoAsString()
        {
            if (defaultProfilePhoto != null)
            {
                return FromTextureToStringConverter(defaultProfilePhoto.texture);
            }

            Texture2D fallbackTexture = new Texture2D(2, 2);
            fallbackTexture.SetPixels(new[]
            {
                Color.white, Color.white,
                Color.white, Color.white
            });
            fallbackTexture.Apply();

            string result = FromTextureToStringConverter(fallbackTexture);

            Destroy(fallbackTexture);

            return result;
        }

        private string FromTextureToStringConverter(Texture2D texture)
        {
            Texture2D resized = ResizeTexture(texture, 128, 128);

            byte[] imageBytes = resized.EncodeToJPG(50);

            Destroy(resized);

            return Convert.ToBase64String(imageBytes);
        }

        private Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);

            Graphics.Blit(source, renderTexture);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            return result;
        }

        private Texture2D FromStringToTextureConverter(string textureString)
        {
            byte[] imageBytes = Convert.FromBase64String(textureString);

            Texture2D result = new Texture2D(2, 2);
            result.LoadImage(imageBytes);

            return result;
        }

        private void OnDestroy()
        {
            StopListeners();
        }
    }

    [FirestoreData]
    public class UserData
    {
        [FirestoreProperty] public string Username { get; set; }
        [FirestoreProperty] public string ProfilePhoto { get; set; }
        [FirestoreProperty] public List<BadgeData> Badges { get; set; }
        [FirestoreProperty] public int MaxOwnGameScore { get; set; }
    }

    [FirestoreData]
    public class BadgeData
    {
        [FirestoreProperty] public int BadgeID { get; set; }
        [FirestoreProperty] public string BadgeName { get; set; }
        [FirestoreProperty] public string BadgeImgURL { get; set; }

        public BadgeData()
        {
        }
    }
}
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Badges;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Managers
{
    public class UserDataManager : BasePersistentManager<UserDataManager>
    {
        [Header("UI User Information")] [SerializeField]
        private TMP_Text usernameText;

        [SerializeField] private Image profilePhoto;
        [SerializeField] private List<GameObject> badges;
        [SerializeField] private GameObject badgePrefab;
        [SerializeField] private GameObject badgesParent;

        [Header("UI Change Username")] [SerializeField]
        private AlertPanel alertPanel;

        private FirebaseFirestore db;
        private ListenerRegistration usernameListener;
        private ListenerRegistration profilePhotoListener;
        private ListenerRegistration badgesListener;

        private async void Start()
        {
            try
            {
                await InitializeFirestore();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private async Task InitializeFirestore()
        {
            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
            }
        }

        public async Task CreatePlayerProfileAsync(string email, string username)
        {
            UserData newUser = new UserData
            {
                Username = username,
                ProfilePhoto = FromTextureToStringConverter(profilePhoto.sprite.texture),
                Badges = new List<BadgeData>(),
                MaxOwnGameScore = 0,
            };

            DocumentReference userRef = db.Collection("Users").Document(email);

            await userRef.SetAsync(newUser).ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError(task.Exception);
                }
            });
        }

        public async Task<bool> IsUserInFirestore(string uid)
        {
            DocumentReference docRef = db.Collection("Users").Document(uid);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        public void InitializeListeners(string uid)
        {
            DocumentReference usernameRef = db.Collection("Users").Document(uid);

            usernameListener = usernameRef.Listen(snapshot =>
            {
                if (snapshot.Exists)
                {
                    string newUsername = snapshot.GetValue<string>("Username");
                    usernameText.text = newUsername;
                }
            });
            profilePhotoListener = usernameRef.Listen(snapshot =>
            {
                if (snapshot.Exists)
                {
                    string newProfilePhoto = snapshot.GetValue<string>("ProfilePhoto");
                    Texture2D newTexture = FromStringToTextureConverter(newProfilePhoto, profilePhoto.sprite.texture);
                    Sprite imageSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height),
                        new Vector2(0.5f, 0.5f));
                    profilePhoto.sprite = imageSprite;
                }
            });
            badgesListener = usernameRef.Listen(async snapshot =>
            {
                if (snapshot.Exists)
                {
                    foreach (GameObject badge in badges) Destroy(badge);
                    badges.Clear();
                    
                    List<BadgeData> badgesFromFirestore = snapshot.GetValue<List<BadgeData>>("Badges");
                    foreach (BadgeData t in badgesFromFirestore)
                    {
                        GameObject newBadget = Instantiate(badgePrefab, badgesParent.transform);
                        newBadget.TryGetComponent(out Badge badge);
                        badge.UpdateBadgeInfo(t.BadgeName);
                        /////////////////////////////////////////////////////////////////
                        badge.BadgeImage.sprite = await DownloadSprite(t.BadgeImgURL);
                        
                        badges.Add(newBadget);

                        // TODO: Change with the URL
                        //badges[i].BadgeImage.sprite = await DownloadSprite(badgesFromFirestore[i].BadgeName);
                    }
                }
            });
        }

        private async Task<Sprite> DownloadSprite(string url)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        private Texture2D FromStringToTextureConverter(string texture, Texture2D result)
        {
            byte[] imageBytes = Convert.FromBase64String(texture);
            result.LoadImage(imageBytes);
            return result;
        }

        public async void SetProfilePhotoInFirestore(string newProfilePhoto)
        {
            DocumentReference profilePhotoRef = db.Collection("Users").Document(AuthManager.Instance.Auth.CurrentUser.Email);
            await profilePhotoRef.UpdateAsync("ProfilePhoto", FromTextureToStringConverter(profilePhoto.sprite.texture));
        }

        public void ChangeUsernameButton()
        {
            alertPanel.ShowAlert("Change Username", "Type a new username!", () => SetUsernameInFirestore(alertPanel.InputField.text));
        }

        public async void SetBadgeInFirestore(BadgeData badgeData)
        {
            DocumentReference badgesRef = db.Collection("Users").Document(AuthManager.Instance.Auth.CurrentUser.Email);

            await db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(badgesRef);

                if (snapshot.Exists)
                {
                    List<BadgeData> badges = new List<BadgeData>();
                    badges = snapshot.GetValue<List<BadgeData>>("Badges");
                    badges.Add(badgeData);
                    transaction.Update(badgesRef, "Badges", badges);
                }
            });
        }

        public async void SetMaxScoreInFirestore(int maxOwnGameScore)
        {
            DocumentReference maxScoreRef = db.Collection("Users").Document(AuthManager.Instance.Auth.CurrentUser.Email);
            await maxScoreRef.UpdateAsync("MaxOwnGameScore", maxOwnGameScore);
        }

        public async void SetUsernameInFirestore(string newUsername)
        {
            DocumentReference maxScoreRef = db.Collection("Users").Document(AuthManager.Instance.Auth.CurrentUser.Email);
            await maxScoreRef.UpdateAsync("Username", newUsername);
        }

        private void OnDestroy()
        {
            usernameListener?.Stop();
            profilePhotoListener?.Stop();
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
        [FirestoreProperty]
        public int BadgeID { get; set; }
        
        [FirestoreProperty]
        public string BadgeName { get; set; }

        [FirestoreProperty]
        public string BadgeImgURL { get; set; }


        public BadgeData()
        {
        }
    }
}
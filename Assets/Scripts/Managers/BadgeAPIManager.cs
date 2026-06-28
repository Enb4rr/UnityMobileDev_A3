using Managers;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BadgeAPIManager : MonoBehaviour
{
    private Dictionary<int, BadgeData> badgeCollection = new Dictionary<int, BadgeData>();


    public void TriggerGetBAdges()
    {
        StartCoroutine(GetBadges());      
        
    }
    private IEnumerator GetBadges()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://rickandmortyapi.com/api/character/1,2,3,4,5,6,7,8,9,10");
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }

        List<BadgeMapper> badges = JsonConvert.DeserializeObject<List<BadgeMapper>>(www.downloadHandler.text);

        foreach (BadgeMapper item in badges)
        {
            BadgeData badgeData = new BadgeData
            {
                BadgeID = item.Id,
                BadgeName = item.Name,
                BadgeImgURL = item.ImgURL
            };
            Debug.Log(badgeData.BadgeImgURL);

            badgeCollection.Add(badgeData.BadgeID, badgeData);

        }
        UserDataManager.Instance.SetBadgeInFirestore(badgeCollection[1]);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    [Serializable]
    public class BadgeMapper
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("image")]
        public string ImgURL { get; set; }
    }
}

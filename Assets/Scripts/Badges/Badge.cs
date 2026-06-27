using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Badges
{
    public class Badge : MonoBehaviour
    {
        [field: SerializeField] public Image BadgeImage { get; set; }
        [field: SerializeField] public TMP_Text BadgeTitle { get; set; }

        public void UpdateBadgeInfo(string title, Sprite sprite = null)
        {
            //BadgeSprite = sprite;
            BadgeTitle.text = title;
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "WinnerOfTheDay", menuName = "Scriptable Objects/WinnerOfTheDay")]
public class WinnerOfTheDay : ScriptableObject
{
    public string email;
    public string imageURL;
    public bool viewedWinNotification;
}

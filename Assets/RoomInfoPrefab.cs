using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class RoomInfoPrefab : MonoBehaviour
{
    public Text roomName;
    public Button joinButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRoomName(string name)
    {
        roomName.text = name;
    }

    public void SetJoinEnabled(bool enabled)
    {
        joinButton.interactable = enabled;
    }

    public void OnJoinButtonClicked()
    {
        LobbyManager.instance.JoinRoom(roomName.text);
    }
}

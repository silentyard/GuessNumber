using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class CreateRoomPanel : MonoBehaviour
{
    public InputField roomName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomName.text, new RoomOptions { MaxPlayers = 2 });
    }
}

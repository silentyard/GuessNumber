using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Transform roomListPanel;
    public CreateRoomPanel createRoomPanel;

    List<RoomInfoPrefab> rooms;

    public GameObject roomInfoPrefab;

    public static LobbyManager instance;
    // Start is called before the first frame update
    void Start()
    {
        rooms = new List<RoomInfoPrefab>();
        instance = this;

        StartCoroutine(Initialization());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Initialization()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateRoomList(Launcher.roomInfo);
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // Delete current rooms
        foreach (RoomInfoPrefab room in rooms)
            Destroy(room.gameObject);

        rooms.Clear();

        // create new lists
        foreach(RoomInfo room in roomList)
        {
            if (room.PlayerCount == 0)
                continue;

            RoomInfoPrefab newRoom = Instantiate(roomInfoPrefab).GetComponent<RoomInfoPrefab>();
            newRoom.transform.SetParent(roomListPanel);

            newRoom.SetRoomName(room.Name);
            newRoom.SetJoinEnabled(room.PlayerCount <= 1);

            rooms.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.LogFormat("OnRoomListUpdate(): {0} room exist", roomList.Count);
        UpdateRoomList(roomList);
    }*/

    public void OnCreateButtonClicked()
    {
        createRoomPanel.CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError(message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            PhotonNetwork.LoadLevel("Room");
    }

}

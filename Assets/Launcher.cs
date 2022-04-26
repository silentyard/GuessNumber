using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region public fields

    public static List<RoomInfo> roomInfo = new List<RoomInfo>();
    public static Launcher instance;

    #endregion

    #region Private Fields
    bool isConnecting;
    string gameVersion = "1";

    [SerializeField]
    private byte maxPlayerPerRoom = 4;

    [SerializeField]
    private GameObject controlPanel, progessLabel;

    #endregion

    #region MonoBehavior callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        progessLabel.SetActive(false);
        controlPanel.SetActive(true);

    }
    #endregion


    #region public methods
    public void Connect()
    {
        controlPanel.SetActive(false);
        progessLabel.SetActive(true);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    #endregion

    #region PunCallbacks callbacks
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called");
        PhotonNetwork.JoinLobby();
    }
   

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby() called");
        SceneManager.LoadScene("Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("OnJoinRoomFailed() was called, create a new room");
        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayerPerRoom});
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomInfo = roomList;
        if(LobbyManager.instance)
            LobbyManager.instance.UpdateRoomList(roomInfo);
    }


    #endregion
}

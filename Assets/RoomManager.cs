using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public Text player1Name, player2Name;
    public Button startButton, readyButton;
    public Image readySign;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;

        StartCoroutine(UpdateUI());

        if(PhotonNetwork.IsMasterClient)
        {
            //set isPlayerReady to false
            Hashtable ready = new Hashtable();
            ready.Add("isPlayerReady", false);
            PhotonNetwork.CurrentRoom.SetCustomProperties(ready);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom() was called");
    }

    public void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("try to load a level but not a master client");
            return;
        }
        Debug.LogFormat("Loading arena");

        PhotonNetwork.LoadLevel("Game");
    }

    IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(0.1f);  //wait for new player's synchonization

        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            player1Name.text = PhotonNetwork.LocalPlayer.NickName;
            player2Name.text = "";
            startButton.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(false);

            startButton.interactable = false;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                player1Name.text = PhotonNetwork.LocalPlayer.NickName;
                player2Name.text = PhotonNetwork.PlayerListOthers[0].NickName;
                startButton.gameObject.SetActive(true);
                readyButton.gameObject.SetActive(false);
            }
            else
            {
                player1Name.text = PhotonNetwork.PlayerListOthers[0].NickName;
                player2Name.text = PhotonNetwork.LocalPlayer.NickName;
                startButton.gameObject.SetActive(false);
                readyButton.gameObject.SetActive(true);
            }
        }
    }

    public void OnReadyButtonClicked()
    {
        Hashtable ready = new Hashtable();
        ready.Add("isPlayerReady", !(bool)PhotonNetwork.CurrentRoom.CustomProperties["isPlayerReady"]);
        PhotonNetwork.CurrentRoom.SetCustomProperties(ready);

        readySign.gameObject.SetActive((bool)ready["isPlayerReady"]);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("{0} enters the room", newPlayer.NickName);

        StartCoroutine(UpdateUI());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("{0} disconnects", otherPlayer.NickName);

        //set isPlayerReady to false
        Hashtable ready = new Hashtable();
        ready.Add("isPlayerReady", false);
        PhotonNetwork.CurrentRoom.SetCustomProperties(ready);
 
        startButton.interactable = false;
        StartCoroutine(UpdateUI());
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (propertiesThatChanged.ContainsKey("isPlayerReady"))
        {
            startButton.interactable = (bool)propertiesThatChanged["isPlayerReady"];
            readySign.gameObject.SetActive(startButton.interactable);
        }
    }
}

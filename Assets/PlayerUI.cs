using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using DG.Tweening;

public class PlayerUI : MonoBehaviourPunCallbacks
{
    public PlayerDeck playerDeck;
    public static PlayerUI instance;

    public InputField guessNumber;
    public GameObject continuePanel;

    public Text dialogText;
    public Text scoreText;

    public Button leaveButton;

    string masterPlayerName;
    string notMasterPlayerName;

    //public PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        if (PhotonNetwork.IsMasterClient)
        {
            masterPlayerName = PhotonNetwork.LocalPlayer.NickName;
            notMasterPlayerName = PhotonNetwork.PlayerListOthers[0].NickName;
        }
        else
        {
            masterPlayerName = PhotonNetwork.PlayerListOthers[0].NickName;
            notMasterPlayerName = PhotonNetwork.LocalPlayer.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGuessButtonClicked()
    {
        playerDeck.guessConfirm = true;
    }

    public void OnContinueYesClicked()
    {
        playerDeck.continueToGuess = 1;
        continuePanel.SetActive(false);
    }

    public void OnContinueNoClicked()
    {
        playerDeck.continueToGuess = 2;
        continuePanel.SetActive(false);
    }

    public void ShowDialog(string text)
    {
        dialogText.text = "";
        dialogText.DOText(text, 1.0f);
    }

    [PunRPC]
    void BroadcastDialog(string message)
    {
        ShowDialog(message);
    }

    [PunRPC]
    public void UpdateScore(int my, int oppo)
    {
        if(PhotonNetwork.IsMasterClient)
            scoreText.text = string.Format("{0} {1} : {2} {3}", masterPlayerName, my, oppo, notMasterPlayerName);
        else
            scoreText.text = string.Format("{0} {1} : {2} {3}", masterPlayerName, oppo, my, notMasterPlayerName);

    }
}

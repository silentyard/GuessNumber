using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using DG.Tweening;

using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;


public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance;

    public bool masterTurn = true;
    public int initialCard = 1;
    public int goalPoints = 1;

    public float phaseTransitionTime = 1.0f;

    public GameObject playerDeck;

    public float dialogTransitionTime = 3f;

    int myScore = 0;
    int opponentsScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //if (PhotonNetwork.IsMasterClient)
        //{
            Hashtable hash = new Hashtable();
            hash.Add("initCard", initialCard);
            hash.Add("guessPhase", false);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        //}

        playerDeck = PhotonNetwork.Instantiate("PlayerDeck", Vector3.zero, Quaternion.identity);
        PlayerUI.instance.playerDeck = playerDeck.GetComponent<PlayerDeck>();

        //show dialog
        if (PhotonNetwork.IsMasterClient)
            PlayerUI.instance.ShowDialog("It's your turn, please pick a card...");
        else
            PlayerUI.instance.ShowDialog("Wait for the opponent to pick a card...");

        PlayerUI.instance.UpdateScore(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeaveGame()
    {
        SceneManager.LoadScene("Room");
    }

    IEnumerator SwitchPhase()
    {
        PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.All, "Entering Guessing Phase...");
        yield return new WaitForSeconds(2);

        //set player's unpicked card number
        Hashtable hash = new Hashtable();
        hash.Add("RestCards", initialCard);
        PhotonNetwork.SetPlayerCustomProperties(hash);

        // move player deck
        playerDeck.GetComponent<PlayerDeck>().MoveToGuessPosition();

        // move unpicked cards
        GameObject blueCardDeck = GameObject.Find("Blue Cards");
        GameObject redCardDeck = GameObject.Find("Red Cards");

        if (PhotonNetwork.IsMasterClient)
        {
            blueCardDeck.GetComponent<CardDeck>().CollectUnchosenCards();
            redCardDeck.GetComponent<CardDeck>().CollectUnchosenCards();

            StartCoroutine(playerDeck.GetComponent<PlayerDeck>().GuessPhaseMovement());
        }
        else
            PlayerUI.instance.ShowDialog("Opponent's turn...");
    }   
    
    [PunRPC]
    void GuessPhaseSwitchTurn()
    {
        StartCoroutine(playerDeck.GetComponent<PlayerDeck>().GuessPhaseMovement());
    }

    [PunRPC]
    void MinusUnguessedCard()
    {
        int rest = (int)PhotonNetwork.LocalPlayer.CustomProperties["RestCards"];
        Hashtable hash = new Hashtable();
        hash.Add("RestCards", rest - 1);

        PhotonNetwork.SetPlayerCustomProperties(hash);
    }

    [PunRPC]
    void GameOver(Player winner)
    {
        PlayerUI.instance.leaveButton.gameObject.SetActive(true);
        PlayerUI.instance.ShowDialog(string.Format("{0} wins, please press the leave button", winner.NickName));
    }

    public IEnumerator GuessCard(Card pickedCard, string guessNumber)
    {
        string broadcastDialog = PhotonNetwork.LocalPlayer.NickName + " guessed the card is " + guessNumber + "...";
        PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.All, broadcastDialog);

        yield return new WaitForSeconds(dialogTransitionTime);

        if (pickedCard.number == int.Parse(guessNumber))
        {
            //guess succeeded
            PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.All, "Correct!");

            //position and open the guessed card
            pickedCard.openned = true;
            pickedCard.transform.DORotate(pickedCard.transform.eulerAngles + new Vector3(0, 180, 0), pickedCard.transitionTime);

            yield return new WaitForSeconds(dialogTransitionTime / 2);

            //add score
            Hashtable hash = new Hashtable();
            hash.Add("score", myScore + 1);
            PhotonNetwork.SetPlayerCustomProperties(hash);

            //check gameOver (will run before player custom properties being updated)
            if (myScore + 1 >= goalPoints)
            {
                photonView.RPC("GameOver", RpcTarget.All, PhotonNetwork.LocalPlayer);
                yield break;
            }

            //minus and check if there are any cards left for guessing
            photonView.RPC("MinusUnguessedCard", RpcTarget.Others);

            int opponentRestCards = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["RestCards"];

            if (opponentRestCards > 1)
                //choose to continue guess or not?
                StartCoroutine(playerDeck.GetComponent<PlayerDeck>().ContinueToGuessOrNot());
            else
            {
                //no cards left to guess, position the cards and switch turn
                playerDeck.GetComponent<PlayerDeck>().PositionCards();
                playerDeck.GetComponent<PlayerDeck>().IncreaseUnguessedCards();
                PlayerUI.instance.ShowDialog("No cards left to guess, switch to opponent's turn...");
                photonView.RPC("GuessPhaseSwitchTurn", RpcTarget.Others);
            }

        }
        else
        {
            //guess failed
            PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.All, "Wrong!");

            PlayerDeck temp = playerDeck.GetComponent<PlayerDeck>();

            //position the drawed card and flip to opponent
            temp.guessPhaseHoldingCard.openned = true;
            temp.PositionCards();

            //reposition the picked card
            pickedCard.moveTo(pickedCard.transform.position - new Vector3(0, temp.floatCardOffsetY, 0), pickedCard.transform.eulerAngles);

            yield return new WaitForSeconds(dialogTransitionTime);

            //switch turn
            PlayerUI.instance.ShowDialog("Opponent's turn...");
            photonView.RPC("GuessPhaseSwitchTurn", RpcTarget.Others);
        }

    }

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("try to load a level but not a master client");
        }
        Debug.LogFormat("Loading arena");



        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("{0} enters the room", newPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("is master client: {0}", PhotonNetwork.IsMasterClient);

            //LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("{0} disconnects", otherPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
           //LoadArena();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(masterTurn);
        }
        else
        {
            this.masterTurn = (bool)stream.ReceiveNext();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("guessPhase"))
        {
            if((bool)PhotonNetwork.CurrentRoom.CustomProperties["guessPhase"] == true)
            {
                StartCoroutine(SwitchPhase());
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("score"))
        {
            if (targetPlayer == PhotonNetwork.LocalPlayer)
                myScore++;
            else
                opponentsScore++;

            PlayerUI.instance.UpdateScore(myScore, opponentsScore);
        }
    }
}

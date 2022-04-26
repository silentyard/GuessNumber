using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using DG.Tweening;

public class PlayerDeck : MonoBehaviourPunCallbacks
{
    Player opponent;
    public float xRange = 12;
    public float posZ = 60;
    public float posY = 7.5f;

    public float floatCardOffsetY = 3;

    public float moveToGuessPosition_ZRatio = 0.7f;

    public bool guessConfirm = false;
    public byte continueToGuess = 0;
    public Card guessPhaseHoldingCard = null;

    [SerializeField]
    int maxCards;

    List<Card> cards = new List<Card>();

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
            transform.position = new Vector3(0, posY, posZ * (PhotonNetwork.IsMasterClient ? -1 : 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        if(!((PhotonNetwork.IsMasterClient && GameManager.instance.masterTurn) || (!PhotonNetwork.IsMasterClient && !GameManager.instance.masterTurn))){
            return;
        }

        if(cards.Count < maxCards)
        {
            ChooseCard();
            bool switchPhase = CheckPhase();
            return;
        }

        //Guess Phase
        ////use coroutine to handle guess phase
    }

    void ChooseCard()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.tag == "card")
                {
                    Card chosenCard = hitObject.GetComponent<Card>();
                    if(chosenCard.owner == null)
                    {
                        chosenCard.owner = PhotonNetwork.LocalPlayer;
                        //hitObject.transform.SetParent(gameObject.transform);

                        AddCard(chosenCard);
                        PositionCards();

                        StartCoroutine(SwitchTurn());

                        Debug.Log("current card: " + cards.Count);

                    }

                }
            }
        }
    }

    public IEnumerator GuessPhaseMovement()
    {
        yield return GuessPhaseDrawCard();

        //check if opponent has cards left for guessing
        int opponentRestCards = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["RestCards"];

        if(opponentRestCards > 0)
        {
            yield return GuessPhasePickAndGuess();
        }
        else
        {
            //no cards to guess, position the cards, increase unguessed card number, and switch turn
            PlayerUI.instance.ShowDialog("No cards for you to guess, switch to opponent's turn.");
            PositionCards();
            IncreaseUnguessedCards();
            GameManager.instance.photonView.RPC("GuessPhaseSwitchTurn", RpcTarget.Others);
        }
    }

    IEnumerator GuessPhaseDrawCard()
    {
        PlayerUI.instance.ShowDialog("It's your turn. Please pick a card...");
        guessPhaseHoldingCard = null;
        while (!guessPhaseHoldingCard)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.tag == "card")
                    {
                        Card chosenCard = hitObject.GetComponent<Card>();
                        if (chosenCard.owner == null)
                        {
                            chosenCard.owner = PhotonNetwork.LocalPlayer;

                            AddCard(chosenCard);
                            Vector3 rotation = new Vector3(0, PhotonNetwork.IsMasterClient ? 0 : 180, 0);
                            chosenCard.moveTo(Vector3.zero, rotation);

                            guessPhaseHoldingCard = chosenCard;
                        }

                    }
                }
            }
            yield return null;
        }
        
    }

    IEnumerator GuessPhasePickAndGuess()
    {
        PlayerUI.instance.ShowDialog("Choose a card from the opponent and guess.");

        Card picked = null;
        guessConfirm = false;

        while (!guessConfirm)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.tag == "card")
                    {
                        Card hitObjectCard = hitObject.GetComponent<Card>();
                        if (hitObjectCard.owner == PhotonNetwork.PlayerListOthers[0] && hitObjectCard.openned == false)
                        {
                            if (picked != hitObjectCard) 
                            { 
                                if(picked != null)
                                    picked.moveTo(picked.transform.position - new Vector3(0, floatCardOffsetY, 0), picked.transform.eulerAngles);

                                picked = hitObjectCard;
                                Vector3 floatPosition = picked.transform.position + new Vector3(0, floatCardOffsetY, 0);
                                picked.moveTo(floatPosition, picked.transform.eulerAngles);
                            }
                        }
                    }
                }
            }
            yield return null;
        }
        StartCoroutine(GameManager.instance.GuessCard(picked, PlayerUI.instance.guessNumber.text));

        Debug.LogFormat("Ground Truth : {0} {1}", picked.isBlue, picked.number);
    }

    public IEnumerator ContinueToGuessOrNot()
    {
        // handle UI
        PlayerUI.instance.continuePanel.SetActive(true);

        continueToGuess = 0;
        while(continueToGuess == 0)
        {
            yield return null;
        }

        if(continueToGuess == 1)
        {   //if choose to continue
            PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.Others, PhotonNetwork.LocalPlayer.NickName + " chooses to continue...");
            yield return GuessPhasePickAndGuess();
        }
        else
        {   //if choose not to continue
            PositionCards();
            IncreaseUnguessedCards();
            PlayerUI.instance.ShowDialog("Opponent's turn...");
            GameManager.instance.photonView.RPC("GuessPhaseSwitchTurn", RpcTarget.Others);
        }
    }

    public void IncreaseUnguessedCards()
    {
        int rest = (int)PhotonNetwork.LocalPlayer.CustomProperties["RestCards"];
        rest++;

        Hashtable hash = new Hashtable();
        hash.Add("RestCards", rest);
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }

    IEnumerator SwitchTurn()
    {
        PlayerUI.instance.ShowDialog("Wait for the opponent to pick a card...");
        PlayerUI.instance.photonView.RPC("BroadcastDialog", RpcTarget.Others, "It's your turn, please pick a card...");

        opponent = PhotonNetwork.PlayerListOthers[0];
        GameManager.instance.masterTurn = !GameManager.instance.masterTurn;
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.photonView.TransferOwnership(opponent);


    }

    void AddCard(Card chosenCard)
    {
        int left = 0;
        int right = cards.Count;
        int middle = (left + right) / 2;
        while(left < right)
        {
            middle = (left + right) / 2;
            if (cards[middle].number > chosenCard.number)
                right = middle;
            else if (cards[middle].number < chosenCard.number)
                left = middle + 1;
            else
            {
                if (chosenCard.isBlue)
                    cards.Insert(middle, chosenCard);
                else
                    cards.Insert(middle + 1, chosenCard);
                return;
            }
        }
        cards.Insert(left, chosenCard);
    }

    public void PositionCards()
    {
        // get list of positions
        List<Vector3> cardPositions = CalculateCardPosition();
        Vector3 rotation = new Vector3(0, PhotonNetwork.IsMasterClient ? 0 : 180, 0);

        for(int i = 0; i < cards.Count; i++)
        {
            //when positioning, consider the card is openned or not (used also in the guessing phase)
            cards[i].moveTo(cardPositions[i], cards[i].openned ? rotation + new Vector3(0, 180, 0) : rotation);
        }
    }

    List<Vector3> CalculateCardPosition()
    {
        int n = cards.Count;
        float leftmostX = -xRange * (n % 2 == 0 ? n / 2 - 0.5f : n / 2);
        int mirror = PhotonNetwork.IsMasterClient ? 1 : -1;

        List<Vector3> output = new List<Vector3>();
        for (int i = 0; i < n; i++)
            output.Add(new Vector3((leftmostX + i * xRange) * mirror, transform.position.y, transform.position.z));

        return output;
    }

    bool CheckPhase()
    {
        if (cards.Count == maxCards && !PhotonNetwork.IsMasterClient)
        {
            Hashtable hash = new Hashtable();
            hash.Add("guessPhase", true);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            Debug.LogFormat("Sent by {0}", PhotonNetwork.LocalPlayer);
            return true;
        }
        else
            return false;
    }

    public void MoveToGuessPosition()
    {
        //move the card deck (for furthur calculation of card positions)
        Vector3 pos = transform.position;
        transform.DOMove(new Vector3(pos.x, pos.y, pos.z * moveToGuessPosition_ZRatio), GameManager.instance.phaseTransitionTime);

        //move the cards
        for (int i = 0; i < cards.Count; i++)
        {
            pos = cards[i].transform.position;
            cards[i].moveTo(new Vector3(pos.x, pos.y, pos.z * moveToGuessPosition_ZRatio), cards[i].transform.eulerAngles);
        }

        //move the camera
        Vector3 camera_pos = Camera.main.transform.position;
        Camera.main.transform.DOMove(new Vector3(camera_pos.x, camera_pos.y, camera_pos.z * moveToGuessPosition_ZRatio), GameManager.instance.phaseTransitionTime);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!photonView.IsMine)
            return;

        Debug.LogFormat("From {0}", PhotonNetwork.LocalPlayer);
        if (propertiesThatChanged.ContainsKey("initCard"))
            maxCards = (int)PhotonNetwork.CurrentRoom.CustomProperties["initCard"];

        foreach (DictionaryEntry entry in propertiesThatChanged)
            Debug.LogFormat("{0} | {1}", entry.Key, entry.Value);

        Debug.Log("maxCards" + maxCards);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using DG.Tweening;

public class Card : MonoBehaviour, IPunObservable
{
    PhotonView photonView;
    public Ease ease;
    public float transitionTime = 1.0f;

    public Player owner = null;
    public bool openned = false; 

    public bool isBlue;
    public int number;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void moveTo(Vector3 position, Vector3 rotation)
    {
        if (photonView.IsMine == false)
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        MovePosition(position);
        Rotate(rotation);
    }

    void MovePosition(Vector3 position)
    {
        transform.DOMove(position, transitionTime).SetEase(ease);
    }

    void Rotate(Vector3 rotation)
    {
        transform.DORotate(rotation, transitionTime).SetEase(ease);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(owner);
            stream.SendNext(openned);
        }
        else
        {
            owner = (Player)stream.ReceiveNext();
            openned = (bool)stream.ReceiveNext();
        }
    }

}

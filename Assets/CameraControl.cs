using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            transform.position -= new Vector3(0, 0, 2 * transform.position.z);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180, transform.eulerAngles.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

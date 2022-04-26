using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class PlayerNameInputField : MonoBehaviour
{
    #region Private Constants
    const string playerNamePrefKey = "PlayerName";
    #endregion


    #region MonoBehavior callbacks
    // Start is called before the first frame update
    void Start()
    {
        string defaultName = string.Empty;
        InputField inputField = this.GetComponent<InputField>();

        if(inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }
        PhotonNetwork.NickName = defaultName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region public functions
    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player name is null or empty");
            return;
        }
        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
    #endregion
}

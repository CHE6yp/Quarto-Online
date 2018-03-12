using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Params : MonoBehaviour {


    public string playerName;
    public InputField playerNameInputField;
    public bool ai = false;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
        playerName = PlayerPrefs.GetString("playerName", "PlayerName");
        playerNameInputField.text = playerName;
	}

    public void SetName(string newName)
    {
        PlayerPrefs.SetString("playerName", newName);
        playerName = newName;
    }

    public void TurnAiOn()
    {
        ai = true;
    }

	
}

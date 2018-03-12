using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;


public class MenuController : MonoBehaviour {

	public static MenuController instance;
    public OnlineScene.MatchManager matchManagerOnline;
    public OfflineScene.MatchManager matchManagerOffline;
    
    public GameObject mainCanvas;
    public GameObject eventSystem;
    public NetworkManager netManager;


	
    public int currentCanvas = 0;
    public int currentSheet = 0;
    public int previousCanvas = 0;

    public Canvas[] activeSheet;
    
    public Canvas[] menuSheet = new Canvas[0];
    public GameObject menuCanvas;
    public Canvas[] matchSheet = new Canvas[0];
    public GameObject matchCanvas;

    //multiplayer
    public Text matchName;
    public Text matchPass;

    List<GameObject> matchesFound = new List<GameObject>();

    public GameObject foundMatchPrefab;
    public GameObject canvasFoundGames;

    //found games canvas shienanigans
    public Transform canvas7;
    public bool fgcDragged;
    //public Transform canvas0;


    //options
    public Dropdown scrResDrop;
    public Toggle fullscrToggle;
    public Dropdown qualityDrop;
    public Dropdown monitorDrop;

    //ingame
    public Text turnText;
    public Text phaseText;

    public GameObject chatButton;




    // Use this for initialization
    void Start () {
        instance = this;

        activeSheet = menuSheet;

        #region options
        //screenRes options menu
        List<string> resStr = new List<string>();
        int resX = 0;
        int dropval = 0 ;
        foreach (var res in Screen.resolutions)
        {
            resStr.Add(res.width + "x" + res.height);
            if ((res.width == Screen.currentResolution.width) && (res.height == Screen.currentResolution.height))
            {
                dropval = resX;
            }
            resX++;
        }
		
		scrResDrop.ClearOptions();
		scrResDrop.AddOptions(resStr);
		scrResDrop.value = dropval;
        #endregion

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(eventSystem);
        DontDestroyOnLoad(mainCanvas);


        

        SceneManager.LoadScene("menu");

        


    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            SwitchCanvasBackRoot();
	}

    public void SwitchCanvas (int id)
    {
        previousCanvas = currentCanvas;
        activeSheet[currentCanvas].gameObject.SetActive(false);
        activeSheet[id].gameObject.SetActive(true);
        
        currentCanvas = id;
        
    }

    public void SwitchCanvasBackRoot()
    {
        if (currentCanvas == 0)
            return;
        if (currentCanvas == 1 || currentCanvas == 5 || currentCanvas == 4)
            SwitchCanvas(0);
        else if (currentCanvas == 2 || currentCanvas == 3)
            SwitchCanvas(1);
        else if (currentCanvas == 6|| currentCanvas == 7 || currentCanvas == 8)
            SwitchCanvas(3);

    }

    public void SwitchSheet ()
    {
        SwitchCanvas(0);
        if (currentSheet == 0)
        {
            activeSheet = matchSheet;
            
            menuCanvas.SetActive(false);
            matchCanvas.SetActive(true);
            currentSheet = 1;
        }
        else
        {
            activeSheet = menuSheet;
            menuCanvas.SetActive(true);
            matchCanvas.SetActive(false);
            currentSheet = 0;
        }
        previousCanvas = 0;
        
    }

    public void StartHotseat()
    {
        SceneManager.LoadScene(3);
        SwitchSheet();
    }

    public void DragFoundGamesCanvas()
    {
        if (!fgcDragged)
        {
            canvasFoundGames.transform.parent = null;
            fgcDragged = true;
        }
        else
        {
            canvasFoundGames.transform.parent = canvas7;
            fgcDragged = false;
        }

    }

    public void EnableMatchMaking()
    {
        netManager.StartMatchMaker();
        SwitchCanvas(3);
    }

    public void HostGame()
    {
        //надо добавить вместо первого пустого поля, когда будут пароли
        //matchPass.text
        turnText.text = "Waiting for players";
        phaseText.text = "";
        netManager.matchMaker.CreateMatch(matchName.text, 2, true, "", "", "", 0, 0, netManager.OnMatchCreate);
        SwitchSheet();
        SwitchChatButton();
    }

    public void GoToFindGames()
    {
        //DragFoundGamesCanvas();
        SwitchCanvas(7);
        StartCoroutine(FindGamesCour());
    }
   

    public void FindGame()
    {
        netManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, netManager.OnMatchList);
        //SwitchCanvas(7);
        ShowGames();

    }

    public IEnumerator FindGamesCour()
    {
        yield return new WaitForSeconds(1);
        FindGame();
        yield return new WaitForSeconds(1);
        FindGame();
    }

    public void ShowGames()
    {
        foreach (Transform child in canvasFoundGames.transform)
        {
            Destroy(child.gameObject);
        }

        int drop = -25;
        if (netManager.matches != null)
            for (int i = 0; i < netManager.matches.Count; i++)
            {
                var match = netManager.matches[i];
                if (match.currentSize != 2)
                {
                    GameObject foundMatchNew = GameObject.Instantiate(foundMatchPrefab, canvasFoundGames.transform);
                    matchesFound.Add(foundMatchNew);
                    //foundMatchNew.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { JoinGame(match.name, match.networkId); });
                    foundMatchNew.GetComponent<Button>().onClick.AddListener(delegate { JoinGame(match.name, match.networkId); });


                    //position 
                    //foundMatchNew.GetComponent<RectTransform>().offsetMin = new Vector2(5, foundMatchNew.GetComponent<RectTransform>().offsetMin.y);
                    //foundMatchNew.GetComponent<RectTransform>().offsetMax = new Vector2(-5, foundMatchNew.GetComponent<RectTransform>().offsetMax.y);
                    //foundMatchNew.GetComponent<RectTransform>().anchoredPosition = new Vector3(foundMatchNew.GetComponent<RectTransform>().anchoredPosition.x, drop);
                    foundMatchNew.GetComponent<RectTransform>().anchoredPosition = new Vector3(-35, drop);

                    foundMatchNew.transform.GetChild(1).GetComponent<Text>().text = match.name;
                    //foundMatchNew.transform.GetChild(0).GetComponent<Text>().text = match.name;
                    drop -= 30;
                }

            }
    }

    public void Refresh()
    {
        foreach (GameObject btn in matchesFound)
        {
            GameObject.Destroy(btn);
        }
        ShowGames();

    }

    public void JoinGame(string matchName, UnityEngine.Networking.Types.NetworkID matchId )
    {
        
        netManager.matchName = matchName;
        netManager.matchMaker.JoinMatch(matchId, "", "", "", 0, 0, netManager.OnMatchJoined);
        SwitchSheet();
        SwitchChatButton();
    }

    

    public void LeaveMatch()
    {
        if (netManager.isNetworkActive)
        {
            netManager.StopHost();
            netManager.StopMatchMaker();
            SwitchChatButton();
        }
        else
        {
            SceneManager.LoadScene(1);
            FindObjectOfType<Params>().ai = false;
        }
        SwitchSheet();
    }

    public void ApplyOptions()
    {
        Screen.SetResolution(Screen.resolutions[scrResDrop.value].width, Screen.resolutions[scrResDrop.value].height, fullscrToggle.isOn);
        QualitySettings.SetQualityLevel(qualityDrop.value, true);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwitchRules()
    {

    }


    /////////
    public void SwitchChatButton()
    {
        Debug.Log("SwitchChtBtn to " + !chatButton.activeSelf);
        chatButton.SetActive(!chatButton.activeSelf);
    }

    public void Restart()
    {
        if (matchManagerOnline != null)
            matchManagerOnline.CmdRestart();
        if (matchManagerOffline != null)
            matchManagerOffline.CmdRestart();
    }

    public void SwitchCam()
    {
        if (FindObjectOfType<OfflineScene.Player>())
            FindObjectOfType<OfflineScene.Player>().SwitchCam();
        if (FindObjectOfType<OnlineScene.Player>())
            foreach (OnlineScene.Player pl in FindObjectsOfType<OnlineScene.Player>())
            {
                if (pl.isLocalPlayer)
                    pl.SwitchCam();
            }
    }

    

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnect");
        SwitchSheet();
        if (!Network.isServer)
            if (info == NetworkDisconnection.LostConnection)
            SwitchCanvas(8);
        
    }

}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace OnlineScene
{
    public class Chat : NetworkBehaviour
    {
        public Player player;
        public Text chatText;
        public InputField chatInput;
        public Button sendButton;
        public Scrollbar scrollBar;

        // Use this for initialization
        void Start()
        {
            player = GetComponent<Player>();
            
            chatText = MenuController.instance.matchCanvas.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
            chatInput = MenuController.instance.matchCanvas.transform.GetChild(1).GetChild(1).GetComponent<InputField>();
            sendButton = MenuController.instance.matchCanvas.transform.GetChild(1).GetChild(2).GetComponent<Button>();
            scrollBar = MenuController.instance.matchCanvas.transform.GetChild(1).GetChild(0).GetChild(2).GetComponent<Scrollbar>();

            if (player.isLocalPlayer)
            {
                Debug.Log(player.isLocalPlayer);
                sendButton.onClick.AddListener(delegate { SendChatMessage(); });
                chatText.text = "";
            }
            Debug.Log(player.isLocalPlayer+" 2");
            
        }

        public void SendChatMessage()
        {
            Debug.Log("SndMssg");
            if (chatInput.text != "")
            {
                CmdSendChatMessage(player.playerName, chatInput.text);
                chatInput.text = "";
            }
        }

        [Command]
        public void CmdSendChatMessage(string playerName, string chatMessage)
        {
            

                Debug.Log("CmdSndMssg");
                RpcSendChatMessage(playerName, chatMessage);
                
            
        }

        [ClientRpc]
        public void RpcSendChatMessage(string playerName, string chatMessage)
        {

            Debug.Log("RpcSndMssg");
            chatText.text += playerName + ": " + chatMessage + "\n";
            scrollBar.value = 0;
            ChatAlert();

        }

        public void ChatAlert()
        {
            MenuController.instance.chatButton.GetComponent<Button>().Select();
        }
    }
}

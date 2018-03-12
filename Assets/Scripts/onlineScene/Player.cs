using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OnlineScene;

namespace OnlineScene
{
    public class Player : NetworkBehaviour
    {

        
        public Camera cam;

        public AudioListener audioL;

        [SyncVar]
        public string playerName = "Player";

        public bool myTurn;
        public bool endGame;

      

        bool camOrt = true;





        void Start()
        {

            if (!isLocalPlayer)
            {
                cam.enabled = false;
                audioL.enabled = false;

            }
            else
            {
                CmdSwitchName(FindObjectOfType<Params>().playerName);
            }

            
            

            transform.position = new Vector3(0, 0, 5);
            transform.eulerAngles = new Vector3(0, -45, 0);
        }

        // Update is called once per frame
        void Update()
        {

            if (!isLocalPlayer)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                Click();
            }

        }

        public override void OnStartClient()
        {
            base.OnStartServer();

            Debug.Log("CONNECTION!");
            StartCoroutine(DelayedRegistration());
        }

        //MatchManager.singleton почемуто появляется после того как появляются игроки. 
        //Потому дабы добавить хоста, нужно откладывать регистрацию пока не создаться синглтон
        private IEnumerator DelayedRegistration()
        {
            while (MatchManager.singleton == null)
            {
                yield return null;
            }
            AddPlayer();
            yield return new WaitForSeconds(1);
            if (isLocalPlayer)
                CmdSwitchName(FindObjectOfType<Params>().playerName);
        }


        public void AddPlayer()
        {
            MatchManager.singleton.AddPlayer(this);
        }

        public void Click()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);

            if (myTurn)
            {
                 
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.tag == "Piece" && (MatchManager.singleton.gameState == MatchManager.GameState.PickPiece))
                    {
                        Piece piece = hit.collider.GetComponent<Piece>();
                        CmdSendPiece(piece.id);
                    }

                    if (hit.collider.gameObject.tag == "Place" && (MatchManager.singleton.gameState == MatchManager.GameState.PutPiece))
                    {
                        Place place = hit.collider.GetComponent<Place>();
                        if (!place.taken)
                            CmdSendPlace(place.id);
                    }

                }

            }
        }

        [Command]
        public void CmdSendPiece(int id)
        {

            Debug.Log("SEND PIECE");

            MatchManager.singleton.RpcSetPiece(id);
        }

        [Command]
        public void CmdSendPlace(int id)
        {

            Debug.Log("SEND PLACE");

            MatchManager.singleton.RpcPutPiece(id);
        }



        public void SwitchCam()
        {
            camOrt = (camOrt) ? false : true;
            if (camOrt)
            {
                transform.position = new Vector3(0, 0, 5);
                transform.Rotate(Vector3.up, -45);
                cam.orthographic = true;
                cam.orthographicSize = 7;
                cam.transform.localPosition += new Vector3(0, 0, -9);
                cam.transform.Rotate(Vector3.right, -20);
            }
            else
            {
                cam.orthographic = false;
                transform.position = new Vector3(0, 0, 0);
                transform.Rotate(Vector3.up, 45);
                cam.transform.localPosition += new Vector3(0, 0, 9);
                cam.transform.Rotate(Vector3.right, 20);

            }
        }

        [Command]
        public void CmdSwitchName(string newName)
        {
            RpcSwitchName(newName);
        }

        [ClientRpc]
        public void RpcSwitchName(string newName)
        {
            playerName = newName;
        }
    }

    
}
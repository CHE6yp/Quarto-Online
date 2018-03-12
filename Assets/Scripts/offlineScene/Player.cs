using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
namespace OfflineScene
{
    public class Player : MonoBehaviour
    {

        //public Cube cube;
        [HideInInspector]
        public Camera cam;
        
        [HideInInspector]
        public AudioListener audioL;

        //public NetworkPlayer player;

        public bool myTurn;
        public bool endGame;

        
        bool camOrt = true;



        // Use this for initialization
        void Start()
        {
            
            //camHole.projectionMatrix = camHole.projectionMatrix* Matrix4x4.Scale(new Vector3(-1, -1, 1));

            
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Click();
            }

        }


        public void Click()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);

            if (myTurn)
            {
                //if (MatchManager.singleton.gameState == MatchManager.GameState.PickPiece) 
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.tag == "Piece" && (MatchManager.singleton.gameState == MatchManager.GameState.PickPiece))
                    {
                        Piece piece = hit.collider.GetComponent<Piece>();
                        CmdSendPiece(piece);
                    }

                    if (hit.collider.gameObject.tag == "Place" && (MatchManager.singleton.gameState == MatchManager.GameState.PutPiece))
                    {
                        Place place = hit.collider.GetComponent<Place>();
                        if (!place.taken)
                            CmdSendPlace(place);
                    }

                }

            }
        }

        //[Command]
        public void CmdSendPiece(Piece piece)
        {

            Debug.Log("SEND PIECE");

            MatchManager.singleton.RpcSetPiece(piece);
        }

        //[Command]
        public void CmdSendPlace(Place place)
        {

            Debug.Log("SEND PLACE");

            MatchManager.singleton.RpcPutPiece(place);
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
                cam.transform.localPosition += new Vector3 (0, 0, -9);
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
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using OnlineScene;

namespace OnlineScene
{
    public class MatchManager : NetworkBehaviour
    {

        public static MatchManager singleton;
        public int playerCount = 0;
        public int pieceCount = 0;
        
        public Player player1;
        
        public Player player2;

        public enum GameState { WaitForPlayers, Start, PickPiece, PutPiece, EndGame }
        public GameState gameState = GameState.WaitForPlayers;

        public Piece pickedPiece;

        public Place[] places = new Place[16];
        public int[,] placeIds = new int[,] { { 0, 1, 2, 3 }, { 4, 5, 6, 7 }, { 8, 9, 10, 11 }, { 12, 13, 14, 15 },
                                            { 0, 4, 8, 12 }, {1,5,9,13 }, {2,6,10,14 }, {3,7,11,15 }, {0,5,10,15 }, {3,6,9,12 } };

        public Material placeMatEnd;
        public Material placeMatStart;

        public GameObject piecesOrigin;
        public Piece[] pieces = new Piece[16];

        public AudioSource endGameSound;
        public AudioSource pickSound;
        public AudioSource placeSound;

        // Use this for initialization
        void Start()
        {
            singleton = this;
            MenuController.instance.matchManagerOnline = this;
            //StartMatch();
        }



        //[ClientRpc]
        public void StartMatch()
        {
            //put pieces
            float x = -5.25f;
            float z = 0.7f;

            for (int i = 0; i < 16; i++)
            {

                pieces[i].transform.localPosition = new Vector3(x, 0, z);
                pieces[i].played = false;
                pieces[i].outline.enabled = false;
                //pieces[i] = pc.GetComponent<Piece>();
                x += 1.5f;
                if (i == 7)
                {
                    x = -5.25f;
                    z = -0.7f;
                }
            }

            //clear places
            for (int i = 0; i < 16; i++)
            {

                places[i].taken = false;
                places[i].piece = null;
            }

            player1.endGame = false;
            player2.endGame = false;

            for (int y = 0; y < 16; y++)
            {
                places[y].GetComponent<MeshRenderer>().material = placeMatStart;
            }

            MenuController.instance.turnText.text = player1.playerName + " turn";
            MenuController.instance.phaseText.text = "Pick Piece";

            MenuController.instance.turnText.color = new Color32(255, 255, 255, 255);
            //gameState = GameState.Start;

            Debug.Log("Start Game");

            player2.myTurn = false;
            player1.myTurn = true;
            if (player1.playerName == player2.playerName)
            {
                Debug.Log("MatchName: " + player1.playerName + "=" + player2.playerName);
                player2.playerName += " 2";
            }
            pieceCount = 0;
            MenuController.instance.turnText.text = player1.playerName + " turn";
            gameState = GameState.PickPiece;
        }

        //[ClientRpc]
        public void AddPlayer(Player player)
        {
            playerCount++;
            if (player1 == null)
            {
                player1 = player;
                Debug.Log("Player 1 added");
            }
            else
            {
                player2 = player;
                Debug.Log("Names: " + player1.playerName + "=" + player2.playerName);
                
                    
                Debug.Log("Player 2 added");
            }

            if (playerCount == 2)
            {
                StartMatch();
            }

            

        }

        public void SwitchTurn()
        {
            player1.myTurn = !player1.myTurn;
            player2.myTurn = !player2.myTurn;
            MenuController.instance.turnText.text = (player1.myTurn)? player1.playerName + " turn":player2.playerName + " turn";
            gameState = GameState.PutPiece;
        }

        [ClientRpc]
        public void RpcSetPiece(int id)
        {
            //pickedPiece = ClientScene.FindLocalObject(netId).GetComponent<Piece>();
            pickedPiece = pieces[id];
            if (!pickedPiece.played)
            {
                pickedPiece.Pick();
                MenuController.instance.phaseText.text = "Place Piece";
                pickSound.Play();
                SwitchTurn();
            }
        }


        [ClientRpc]
        public void RpcPutPiece(int id)
        {
            //Place pickedPlace = ClientScene.FindLocalObject(netId).GetComponent<Place>();
            Place pickedPlace = places[id];
            pickedPiece.transform.position = pickedPlace.transform.position + new Vector3(0, 0.75f, 0);
            pickedPlace.piece = pickedPiece;
            pickedPiece.Drop();
            pickedPiece = null;
            pieceCount++;


            MenuController.instance.phaseText.text = "Pick Piece";

            //ПРОВЕРКА ДОСКИ НА ПОБЕДУ
            CheckGame();

        }


        public void CheckGame()
        {
            //10 линий. Это можно сделать элегантнее, но я ленивая жопа и не хочу придумывать
            int shape = 0;
            int color = 0;
            int lenght = 0;
            int hole = 0;

            bool endGame = false;
            int placeSetId = -1;

            for (int x = 0; x < 10; x++)
            {
                shape = 0;
                color = 0;
                lenght = 0;
                hole = 0;
                for (int y = 0; y < 4; y++)
                {
                    if (places[placeIds[x, y]].piece != null)
                    {
                        shape += (places[placeIds[x, y]].piece.shape == Piece.Shape.Box) ? -1 : 1;
                        color += (places[placeIds[x, y]].piece.color == Piece.Color.Black) ? -1 : 1;
                        lenght += (places[placeIds[x, y]].piece.lenght == Piece.Lenght.Long) ? -1 : 1;
                        hole += (places[placeIds[x, y]].piece.hole == Piece.Hole.Empty) ? -1 : 1;
                    }
                }

                if (((shape == 4) || (shape == -4)) ||
                    ((color == 4) || (color == -4)) ||
                    ((lenght == 4) || (lenght == -4)) ||
                    ((hole == 4) || (hole == -4)))
                {
                    Debug.Log("ENDGAME CHECKED!");
                    placeSetId = x;
                    endGame = true;
                    for (int y = 0; y < 4; y++)
                    {
                        places[placeIds[placeSetId, y]].GetComponent<MeshRenderer>().material = placeMatEnd;
                    }
                    //break;
                }
            }

            if (endGame)
            {
                Debug.Log("SECOND CHECK!!");
                /*
                for (int y = 0; y < 4; y++)
                {
                    places[placeIds[placeSetId, y]].GetComponent<MeshRenderer>().material = placeMatEnd;
                }
                */
                EndGame(false);
            }
            else
                if (pieceCount == 16)
            {
                Debug.Log("DRAW!");
                EndGame(true);
            }
            else
            {
                placeSound.Play();
                gameState = GameState.PickPiece;

            }

            //if (places[0].pie)
        }

        //[ClientRpc]
        public void EndGame(bool draw)
        {
            
            Debug.Log("FINALLY ENDGAME!!!");
            gameState = GameState.EndGame;

            player1.endGame = true;
            player2.endGame = true;

            MenuController.instance.turnText.color = new Color32(206, 109, 6, 255);

            if (draw)
                MenuController.instance.turnText.text = "Draw!";
            else
                MenuController.instance.turnText.text = (player1.myTurn) ? player1.playerName + " won!" : player2.playerName + " won!";
            endGameSound.Play();

            MenuController.instance.phaseText.text = "Press \"Restart\" for new game";
        }

        [Command]
        public void CmdRestart()
        {
            //RpcStartMatch();
            RpcRemoteStart();
        }

        [ClientRpc]
        public void RpcRemoteStart()
        {
            StartMatch();
        }

        
    }
}
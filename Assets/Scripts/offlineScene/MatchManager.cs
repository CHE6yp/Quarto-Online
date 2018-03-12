using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using OfflineScene;

namespace OfflineScene
{

    public class MatchManager : MonoBehaviour
    {

        public static MatchManager singleton;
        //public int playerCount = 0;
        public Player players;
        public bool ai;
        public int pieceCount = 0;
        //public Player player2;
        //hotseat
        public bool fistPlayerTurn;

        public enum GameState { WaitForPlayers, Start, PickPiece, PutPiece, EndGame }
        public GameState gameState = GameState.WaitForPlayers;

        public Piece pickedPiece;

        public Place[] places = new Place[16];
        public int[,] placeIds = new int[,] { { 0, 1, 2, 3 }, { 4, 5, 6, 7 }, { 8, 9, 10, 11 }, { 12, 13, 14, 15 },
                                            { 0, 4, 8, 12 }, {1,5,9,13 }, {2,6,10,14 }, {3,7,11,15 }, {0,5,10,15 }, {3,6,9,12 } };

        public Material placeMatEnd;
        public Material placeMatStart;

        public GameObject piecePref;
        public GameObject piecesOrigin;
        public Piece[] pieces = new Piece[16];

        public AudioSource endGameSound;
        public AudioSource pickSound;
        public AudioSource placeSound;

        // Use this for initialization
        void Start()
        {
            singleton = this;
            MenuController.instance.matchManagerOffline = this;
            ai = FindObjectOfType<Params>().ai;
            //StartMatch();
            StartMatch();
        }


        void StateMachine()
        {

            switch (gameState)
            {
                case GameState.WaitForPlayers:

                    break;
                case GameState.Start:
                    
                    break;
                case GameState.PickPiece:

                    break;
                case GameState.PutPiece:

                    break;
                case GameState.EndGame:

                    break;

            }


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
                pieces[i].done = false;
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

            players.endGame = false;
            //player2.endGame = false;

            for (int y = 0; y < 16; y++)
            {
                places[y].GetComponent<MeshRenderer>().material = placeMatStart;
            }


            pieceCount = 0;
            MenuController.instance.turnText.color = new Color32(255, 255, 255,255);
            //gameState = GameState.Start;
            Debug.Log("Start Game");
            players.myTurn = true;
            fistPlayerTurn = true;
            if (ai)
                MenuController.instance.turnText.text = "Your turn";
            else
                MenuController.instance.turnText.text = "Player 1 turn";
            gameState = GameState.PickPiece;
            MenuController.instance.phaseText.text = "Pick Piece";
        }



        public void SwitchTurnHotseat()
        {
            fistPlayerTurn = !fistPlayerTurn;
            MenuController.instance.turnText.text = (fistPlayerTurn) ? "Player 1 turn" : "Player 2 turn";
            gameState = GameState.PutPiece;
        }

        public void SwitchTurnAI()
        {
            fistPlayerTurn = !fistPlayerTurn;
            players.myTurn = !players.myTurn;
            MenuController.instance.turnText.text = (fistPlayerTurn) ? "Your turn" : "AI turn";
            if (!players.myTurn)
                StartCoroutine(PickRandPlace());
            gameState = GameState.PutPiece;
        }

        //[ClientRpc]
        public void RpcSetPiece(Piece pickedPiece)
        {
            if (!pickedPiece.played)
            {
                this.pickedPiece = pickedPiece;
                this.pickedPiece.Pick();
                MenuController.instance.phaseText.text = "Place Piece";
                pickSound.Play();
                if (ai)
                    SwitchTurnAI();
                else
                    SwitchTurnHotseat();
            }
        }


        //[ClientRpc]
        public void RpcPutPiece(Place pickedPlaceARR)
        {
            Place pickedPlace = pickedPlaceARR;
            if (!pickedPlace.taken)
            {
                pickedPiece.done = true;
                pickedPiece.transform.position = pickedPlace.transform.position + new Vector3(0, 0.75f, 0);
                pickedPlace.piece = pickedPiece;
                pickedPiece.Drop();
                pickedPiece = null;
                pickedPlace.taken = true;

                pieceCount++;
                MenuController.instance.phaseText.text = "Pick Piece";

                //ПРОВЕРКА ДОСКИ НА ПОБЕДУ
                CheckGame();
            }

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

                EndGame(false);
            }
            else
                if (pieceCount == 16)
                EndGame(true);
            else
            {
                placeSound.Play();
                gameState = GameState.PickPiece;
            }
            


            //if (places[0].pie)
        }

        //[ClientRpc]s
        public void EndGame(bool draw)
        {
            Debug.Log("FINALLY ENDGAME!!!");
            gameState = GameState.EndGame;

            players.endGame = true;
            
            //CE6E06FF
            //MenuController.instance.turnText.color = new Color(206, 109, 6);
            MenuController.instance.turnText.color = new Color32(206, 109, 6,255);

            if (draw)
                MenuController.instance.turnText.text = "Draw!";
            else if (!ai)
                MenuController.instance.turnText.text = (fistPlayerTurn) ? "Player 1 won!" : "Player 2 won!";
            else
                MenuController.instance.turnText.text = (fistPlayerTurn) ? "You won!" : "AI won!";

            
            endGameSound.Play();
            MenuController.instance.phaseText.text = "Press \"Restart\" for new game";
        }

        //[Command]
        public void CmdRestart()
        {
            StartMatch();
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //AI!!!!!!
        public IEnumerator PickRandPiece()
        {
            int id = Random.Range(0, 16);
            if (!pieces[id].played)
            {
                yield return new WaitForSeconds(1);
                RpcSetPiece(pieces[id]);
                //SwitchTurnAI();
            }
            else
            {
                StartCoroutine( PickRandPiece());
            }
        }

        public IEnumerator PickRandPlace()
        {
            int id = Random.Range(0, 16);
            if (!places[id].taken)
            {
                yield return new WaitForSeconds(2);
                RpcPutPiece(places[id]);
                //SwitchTurnAI();
                if (gameState != GameState.EndGame)
                    StartCoroutine(PickRandPiece());
            }
            else
            {
                StartCoroutine(PickRandPlace());
            }
        }

    }
}
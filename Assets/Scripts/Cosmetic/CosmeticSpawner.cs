using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticSpawner : MonoBehaviour {


    public GameObject piecePref;
    public CosmeticPiece[] piecepool;
    public int size =15;
    public int index = 0;
    public float interval = 0;
	// Use this for initialization
	void Start () {
        piecepool = new CosmeticPiece[size];
        for (int x = 0; x < size; x++)
        {
            piecepool[x] = Instantiate(piecePref).GetComponent<CosmeticPiece>();
            piecepool[x].spawn = gameObject;

        }
	}
	
	// Update is called once per frame
	void Update () {
        interval += Time.deltaTime;
        if (interval >= 1.5f)
        {
            piecepool[index].Randomize();
            index++;
            if (index == size) index = 0;
            interval = 0;
        }
	}
}

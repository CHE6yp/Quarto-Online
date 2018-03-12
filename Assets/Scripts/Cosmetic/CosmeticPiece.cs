using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticPiece : MonoBehaviour {

    bool shape;
    bool lenght;
    bool hole;
    bool color;

    public Material black;
    public Material white;

    public GameObject[] elements = new GameObject[0];
    public GameObject spawn;


	// Use this for initialization
	public void Randomize () {
        shape = (Random.Range(0, 2) == 0) ? true : false;
        lenght = (Random.Range(0, 2) == 0) ? true : false;
        hole = (Random.Range(0, 2) == 0) ? true : false;
        color = (Random.Range(0, 2) == 0) ? true : false;

        Assemble();
    }
	
    void Assemble()
    {
        for (int i = 0;i < 8; i++)
        {
            elements[i].SetActive(false);
            elements[i].GetComponent<MeshRenderer>().material = white;
        }

        int x = 0;
        x += (shape) ? 4 : 0;

        elements[x].SetActive(true);
        if (lenght) elements[x + 1].SetActive(true);
        if (hole)
            if (lenght)
                elements[x + 3].SetActive(true);
            else
                elements[x + 2].SetActive(true);
        if (color)
        {
            elements[x].GetComponent<MeshRenderer>().material = black;
            if (lenght) elements[x+1].GetComponent<MeshRenderer>().material = black;
        } else
        {
            elements[x+2].GetComponent<MeshRenderer>().material = black;
            if (lenght) elements[x + 3].GetComponent<MeshRenderer>().material = black;
        }

        gameObject.transform.position = spawn.transform.position;
        gameObject.transform.rotation = spawn.transform.rotation;
    }

	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.back * Time.deltaTime);
	}
}

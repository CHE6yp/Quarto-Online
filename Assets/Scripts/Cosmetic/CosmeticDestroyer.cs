using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticDestroyer : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }
}

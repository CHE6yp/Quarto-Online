using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Piece : MonoBehaviour
{
    public int id;
    public bool played;
    public enum Shape { Box, Circle };
    public enum Color { White, Black };
    public enum Lenght { Short, Long };
    public enum Hole { Empty, Whole };

    public Shape shape = Shape.Box;
    public Color color = Color.White;
    public Lenght lenght = Lenght.Short;
    public Hole hole = Hole.Empty;

    public bool done;

    ParticleSystem particles;
    public cakeslice.Outline outline;

    // Use this for initialization
    void Start()
    {
        particles = transform.Find("Particle System").GetComponent<ParticleSystem>();
        particles.Stop();
    }


    public void Pick()
    {

        outline.enabled = true;
        Debug.Log("Picked");
        particles.Play();

    }

    public void Drop()
    {
        outline.enabled = false;
        played = true;
        particles.Stop();
    }
}

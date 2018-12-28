using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called before the first frame update
    List<Body> bodies = new List<Body>();
    void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            bodies.Add(new Body(new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-2f, 2f)),
                                Random.Range(1, 3)));
        }
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        foreach (Body b in bodies)
        {
            Gizmos.DrawSphere(b.Position, 0.3f * b.Mass);
        }
    }

    void Update()
    {
        Octree o = new Octree();
        o.Build(bodies, 5);
        foreach (Body b in bodies)
        {
            o.Interact(b, 1f);
        }
        foreach (Body b in bodies)
        {
            b.Update(Time.deltaTime * 2);
        }
    }
}

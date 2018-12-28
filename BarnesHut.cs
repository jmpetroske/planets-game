using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Octree
{
    protected Octree[] Children;
    protected IEnumerable<Body> Bodies;
    protected Bounds Bounds;
    protected Body AverageBody;
    protected Color color;

    public Octree()
    {
        color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.3f);
    }

    public void Build(IEnumerable<Body> bodies, uint maxDepth)
    {
        Body center = CalculateAverageBody(bodies);
        float radius = CalculateRadius(bodies, center.Position);
        Build(new Bounds(center.Position, radius),
              bodies,
              0,
              maxDepth);
    }

    public void Build(
        Bounds bounds,
        IEnumerable<Body> bodies,
        uint currentDepth,
        uint maxDepth)
    {
        Bounds = bounds;
        AverageBody = CalculateAverageBody(bodies);

        uint threshold = 1;
        if (bodies.Count() <= threshold || currentDepth >= maxDepth)
        {
            Bodies = bodies;
            return;
        }

        Children = new Octree[8];
        var subOcts = from b in bodies
                      group b by ((b.Position.x > bounds.center.x ? 1 : 0)
                                + (b.Position.y > bounds.center.y ? 2 : 0)
                                + (b.Position.z > bounds.center.z ? 4 : 0));
        foreach (var oct in subOcts)
        {
            Octree childOct = new Octree();
            Bounds b = bounds.MakeChildBounds(oct.Key);
            childOct.Build(b, oct, currentDepth + 1, maxDepth);
            Children[oct.Key] = childOct;
        }
    }

    public void Interact(Body b, float threshold = 0.1f)
    {
        if (AverageBody == null)
        {
            return;
        }
        float distance = Vector3.Distance(b.Position, AverageBody.Position);
        if (distance == 0)
            return;

        if (Bounds.radius / distance > threshold && Children != null)
        {
            foreach (Octree o in Children.Where(c => c != null))
            {
                o.Interact(b, threshold);
            }
        } else
        {
            b.Interact(AverageBody);
        }
    }

    public static float CalculateRadius(IEnumerable<Body> bodies, Vector3 center)
    {
        float max = 0f;
        foreach (Body b in bodies)
        {
            max = Mathf.Max(max, Math.Abs(b.Position.x - center.x));
            max = Mathf.Max(max, Math.Abs(b.Position.y - center.y));
            max = Mathf.Max(max, Math.Abs(b.Position.z - center.z));
        }
        return max;
    }

    public static Body CalculateAverageBody(IEnumerable<Body> bodies)
    {
        Vector3 weightedPosition = Vector3.zero;
        float totalMass = 0f;
        foreach (Body b in bodies)
        {
            weightedPosition += b.Position;
            totalMass += b.Mass;
        }
        if (totalMass == 0)
        {
            return new Body(Vector3.zero, 0f);
        } else
        {
            return new Body(weightedPosition / totalMass, totalMass);
        }
    }

    public void DrawGizmos()
    {
        this.color.a = 0.2f;
        Gizmos.color = this.color;
        //Gizmos.DrawCube(Bounds.center, Vector3.one * Bounds.radius * 2);
        this.color.a = 1f;
        Gizmos.color = this.color;
        Gizmos.color = Color.blue;
        if (Bodies != null)
        {
            foreach (Body b in Bodies)
            {
                Gizmos.DrawSphere(b.Position, 0.2f);
            }
        }
        if (Children != null)
        {
            foreach (Octree o in Children.Where(c => c != null))
            {
                o.DrawGizmos();
            }
        }
    }
}

public class Body
{
    public Vector3 Position { get; set; }
    public Vector3 Acc { get; set; }
    public float Mass { get; set; }

    public Body(Vector3 position, float mass)
    {
        this.Position = position;
        this.Mass = mass;
    }

    public void Interact(Body other)
    {
        Vector3 diff = other.Position - this.Position;
        float distance = Mathf.Clamp(diff.magnitude, 0.3f, 30f);
        float G = 0.001f;
        float strength = this.Mass * other.Mass * G / (distance * distance);
        Vector3 force = diff.normalized * strength;
        Acc += force / this.Mass;
    }

    public void Update(float deltaTime)
    {
        Position += deltaTime * Acc * 15;
    }
}

public struct Bounds
{
    public Vector3 center;
    public float radius;

    public Bounds(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    public Bounds MakeChildBounds(int dirNum)
    {
        Vector3 childBoundsCenter = new Vector3(center.x, center.y, center.z);
        if (dirNum % 2 == 0) // x
        {
            childBoundsCenter.x -= radius / 2;   
        } else
        {
            childBoundsCenter.x += radius / 2;
        }
        if ((dirNum / 2) % 2 == 0) // y
        {
            childBoundsCenter.y -= radius / 2;
        }
        else
        {
            childBoundsCenter.y += radius / 2;
        }
        if ((dirNum / 4) % 2 == 0) // z
        {
            childBoundsCenter.z -= radius / 2;
        }
        else
        {
            childBoundsCenter.z += radius / 2;
        }
        return new Bounds(childBoundsCenter, radius / 2);
    }
}

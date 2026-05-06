using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CestialBodySettings
{
    public string name;
    public float surfaceGravity;
    public float radius;
    public float mass
    {
        get
        {
            return surfaceGravity * radius * radius / GravitySimulation.gravityConstant;
        }
    }

    public Vector3 startingPosition;
    public Vector3 startingVelocity;

    public Color color;
}

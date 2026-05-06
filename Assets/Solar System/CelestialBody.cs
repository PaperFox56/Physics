using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CelestialBody
{
    public GameObject obj;
    
    public Vector3 Position
    {
        get
        {
            return obj.transform.position;
        }

        set
        {
            obj.transform.position = value;
        }
    }

    public Vector3 velocity;
    public Vector3 acceleration;

    public CestialBodySettings settings;

    public CelestialBody(CestialBodySettings settings, Vector3 vel, GameObject obj)
    {
        this.settings = settings;
        this.obj = obj;
    
        velocity = vel;
        acceleration = new Vector3();
    }

    public CelestialBody(CelestialBody other)
    {
        obj = other.obj;
        settings = other.settings;

        velocity = other.velocity;
        acceleration = other.acceleration;
    }
}

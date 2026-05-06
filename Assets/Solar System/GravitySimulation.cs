using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class GravitySimulation : MonoBehaviour
{

    public static float gravityConstant = 1;

    public bool predictTrajectories;
    public float trajectoryThickness = 1;
    public int trajectoryIterationCount = 100;
    public float trajectoryPredictionStep = .1f;

    // Invalid values mean that everthing is just rendered relative to the universe
    public int CenterBodyIndex = -1;

    public CestialBodySettings[] celestialBodySettings;

    CelestialBody[] celestialBodies;
    GameObject[] celestialBodiesObjects;

    // For when an element is added or removed in the editor

    public void OnValidate()
    {
        Initialize();

        if (predictTrajectories)
        {
            DrawPredictedTrajectories();
        }
    }

    public void Initialize()
    {
        // Create the game objects that will represent the actual bodies
        // We do not want to generate the objects if not necessary

        int bodyCount = celestialBodySettings.Length;

        // 1. Handle Array Resizing and Destruction
        if (celestialBodiesObjects != null && celestialBodiesObjects.Length > bodyCount)
        {
            for (int i = bodyCount; i < celestialBodiesObjects.Length; i++)
            {
                GameObject toDestroy = celestialBodiesObjects[i]; // Capture local reference
                if (toDestroy != null)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (toDestroy != null) DestroyImmediate(toDestroy);
                    };
                }
            }
        }

        // Adjust array size
        System.Array.Resize(ref celestialBodiesObjects, bodyCount);
        celestialBodies = new CelestialBody[bodyCount];

        for (int i = 0; i < bodyCount; i++)
        {
            if (celestialBodiesObjects[i] == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = transform;

                sphere.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                sphere.hideFlags = HideFlags.DontSave;

                LineRenderer line = sphere.AddComponent<LineRenderer>();
                line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                line.startWidth = 0.1f;
                line.endWidth = 0.1f;

                celestialBodiesObjects[i] = sphere;
            }

            // Apply settings
            GameObject bodyObject = celestialBodiesObjects[i];
            CestialBodySettings settings = celestialBodySettings[i];

            bodyObject.name = settings.name;
            bodyObject.transform.localPosition = settings.startingPosition;
            bodyObject.transform.localScale = Vector3.one * settings.radius;

            // Use sharedMaterial to avoid memory leaks in the Editor
            bodyObject.GetComponent<MeshRenderer>().sharedMaterial.color = settings.color;

            LineRenderer lr = bodyObject.GetComponent<LineRenderer>();
            lr.startColor = settings.color;
            lr.endColor = settings.color;
            lr.startWidth = trajectoryThickness;
            lr.endWidth = trajectoryThickness;

            celestialBodies[i] = new CelestialBody(settings, settings.startingVelocity, bodyObject)
            {
                Position = settings.startingPosition
            };
        }
    }

    // Update the positions and velocties of every body in the system
    void StepInSimulation(float deltaTime)
    {
        // Update all the accelerations
        for (int i = 0; i < celestialBodies.Length; i++)
        {
            Vector3 acc = new Vector3();

            for (int j = 0; j < celestialBodies.Length; j++)
            {
                if (i != j)
                {
                    Vector3 distance = celestialBodies[j].Position - celestialBodies[i].Position;
                    float dist = Vector3.Dot(distance, distance) + .001f;
                    acc += gravityConstant * celestialBodies[j].settings.mass * distance.normalized / dist;
                }
            }

            celestialBodies[i].acceleration = acc;
        }

        // Update all the positions and velocities
        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].velocity += celestialBodies[i].acceleration * deltaTime;
            celestialBodies[i].Position += celestialBodies[i].velocity * deltaTime;
        }

        if (CenterBodyIndex >= 0 && CenterBodyIndex < celestialBodies.Length)
        {
            Vector3 centerPosition = celestialBodies[CenterBodyIndex].Position;

            for (int i = 0; i < celestialBodies.Length; i++)
            {
                celestialBodies[i].Position -= centerPosition;
            }
        }
    }

    void DrawPredictedTrajectories()
    {
        // Create a deep copy for simulation
        Vector3[] cachedPositions = new Vector3[celestialBodies.Length];
        Vector3[] cachedVelocities = new Vector3[celestialBodies.Length];
        Vector3[][] positions = new Vector3[celestialBodies.Length][];

        for (int i = 0; i < celestialBodies.Length; i++)
        {
            cachedPositions[i] = celestialBodies[i].Position;
            cachedVelocities[i] = celestialBodies[i].velocity;
            positions[i] = new Vector3[trajectoryIterationCount];
        }

        for (int i = 0; i < trajectoryIterationCount; i++)
        {
            StepInSimulation(trajectoryPredictionStep);
            for (int j = 0; j < celestialBodies.Length; j++)
            {
                positions[j][i] = celestialBodies[j].Position;
            }
        }

        // Update LineRenderers
        for (int j = 0; j < celestialBodies.Length; j++)
        {
            LineRenderer lr = celestialBodiesObjects[j].GetComponent<LineRenderer>();
            lr.positionCount = trajectoryIterationCount;
            lr.SetPositions(positions[j]);
        }

        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].Position = cachedPositions[i];
            celestialBodies[i].velocity = cachedVelocities[i];
        }
        if (CenterBodyIndex >= 0 && CenterBodyIndex < celestialBodies.Length)
        {
            Vector3 centerPosition = celestialBodies[CenterBodyIndex].Position;

            for (int i = 0; i < celestialBodies.Length; i++)
            {
                celestialBodies[i].Position -= centerPosition;
            }
        }
    }

    public void FixedUpdate()
    {
        StepInSimulation(Time.deltaTime);
    }
}

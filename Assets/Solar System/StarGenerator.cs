using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class StarGenerator : MonoBehaviour
{

    public int starCount = 3000;

    public float minDistance = 3000;
    public float maxDistance = 5000;

    public float sizeMultiplier = 1;
    public AnimationCurve sizeRepartition;


    void OnValidate()
    {
        for (int i = transform.childCount; i < starCount; i++)
        {
            // Add more children
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = transform;

            sphere.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Color"));
            sphere.hideFlags = HideFlags.DontSave;
        }

        for (int i = starCount; i < transform.childCount; i++)
        {
            GameObject toDestroy = transform.GetChild(i).gameObject;
            if (toDestroy != null)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (toDestroy != null) DestroyImmediate(toDestroy);
                    };
                }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform star = transform.GetChild(i);

            Vector3 direction = Random.onUnitSphere;
            star.localPosition = Random.Range(minDistance, maxDistance) * direction;

            star.localScale = sizeRepartition.Evaluate(Random.value) * sizeMultiplier * new Vector3(1, 1, 1);
        }
    }
}

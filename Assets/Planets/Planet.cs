using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    [Range(2,256)]
    public int resolution = 10;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    public bool autoUpdate = true;
    [HideInInspector]
    public bool colorFoldout = true;
    [HideInInspector]
    public bool shapeFoldout = true;

    ShapeGenerator shapeGenerator;

     private void Start() {
        shapeGenerator = new ShapeGenerator(shapeSettings);
        Initialize();
        GenerateMesh();
        GenerateColors();
    }

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    [SerializeField, HideInInspector]
    TerrainFace[] faces;

    void Initialize() {
        shapeGenerator = new ShapeGenerator(shapeSettings);

        if (meshFilters == null || meshFilters.Length == 0) {
            meshFilters = new MeshFilter[6]; 
        }
        faces = new TerrainFace[6];

        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back, Vector3.forward};

        for (int i = 0; i < 6; i++) {
            if (meshFilters[i] == null) {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            faces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    public void GeneratePlanet() {
        Initialize();
        GenerateMesh();
        GenerateColors();
    }

    public void OnShapeSettingsUpdated() {
        if (shapeSettings != null && autoUpdate) {
            Initialize();
            GenerateMesh();
        }
    }

    public void OnColorSettingsUpdated() {
        if (colorSettings != null && autoUpdate) {
            Initialize();
            GenerateColors();
        }
    }

    public void GenerateMesh() {
        foreach(TerrainFace face in faces) {
            face.ConstructMesh();
        }
    }

    public void GenerateColors() {
        foreach (MeshFilter m in meshFilters) {
            m.GetComponent<MeshRenderer>().sharedMaterial.color = colorSettings.planetColor;
        }
    }
}

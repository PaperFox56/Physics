using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class TerrainFace
{
    Mesh mesh;

    int resolution;

    ShapeGenerator shapeGenerator;

    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int j = 0;

        Vector3[] pointsToRecalculate = new Vector3[resolution * 4 - 4];
        int[] pointsToRecalculateIndices = new int[pointsToRecalculate.Length];

        Parallel.For(0, resolution, y =>
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;

                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = PointOnCubeToPointOnSphere(pointOnUnitCube);

                vertices[i] = shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);

                if (x == 0 || y == 0 || x == resolution - 1 || y == resolution - 1)
                {
                    pointsToRecalculate[j] = pointOnUnitCube;
                    pointsToRecalculateIndices[j] = i;
                    j++;
                }

                if (x < resolution - 1 && y < resolution - 1)
                {
                    int triIndex = (x + y * (resolution - 1)) * 6;

                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                }
            }
        });

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        Vector2[] neighborOffsets =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(2, 0),
            new Vector2(2, 1),
            new Vector2(2, 2),
            new Vector2(1, 2),
            new Vector2(0, 2),
            new Vector2(0, 1)
        };

        float epsilon = 0.01f;

        Vector3[] normals = mesh.normals;

        // We need to manually recalculate the normals at the edges to smooth out the surface
        Parallel.For(0, pointsToRecalculate.Length, i =>
        {

            // Current point
            Vector3 p = pointsToRecalculate[i];
            Vector3 spherePoint = PointOnCubeToPointOnSphere(p);
            Vector3 center = vertices[pointsToRecalculateIndices[i]];

            Vector3 pA1 = p + axisA * epsilon;
            Vector3 pA2 = p - axisA * epsilon;

            Vector3 pB1 = p + axisB * epsilon;
            Vector3 pB2 = p - axisB * epsilon;

            Vector3 pointA = shapeGenerator.CalculatePointOnPlanet(PointOnCubeToPointOnSphere(pA1));
            Vector3 pointA2 = shapeGenerator.CalculatePointOnPlanet(PointOnCubeToPointOnSphere(pA2));

            Vector3 pointB = shapeGenerator.CalculatePointOnPlanet(PointOnCubeToPointOnSphere(pB1));
            Vector3 pointB2 = shapeGenerator.CalculatePointOnPlanet(PointOnCubeToPointOnSphere(pB2));

            Vector3 tangentA = pointA - pointA2;
            Vector3 tangentB = pointB - pointB2;

            Vector3 normal = Vector3.Cross(tangentA, tangentB).normalized;
            // Vector3 sphereNormal = PointOnCubeToPointOnSphere(p).normalized;
            // normal = (normal + sphereNormal).normalized;

            normals[pointsToRecalculateIndices[i]] = normal;
        });

        mesh.normals = normals;
    }

    static Vector3 PointOnCubeToPointOnSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;
        float x = p.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        float y = p.y * Mathf.Sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
        float z = p.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);
        return new Vector3(x, y, z);
    }
}

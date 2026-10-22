using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace SupercraneSimulator.Editor.SceneBuilder
{
    /// <summary>
    /// Mallas con el tamaño ya aplicado (para no escalar transforms) y materiales URP.
    /// Se guardan como assets para que la escena las pueda referenciar.
    /// </summary>
    internal static class BuilderAssets
    {
        private const string MeshFolder = "Assets/_Project/Models/Generated";
        private const string MaterialFolder = "Assets/_Project/Materials";

        private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();

        public static void ClearCache()
        {
            Meshes.Clear();
        }

        /// <summary>Caja de tamaño <paramref name="size"/> centrada en el origen.</summary>
        public static Mesh Box(Vector3 size) => Get(PrimitiveType.Cube, size, Vector3.one);

        /// <summary>Cilindro vertical de diámetro y alto dados, centrado en el origen.</summary>
        public static Mesh Cylinder(float diameter, float height) =>
            Get(PrimitiveType.Cylinder, new Vector3(diameter, height, diameter), new Vector3(1f, 2f, 1f));

        public static Material Material(string name, Color color, float smoothness = 0.3f, float metallic = 0f)
        {
            EnsureFolder(MaterialFolder);
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh Get(PrimitiveType type, Vector3 size, Vector3 baseSize)
        {
            var key = string.Format(CultureInfo.InvariantCulture, "{0}_{1:0.###}x{2:0.###}x{3:0.###}", type, size.x, size.y, size.z);
            if (Meshes.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            EnsureFolder(MeshFolder);
            var path = $"{MeshFolder}/{key}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = BuildScaledMesh(type, new Vector3(size.x / baseSize.x, size.y / baseSize.y, size.z / baseSize.z));
                mesh.name = key;
                AssetDatabase.CreateAsset(mesh, path);
            }

            Meshes[key] = mesh;
            return mesh;
        }

        private static Mesh BuildScaledMesh(PrimitiveType type, Vector3 scale)
        {
            var temp = GameObject.CreatePrimitive(type);
            var source = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            var vertices = source.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], scale);
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                normals = source.normals,
                uv = source.uv,
                triangles = source.triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}

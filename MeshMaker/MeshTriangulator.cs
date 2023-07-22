using EarClipperLib;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using System.Collections.Generic;
using System.Linq;

namespace MeshMaker
{
    internal static class MeshTriangulator
    {
        public static void Triangulate(LoadResult loadedObj, out List<MeshVertex> meshVertices, out List<MeshFace> meshFaces)
        {
            meshVertices = new List<MeshVertex>();
            meshFaces = new List<MeshFace>();

            EarClipping earClipper = new EarClipping();

            foreach (Group objGroup in loadedObj.Groups)
            {
                foreach (Face objFace in objGroup.Faces)
                {
                    // store the vertex' normal and texture coords by its position
                    // this way, we can retrieve the corresponding info for each triangle vertex
                    var vertexInfo = new Dictionary<Vector3m, (Normal, Texture)>();

                    for (int i = 0; i < objFace.Count; ++i)
                    {
                        FaceVertex objFaceVertex = objFace[i];

                        Vertex objVertex = loadedObj.Vertices[objFaceVertex.VertexIndex - 1];
                        Normal objNormal = loadedObj.Normals[objFaceVertex.NormalIndex - 1];
                        Texture objTextureCoord = loadedObj.Textures[objFaceVertex.TextureIndex - 1];

                        (Normal, Texture) normalAndTexture = (objNormal, objTextureCoord);
                        vertexInfo.Add(new Vector3m(objVertex.X, objVertex.Y, objVertex.Z), normalAndTexture);
                    }

                    earClipper.SetPoints(vertexInfo.Keys.ToList());
                    earClipper.Triangulate();

                    foreach (Vector3m triangleVertexCoords in earClipper.Result)
                    {
                        MeshVertex meshVertex = new MeshVertex();
                        meshVertex.PosX = (float)triangleVertexCoords.X.ToDouble();
                        meshVertex.PosY = (float)triangleVertexCoords.Y.ToDouble();
                        meshVertex.PosZ = (float)triangleVertexCoords.Z.ToDouble();

                        Normal objNormal = vertexInfo[triangleVertexCoords].Item1;
                        Texture objTextureCoord = vertexInfo[triangleVertexCoords].Item2;

                        meshVertex.NormX = objNormal.X;
                        meshVertex.NormY = objNormal.Y;
                        meshVertex.NormZ = objNormal.Z;

                        meshVertex.TexX = objTextureCoord.X;
                        meshVertex.TexY = objTextureCoord.Y;

                        meshVertices.Add(meshVertex);
                    }
                }
            }
            
            // make faces using the vertex indices of each triangle
            for (int i = 0; i < meshVertices.Count; i += 3)
            {
                MeshFace meshFace = new MeshFace();
                meshFace.A = i;
                meshFace.B = i + 1;
                meshFace.C = i + 2;
                meshFaces.Add(meshFace);
            }
        }
    }
}

using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MeshMaker
{
    internal enum MeshVersion
    {
        VERSION_UNKNOWN,
        VERSION_101,
        VERSION_200
    }

    internal class MeshWriter : IDisposable
    {
        protected StreamWriter Writer;

        protected string VersionString;
        protected List<MeshVertex> MeshVertices;
        protected List<MeshFace> MeshFaces;

        public static MeshWriter Create(MeshVersion meshVersion, Stream stream, LoadResult loadedObj)
        {
            switch (meshVersion)
            {
                case MeshVersion.VERSION_101:
                    return new MeshWriterV1(stream, loadedObj);
                case MeshVersion.VERSION_200:
                    return new MeshWriterV2(stream, loadedObj);
                default:
                    throw new ArgumentException("Unsupported mesh version number");
            }
        }

        protected MeshWriter(Stream stream, LoadResult loadedObj)
        {
            Writer = new StreamWriter(stream);
            Writer.AutoFlush = true;

            MeshTriangulator.Triangulate(loadedObj, out MeshVertices, out MeshFaces);
        }

        public void Dispose()
        {
            Writer.Close();
        }

        public virtual void Write()
        {
            Writer.WriteLine(VersionString);
        }
    }

    internal class MeshWriterV1 : MeshWriter
    {
        public MeshWriterV1(Stream stream, LoadResult loadedObj) : base(stream, loadedObj)
        {
            VersionString = "version 1.01";
        }

        public override void Write()
        {
            base.Write();

            string data = string.Empty;

            foreach (MeshVertex vertex in MeshVertices)
            {
                data += string.Format("[{0}, {1}, {2}]", vertex.PosX, vertex.PosY, vertex.PosZ);
                data += string.Format("[{0}, {1}, {2}]", vertex.NormX, vertex.NormY, vertex.NormZ);
                data += string.Format("[{0}, {1}, {2}]", vertex.TexX, vertex.TexY, 0f);
            }

            Writer.WriteLine(MeshFaces.Count);
            Writer.WriteLine(data);
        }
    }

    internal class MeshWriterV2 : MeshWriter
    {
        public MeshWriterV2(Stream stream, LoadResult loadedObj) : base(stream, loadedObj)
        {
            VersionString = "version 2.00";
        }

        public override void Write()
        {
            base.Write();

            BinaryWriter binaryWriter = new BinaryWriter(Writer.BaseStream);

            MeshHeader header = new MeshHeader();
            header.HeaderSize = (short)Marshal.SizeOf<MeshHeader>();
            header.VertexSize = (byte)Marshal.SizeOf<MeshVertex>();
            header.FaceSize = (byte)Marshal.SizeOf<MeshFace>();
            header.VertexCount = MeshVertices.Count;
            header.FaceCount = MeshFaces.Count;

            binaryWriter.Write(header.HeaderSize);
            binaryWriter.Write(header.VertexSize);
            binaryWriter.Write(header.FaceSize);
            binaryWriter.Write(header.VertexCount);
            binaryWriter.Write(header.FaceCount);

            foreach (MeshVertex vertex in MeshVertices)
            {
                binaryWriter.Write(vertex.PosX);
                binaryWriter.Write(vertex.PosY);
                binaryWriter.Write(vertex.PosZ);

                binaryWriter.Write(vertex.NormX);
                binaryWriter.Write(vertex.NormY);
                binaryWriter.Write(vertex.NormZ);

                binaryWriter.Write(vertex.TexX);

                // weird anomaly with how texture coordinates are read
                binaryWriter.Write(1f - vertex.TexY);
            }

            foreach (MeshFace face in MeshFaces)
            {
                binaryWriter.Write(face.A);
                binaryWriter.Write(face.B);
                binaryWriter.Write(face.C);
            }
        }
    }
}

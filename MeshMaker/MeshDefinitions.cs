namespace MeshMaker
{
    internal struct MeshFace
    {
        public int A, B, C;
    }

    internal struct MeshVertex
    {
        public float PosX, PosY, PosZ;
        public float NormX, NormY, NormZ;
        public float TexX, TexY;
    }

    internal struct MeshHeader
    {
        public short HeaderSize;
        public byte VertexSize, FaceSize;
        public int VertexCount, FaceCount;
    }
}

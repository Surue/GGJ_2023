namespace UnityEngine.UI.Extensions
{
    public class EmissiveImage : Image
    {
        public Color emissionColor;
        [Range(0f, 1f)] public float emissionIntensity = 1f;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            int verticesCount = toFill.currentVertCount;
            for (int v = 0; v < verticesCount; v++)
            {
                UIVertex vertex = new UIVertex();
                toFill.PopulateUIVertex(ref vertex, v);
                vertex.normal = (Vector4)emissionColor * emissionIntensity;
                toFill.SetUIVertex(vertex, v);
            }
        }
    }
}
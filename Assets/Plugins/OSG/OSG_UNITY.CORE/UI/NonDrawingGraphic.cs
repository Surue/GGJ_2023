// Old Skull Games
// Bernard Barthelemy from https://answers.unity.com/users/153967/slippdouglas.html
// Wednesday, January 31, 2018 
using UnityEngine.UI;

namespace OSG
{
    /// <summary>
    /// A concrete subclass of the Unity UI `Graphic` class that just skips drawing.
    /// Useful for providing a raycast target without actually drawing anything.
    /// </summary>
    public class NonDrawingGraphic : Graphic
{
    public override void SetMaterialDirty()
    {
        return;
    }

    public override void SetVerticesDirty()
    {
        return;
    }

    /// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }
}


}
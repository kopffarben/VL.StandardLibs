global using Context = VL.ImGui.Context;
using VL.ImGui;

namespace VL.ImNodes.Widgets
{
    /// <summary>
    /// Test widget to debug Context issues
    /// </summary>
    [GenerateNode(Category = "ImNodes.Test")]
    public partial class TestWidget : Widget
    {
        internal override void UpdateCore(VL.ImGui.Context context)
        {
            // Just a test - do nothing
        }
    }
}
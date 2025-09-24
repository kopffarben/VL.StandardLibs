using imnodesNET;
using VL.ImGui;
using Context = VL.ImGui.Context;

namespace VL.ImNodes.Widgets
{
    /// <summary>
    /// Output attribute (pin) widget for ImNodes
    /// </summary>
    [GenerateNode(Category = "ImNodes.Widgets")]
    public sealed partial class OutputAttribute : Widget
    {
        public IEnumerable<Widget> Children { get; set; } = Enumerable.Empty<Widget>();

        [Pin(Priority = 100)]
        public int AttributeId { get; set; }

        internal override void UpdateCore(Context context)
        {
            // Begin the output attribute
            imnodes.BeginOutputAttribute(AttributeId);

            try
            {
                // Execute child widgets within the attribute
                foreach (var child in Children)
                {
                    if (child != null)
                        context.Update(child);
                }
            }
            finally
            {
                // End the output attribute
                imnodes.EndOutputAttribute();
            }
        }
    }
}
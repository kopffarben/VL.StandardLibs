using imnodesNET;
using VL.ImGui;

namespace VL.ImNodes.Widgets
{
    /// <summary>
    /// Input attribute (pin) widget for ImNodes
    /// </summary>
    [GenerateNode(Category = "ImNodes.Widgets")]
    public sealed partial class InputAttribute : Widget
    {
        public IEnumerable<Widget> Children { get; set; } = Enumerable.Empty<Widget>();

        [Pin(Priority = 100)]
        public int AttributeId { get; set; }

        internal override void UpdateCore(Context context)
        {
            // Begin the input attribute
            imnodes.BeginInputAttribute(AttributeId);

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
                // End the input attribute
                imnodes.EndInputAttribute();
            }
        }
    }
}
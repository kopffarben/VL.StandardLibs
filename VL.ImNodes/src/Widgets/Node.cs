using imnodesNET;
using System.Numerics;
using VL.ImGui;

namespace VL.ImNodes.Widgets
{
    /// <summary>
    /// Node container widget for ImNodes
    /// </summary>
    [GenerateNode(Category = "ImNodes.Widgets")]
    public sealed partial class Node : Widget
    {
        public IEnumerable<Widget> Children { get; set; } = Enumerable.Empty<Widget>();

        [Pin(Priority = 100)]
        public int NodeId { get; set; }

        public Vector2? Position { get; set; }

        internal override void UpdateCore(Context context)
        {
            // Set position if specified
            if (Position.HasValue)
            {
                imnodes.SetNodeEditorSpacePos(NodeId, Position.Value);
            }

            // Begin the node
            imnodes.BeginNode(NodeId);

            try
            {
                // Execute child widgets within the node
                foreach (var child in Children)
                {
                    if (child != null)
                        context.Update(child);
                }
            }
            finally
            {
                // End the node
                imnodes.EndNode();
            }
        }
    }
}
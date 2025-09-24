using imnodesNET;
using VL.ImGui;

namespace VL.ImNodes.Widgets
{
    /// <summary>
    /// Represents a link between two attributes/pins
    /// </summary>
    public struct Link
    {
        public int LinkId { get; set; }
        public int FromAttributeId { get; set; }
        public int ToAttributeId { get; set; }

        public Link(int linkId, int fromAttributeId, int toAttributeId)
        {
            LinkId = linkId;
            FromAttributeId = fromAttributeId;
            ToAttributeId = toAttributeId;
        }
    }

    /// <summary>
    /// Links widget for drawing connections in ImNodes
    /// </summary>
    [GenerateNode(Category = "ImNodes.Widgets")]
    public sealed partial class Links : Widget
    {
        public IEnumerable<Link> LinksData { get; set; } = Enumerable.Empty<Link>();

        internal override void UpdateCore(Context context)
        {
            // Draw each link
            foreach (var link in LinksData)
            {
                imnodes.Link(link.LinkId, link.FromAttributeId, link.ToAttributeId);
            }
        }

        protected override bool HasItemState => false;
    }
}
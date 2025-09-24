using imnodesNET;
using System.Numerics;
using VL.ImGui;

namespace VL.ImNodes.Editors
{
    /// <summary>
    /// Core node editor widget that provides the ImNodes editing canvas
    /// </summary>
    [GenerateNode(Category = "ImNodes")]
    public sealed partial class NodeEditor : Widget
    {
        private EditorContext? _context;

        public IEnumerable<Widget> Children { get; set; } = Enumerable.Empty<Widget>();
        
        public EditorContext? Context { get; set; }
        
        public bool Enabled { get; set; } = true;

        internal override void UpdateCore(Context context)
        {
            if (!Enabled)
                return;

            // Use provided context or create/reuse our own
            var editorContext = Context ?? (_context ??= new EditorContext());
            
            // Set as current context
            editorContext.SetCurrent();

            // Begin the node editor
            imnodes.BeginNodeEditor();

            try
            {
                // Execute child widgets within the editor
                foreach (var child in Children)
                {
                    if (child != null)
                        context.Update(child);
                }
            }
            finally
            {
                // End the node editor
                imnodes.EndNodeEditor();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
                _context = null;
            }
            base.Dispose(disposing);
        }
    }
}
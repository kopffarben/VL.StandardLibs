using VL.Core;
using VL.Core.CompilerServices;

[assembly: AssemblyInitializer(typeof(VL.ImNodes.Initialization))]

namespace VL.ImNodes
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        public override void Configure(AppHost appHost)
        {
            NodeBuildingUtils.RegisterGeneratedNodes(appHost, "VL.ImNodes.Nodes", typeof(Initialization).Assembly);
        }
    }
}
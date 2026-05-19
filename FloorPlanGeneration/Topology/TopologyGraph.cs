using System.Collections.Generic;

namespace FloorPlanGeneration.Topology
{
    public sealed class TopologyGraph
    {
        public TopologyGraph()
        {
            Nodes = new List<SpaceNode>();
            Edges = new List<AdjacencyEdge>();
        }

        public List<SpaceNode> Nodes { get; set; }
        public List<AdjacencyEdge> Edges { get; set; }

        public void AddNode(string id, string kind, string referenceId, string parentId = "")
        {
            Nodes.Add(new SpaceNode
            {
                Id = id,
                Kind = kind,
                ReferenceId = referenceId,
                ParentId = parentId
            });
        }

        public void AddEdge(string from, string to, string kind, string reason = "")
        {
            Edges.Add(new AdjacencyEdge
            {
                From = from,
                To = to,
                Kind = kind,
                Reason = reason
            });
        }
    }

    public sealed class SpaceNode
    {
        public SpaceNode()
        {
            Id = string.Empty;
            Kind = string.Empty;
            ReferenceId = string.Empty;
            ParentId = string.Empty;
        }

        public string Id { get; set; }
        public string Kind { get; set; }
        public string ReferenceId { get; set; }
        public string ParentId { get; set; }
    }

    public sealed class AdjacencyEdge
    {
        public AdjacencyEdge()
        {
            From = string.Empty;
            To = string.Empty;
            Kind = string.Empty;
            Reason = string.Empty;
        }

        public string From { get; set; }
        public string To { get; set; }
        public string Kind { get; set; }
        public string Reason { get; set; }
    }
}

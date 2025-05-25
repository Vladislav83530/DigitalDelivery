import { GraphEdge } from "./graph-endge.model";
import { GraphNode } from "./graph-node.model";

export interface GraphData {
    nodes: GraphNode[];
    edges: GraphEdge[];
}
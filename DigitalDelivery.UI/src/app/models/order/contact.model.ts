import { GraphNode } from "../map/graph-node.model";
import { Address } from "./address.model";

export interface Contact {
    phoneNumber: string;
    firstName: string;
    lastName: string;
    address: GraphNode;
}


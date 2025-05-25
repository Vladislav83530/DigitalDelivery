import { Component, OnInit } from "@angular/core";
import { MapComponent } from "../map/map.component";
import { MapService } from "../../services/map.service";
import { GraphData } from "../../models/map/graph-data.model";
import { NgIf } from "@angular/common";

@Component({
    selector: 'app-graph',
    standalone: true,
    imports: [MapComponent, NgIf],
    templateUrl: './graph.component.html',
    styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit {
    public graphData: GraphData | null = null;

    constructor(private mapService: MapService) {}

    ngOnInit(): void {
        this.loadGraph();
    }

    private loadGraph(): void {
        this.mapService.getGraph().subscribe({
            next: (data: GraphData) => {
                this.graphData = data;
                console.log(this.graphData);
            },
            error: (error) => {
                console.error('Error loading graph:', error);
            }
        });
    }
}
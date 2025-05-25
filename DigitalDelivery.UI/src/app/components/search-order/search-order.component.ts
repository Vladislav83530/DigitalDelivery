import { Component } from '@angular/core';
import { RouteService } from '../../services/route-service';
import { GraphData } from "../../models/map/graph-data.model";
import { MapComponent } from '../map/map.component';
import { NgIf } from '@angular/common';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order/order.model';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import ValidateForm from '../../helpers/validate-form';
import { NgToastModule, NgToastService } from 'ng-angular-popup';
import { Router } from '@angular/router';

@Component({
  selector: 'app-search-order',
  standalone: true,
  imports: [MapComponent, NgIf, ReactiveFormsModule, NgToastModule],
  templateUrl: './search-order.component.html',
  styleUrl: './search-order.component.css'
})
export class SearchOrderComponent {
    public searchOrderForm!: FormGroup;
    public order: Order | null = null;

    constructor(
        private orderService: OrderService,
        private formBuider: FormBuilder,
        private toast: NgToastService,
        private router: Router
    ) {
        this.searchOrderForm = this.formBuider.group({
            orderNumber: ['', Validators.required]
        });
    }

    ngOnInit(): void {
    }
    onSearchOrder(): void {
        if (this.searchOrderForm.valid) {
            this.orderService.getOrder(this.searchOrderForm.value.orderNumber).subscribe({
                next: (data: Order) => {
                    this.order = data;
                    this.router.navigate(['/order'], { state: { orderData: data } });
                    this.toast.success('Order found successfully!', 'Success', 5000);
                },
                error: (error) => {
                    this.toast.danger('Oops! Something went wrong. Please try again.', 'Error', 5000);
                }
            });
        }
        else {
            ValidateForm.validateAllFormsField(this.searchOrderForm);
            this.toast.info('Please check your input and try again.', 'Info', 5000);
        }
    }

    showInputValidation(fieldName: string) {
        return this.searchOrderForm.controls[fieldName].dirty && this.searchOrderForm.hasError('required', fieldName)
    }
}

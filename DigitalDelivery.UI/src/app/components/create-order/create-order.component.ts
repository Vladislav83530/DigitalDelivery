import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MapComponent } from '../map/map.component';
import { Address } from '../../models/create-order/address.model';
import { OrderService } from '../../services/order.service';
import { NgToastService } from 'ng-angular-popup';
import { Router } from '@angular/router';
import { formatUAPhoneNumber } from '../../utils/phone-utils';

@Component({
    selector: 'app-create-order',
    standalone: true,
    imports: [ReactiveFormsModule, NgIf, MapComponent],
    templateUrl: './create-order.component.html'
})
export class CreateOrderComponent {
    createOrderForm: FormGroup;
    pickupCoordinates: Address | null = null;
    deliveryCoordinates: Address | null = null;
    formSubmitted = false;

    constructor(
        private orderService: OrderService,
        private toast: NgToastService,
        private router: Router
    ) {
        this.createOrderForm = new FormGroup({
            phone: new FormControl('', [Validators.required]),
            weight: new FormControl('', [Validators.required, Validators.min(0.1)]),
            width: new FormControl('', [Validators.required, Validators.min(1)]),
            height: new FormControl('', [Validators.required, Validators.min(1)]),
            depth: new FormControl('', [Validators.required, Validators.min(1)]),
            pickupAddress: new FormControl('', [Validators.required]),
            deliveryAddress: new FormControl('', [Validators.required])
        });
    }

    showInputValidation(fieldName: string): boolean {
        const control = this.createOrderForm.controls[fieldName];
        return (control.dirty || control.touched || this.formSubmitted) && 
               (control.hasError('required') || control.hasError('min'));
    }

    onPhoneInput(): void {
        const control = this.createOrderForm.controls['phone'];
        const formatted = formatUAPhoneNumber(control.value);
        control.setValue(formatted, { emitEvent: false });
    }

    onAddressesChange(addresses: any): void {
        if (addresses) {
            this.pickupCoordinates = addresses.pickup;
            this.deliveryCoordinates = addresses.delivery;
            
            this.createOrderForm.patchValue({
                pickupAddress: addresses.pickupAddress,
                deliveryAddress: addresses.deliveryAddress
            });
            
            this.createOrderForm.get('pickupAddress')?.markAsTouched();
            this.createOrderForm.get('deliveryAddress')?.markAsTouched();
        }
    }

    onSubmit(): void {
        this.formSubmitted = true;
        
        Object.keys(this.createOrderForm.controls).forEach(key => {
            this.createOrderForm.get(key)?.markAsTouched();
        });
        
        if (this.createOrderForm.valid && this.pickupCoordinates && this.deliveryCoordinates) {
            const orderData = this.GetCreateOrderRequest();
            
            this.orderService.createOrder(orderData).subscribe({
                next: (response) => {
                    if (response.success) {
                        this.toast.success('Order created successfully! 🎉', 'Success', 5000);
                        this.createOrderForm.reset();
                        this.router.navigate(['/order', response.data]);
                    } else {
                        this.toast.danger(response.message || 'Failed to create order', 'Error', 5000);
                    }
                },
                error: (error) => {
                    this.toast.danger('An error occurred while creating the order. Please try again.', 'Error', 5000);
                }
            });
        }
    }

    private GetCreateOrderRequest(): any {
        return {
            recipientPhoneNumber: this.createOrderForm.value.phone,
            pickupAddress: this.pickupCoordinates,
            deliveryAddress: this.deliveryCoordinates,
            packageDetails: {
                weightKg: this.createOrderForm.value.weight,
                widthCm: this.createOrderForm.value.width,
                heightCm: this.createOrderForm.value.height,
                depthCm: this.createOrderForm.value.depth
            }
        }
    }
}

import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MapComponent } from '../map/map.component';
import { Address } from '../../models/create-order/address.model';
import { OrderService } from '../../services/order.service';
import { NgToastService } from 'ng-angular-popup';
import { Router } from '@angular/router';

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

    formatPhoneNumber(): void {
        let value = this.createOrderForm.controls['phone'].value.replace(/\D/g, '');
        
        if (value.startsWith('380')) {
            value = '+' + value;
        } else if (value.startsWith('0')) {
            value = '+380' + value.slice(1);
        } else {
            value = '+380';
        }
      
        if (value.length > 4) value = value.slice(0, 4) + ' (' + value.slice(4);
        if (value.length > 8) value = value.slice(0, 8) + ') ' + value.slice(8);
        if (value.length > 13) value = value.slice(0, 13) + '-' + value.slice(13);
        if (value.length > 16) value = value.slice(0, 16) + '-' + value.slice(16);
      
        this.createOrderForm.controls['phone'].setValue(value.slice(0, 19), { emitEvent: false });
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
                        this.router.navigate(['/']);
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

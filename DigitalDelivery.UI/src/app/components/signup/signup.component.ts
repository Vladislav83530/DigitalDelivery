import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import ValidateForm from '../helpers/validate-form';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './signup.component.html',
  styleUrl: '../../../assets/css/common-auth.css'
})
export class SignupComponent {
    passwordInputType: string = 'password';
    eyeIcon: string = 'fa-eye-slash';
    isText: boolean = false;
    signupForm!: FormGroup;
    
    constructor(private formBuider: FormBuilder) {
        this.signupForm = this.formBuider.group({
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            email: ['', Validators.required],
            password: ['', Validators.required]
        });
    }

    hideShowPass(): void {
        this.isText = !this.isText;
        this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
        this.passwordInputType = this.isText ? 'text' : 'password';
    }

    onSubmit() {
            if (this.signupForm.valid) {
    
            }
            else {
                ValidateForm.validateAllFormsField(this.signupForm);
                alert("Your form is invalid");
            }
        }
    
    showInputValidation(fieldName: string) {
        return this.signupForm.controls[fieldName].dirty && this.signupForm.hasError('required', fieldName)
    }
}

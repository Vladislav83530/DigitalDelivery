import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import ValidateForm from '../helpers/validate-form';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './login.component.html',
  styleUrl: '../../../assets/css/common-auth.css'
})
export class LoginComponent {
    passwordInputType: string = 'password';
    eyeIcon: string = 'fa-eye-slash';
    isText: boolean = false;
    loginForm!: FormGroup;

    constructor(private formBuider: FormBuilder) {
        this.loginForm = this.formBuider.group({
            email: ['', Validators.required],
            password: ['', Validators.required]
        });
    }

    hideShowPass() {
        this.isText = !this.isText;
        this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
        this.passwordInputType = this.isText ? 'text' : 'password';
    }

    onSubmit() {
        if (this.loginForm.valid) {

        }
        else {
            ValidateForm.validateAllFormsField(this.loginForm);
            alert("Your form is invalid");
        }
    }

    showInputValidation(fieldName: string) {
        return this.loginForm.controls[fieldName].dirty && this.loginForm.hasError('required', fieldName)
    }
}

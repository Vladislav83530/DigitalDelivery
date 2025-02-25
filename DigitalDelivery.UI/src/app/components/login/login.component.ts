import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import ValidateForm from '../../helpers/validate-form';
import { AuthService } from '../../services/auth.service';

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

    constructor(
        private formBuider: FormBuilder,
        private authService: AuthService) 
    {
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

    onLogin() {
        if (this.loginForm.valid) {
            this.authService.login(this.loginForm.value).subscribe({
                next: (result) => {
                    alert('Welcome back! 🎉 You have successfully logged in.');
                },
                error: (error) => {
                    alert('Oops! Something went wrong. Please check your credentials and try again.');
                }
            })
        }
        else {
            ValidateForm.validateAllFormsField(this.loginForm);
            alert("Please check your input and try again.");
        }
    }
    

    showInputValidation(fieldName: string) {
        return this.loginForm.controls[fieldName].dirty && this.loginForm.hasError('required', fieldName)
    }
}

import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import ValidateForm from '../../helpers/validate-form';
import { AuthService } from '../../services/auth.service';
import { NgToastModule, NgToastService } from 'ng-angular-popup';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgToastModule],
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
        private authService: AuthService,
        private toast: NgToastService) 
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
                    if (result.success) 
                    {
                        this.authService.storeToken(result.data.accessToken);
                        this.authService.storeRefreshToken(result.data.refreshToken);
                        this.toast.success('Welcome back! 🎉 You have successfully logged in.', 'Success', 5000);
                    }
                    else {
                        this.toast.danger(result.message, 'Error', 5000);
                    }
                },
                error: (error) => {
                    this.toast.danger('Oops! Something went wrong. Please check your credentials and try again.', 'Error', 5000);
                }
            })
        }
        else {
            ValidateForm.validateAllFormsField(this.loginForm);
            this.toast.info('Please check your input and try again.', 'Info', 5000);
        }
    }
    

    showInputValidation(fieldName: string) {
        return this.loginForm.controls[fieldName].dirty && this.loginForm.hasError('required', fieldName)
    }
}

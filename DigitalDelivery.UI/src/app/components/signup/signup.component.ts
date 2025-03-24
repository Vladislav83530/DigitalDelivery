import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import ValidateForm from '../../helpers/validate-form';
import { NgIf } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { NgToastModule, NgToastService } from 'ng-angular-popup';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgToastModule],
  templateUrl: './signup.component.html',
  styleUrl: '../../../assets/css/common-auth.css'
})
export class SignupComponent {
    passwordInputType: string = 'password';
    eyeIcon: string = 'fa-eye-slash';
    isText: boolean = false;
    signupForm!: FormGroup;
    
    constructor(
        private formBuider: FormBuilder,
        private authService: AuthService,
        private router: Router,
        private toast: NgToastService) 
    {
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

    onSignup() {
        if (this.signupForm.valid) {
            this.authService.signUp(this.signupForm.value).subscribe({
                next: (result) => {
                    if (result.success) {
                        this.toast.success('Registration successful! 🎉', 'Success', 5000);
                        this.signupForm.reset();
                        this.router.navigate(['login']);
                    }
                    else {
                        this.toast.danger(result.message, 'Error', 5000);
                    }
                },
                error: () => {
                    this.toast.danger('Oops! Something went wrong. Please check your credentials and try again.', 'Error', 5000);
                }
            })
        }
        else {
            ValidateForm.validateAllFormsField(this.signupForm);
            this.toast.info('Please check your input and try again.', 'Info', 5000);
        }
    }
    
    showInputValidation(fieldName: string) {
        return this.signupForm.controls[fieldName].dirty && this.signupForm.hasError('required', fieldName)
    }
}

import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import ValidateForm from '../../helpers/validate-form';
import { NgIf } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

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
    
    constructor(
        private formBuider: FormBuilder,
        private authService: AuthService,
        private router: Router) 
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
                next: () => {
                    alert('Registration successful! 🎉');
                    this.signupForm.reset();
                    this.router.navigate(['login']);
                },
                error: () => {
                    alert('Oops! Something went wrong. Please try again or contact support.');
                }
            })
        }
        else {
            ValidateForm.validateAllFormsField(this.signupForm);
            alert('Please check your input and try again.');
        }
    }
    
    showInputValidation(fieldName: string) {
        return this.signupForm.controls[fieldName].dirty && this.signupForm.hasError('required', fieldName)
    }
}

import { Component } from '@angular/core';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [],
  templateUrl: './signup.component.html',
  styleUrl: '../../../assets/css/common-auth.css'
})
export class SignupComponent {
    public passwordInputType: string = 'password';
    public eyeIcon: string = 'fa-eye-slash';
    public isText: boolean = false;

    hideShowPass(): void {
        this.isText = !this.isText;
        this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
        this.passwordInputType = this.isText ? 'text' : 'password';
    }
}

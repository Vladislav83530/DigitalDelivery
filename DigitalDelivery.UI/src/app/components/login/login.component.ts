import { Component } from '@angular/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: '../../../assets/css/common-auth.css'
})
export class LoginComponent {
    public passwordInputType: string = 'password';
    public eyeIcon: string = 'fa-eye-slash';
    public isText: boolean = false;

    hideShowPass(): void {
        this.isText = !this.isText;
        this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
        this.passwordInputType = this.isText ? 'text' : 'password';
    }
}

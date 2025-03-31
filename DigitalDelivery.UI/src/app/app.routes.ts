import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { AuthGuard } from './guards/auth.guard';
import { MainPageComponent } from './components/main-page/main-page.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'signup', component: SignupComponent },
    { path: 'account', component: UserProfileComponent, canActivate: [AuthGuard]},
    { path: '**', component: MainPageComponent, canActivate: [AuthGuard]}
];

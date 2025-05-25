import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { AuthGuard } from './guards/auth.guard';
import { MainPageComponent } from './components/main-page/main-page.component';
import { CreateOrderComponent } from './components/create-order/create-order.component';
import { GraphComponent } from './components/graph/graph.component';
import { SearchOrderComponent } from './components/search-order/search-order.component';
import { OrderComponent } from './components/order/order.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { FaqComponent } from './components/faq/faq.component';

export const routes: Routes = [
    { path: '', component: MainPageComponent },
    { path: 'login', component: LoginComponent },
    { path: 'signup', component: SignupComponent },
    { path: 'account', component: UserProfileComponent, canActivate: [AuthGuard]},
    { path: 'create-order', component: CreateOrderComponent, canActivate: [AuthGuard]},
    { path: 'search-order', component: SearchOrderComponent, canActivate: [AuthGuard]},
    { path: 'order/:orderNumber', component: OrderComponent, canActivate: [AuthGuard]},
    { path: 'order', component: OrderComponent, canActivate: [AuthGuard]},
    { path: 'graph', component: GraphComponent, canActivate: [AuthGuard]},
    { path: 'not-found', component: NotFoundComponent },
    { path: 'faq', component: FaqComponent },
    { path: '**', component: MainPageComponent }
];

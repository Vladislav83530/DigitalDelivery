import { NgIf } from '@angular/common';
import { Component, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [RouterModule, NgIf],
  templateUrl: './main-page.component.html',
  styleUrl: './main-page.component.css'
})
export class MainPageComponent implements OnInit {
    isLoggedIn: boolean = false;
    private platformId = inject(PLATFORM_ID);

    constructor(private router: Router) {}
    
    ngOnInit() {
        this.checkLoginStatus();
    }

    private checkLoginStatus() {
        if (isPlatformBrowser(this.platformId)) {
            const token = localStorage.getItem('token');
            this.isLoggedIn = !!token;
        }
    }
    
    logout() {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem('token');
            this.isLoggedIn = false;
        }
    }
}

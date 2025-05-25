import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-not-found',
    standalone: true,
    imports: [RouterLink],
    template: `
        <div class="min-vh-100 d-flex align-items-center">
            <div class="container text-center">
                <div class="robot-container">
                    <div class="robot">
                        <!-- Robot Head -->
                        <div class="robot-head">
                            <div class="eye left"></div>
                            <div class="eye right"></div>
                            <div class="mouth"></div>
                        </div>
                        
                        <!-- Robot Body -->
                        <div class="robot-body">
                            <div class="display">404</div>
                            <div class="delivery-box"></div>
                        </div>
                        
                        <!-- Robot Arms -->
                        <div class="arm left"></div>
                        <div class="arm right"></div>
                        
                        <!-- Robot Base -->
                        <div class="robot-base">
                            <div class="wheel left"></div>
                            <div class="wheel right"></div>
                        </div>
                    </div>
                </div>
                <h1 class="display-1">404</h1>
                <h2 class="mb-4">Page Not Found</h2>
                <p class="lead mb-4">The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.</p>
                <a routerLink="/search-order" class="btn btn-primary">Go to Search</a>
            </div>
        </div>
    `,
    styles: [`
        .display-1 {
            color: #dc3545;
            font-weight: bold;
        }

        .robot-container {
            margin: 50px auto;
            width: 200px;
            height: 300px;
            position: relative;
        }

        .robot {
            position: relative;
            width: 100%;
            height: 100%;
            animation: float 3s ease-in-out infinite;
        }

        @keyframes float {
            0%, 100% { transform: translateY(0); }
            50% { transform: translateY(-20px); }
        }

        .robot-head {
            position: absolute;
            top: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 100px;
            height: 100px;
            background: #dc3545;
            border-radius: 20px;
        }

        .eye {
            position: absolute;
            width: 20px;
            height: 20px;
            background: white;
            border-radius: 50%;
            top: 30px;
        }

        .eye.left { left: 20px; }
        .eye.right { right: 20px; }

        .mouth {
            position: absolute;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%);
            width: 40px;
            height: 10px;
            background: white;
            border-radius: 5px;
        }

        .robot-body {
            position: absolute;
            top: 110px;
            left: 50%;
            transform: translateX(-50%);
            width: 120px;
            height: 120px;
            background: #dc3545;
            border-radius: 20px;
        }

        .display {
            position: absolute;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            width: 60px;
            height: 30px;
            background: #dc3545;
            border: 2px solid white;
            border-radius: 5px;
            color: white;
            font-weight: bold;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .delivery-box {
            position: absolute;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%);
            width: 60px;
            height: 40px;
            background: #dc3545;
            border: 2px solid white;
            border-radius: 5px;
        }

        .arm {
            position: absolute;
            width: 20px;
            height: 60px;
            background: #dc3545;
            border-radius: 10px;
            top: 130px;
        }

        .arm.left {
            left: 30px;
            transform: rotate(30deg);
        }

        .arm.right {
            right: 30px;
            transform: rotate(-30deg);
        }

        .robot-base {
            position: absolute;
            bottom: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 140px;
            height: 20px;
            background: #dc3545;
            border-radius: 10px;
        }

        .wheel {
            position: absolute;
            width: 30px;
            height: 30px;
            background: #666;
            border-radius: 50%;
            bottom: -15px;
        }

        .wheel.left { left: 20px; }
        .wheel.right { right: 20px; }

        .wheel::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 15px;
            height: 15px;
            background: #dc3545;
            border-radius: 50%;
        }

        .wheel {
            animation: rotate 3s linear infinite;
        }

        @keyframes rotate {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }
    `]
})
export class NotFoundComponent {} 
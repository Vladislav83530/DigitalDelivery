import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  templateUrl: './user-profile.component.html',
})
export class UserProfileComponent implements OnInit {

    constructor(private accountService: AccountService) {}

    ngOnInit(): void {
        this.getAccount();
    }

    getAccount() {
        this.accountService.getAccount().subscribe((result: any)=> {
            console.log(result.data)
        });
    }
}

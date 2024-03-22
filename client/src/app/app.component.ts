import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './models/user';
import { AccountService } from './services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(private accountService: AccountService){}

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser(){
    let user: User;
    let parsedUser = localStorage.getItem('user')??null;
    if(parsedUser){
      user = JSON.parse(parsedUser);
      this.accountService.setCurrentUser(user);
    }
  }
}
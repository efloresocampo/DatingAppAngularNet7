import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, ReplaySubject } from 'rxjs';
import { User } from '../models/user';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  currentUser$ = new ReplaySubject<User | null>(1);

  constructor(private http: HttpClient) { }
  login(model: User) {
    return this.http.post<User>(this.baseUrl + 'account/login', model)
      .pipe(map((response: User) => {
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      }));
  }

  register(model: User) {
    return this.http
      .post<User>(this.baseUrl + 'account/register', model)
      .pipe(map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
        }
      }));
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser$.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser$.next(null);
  }
}

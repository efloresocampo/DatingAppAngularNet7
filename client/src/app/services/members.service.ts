import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { of } from 'rxjs/internal/observable/of';
import { map } from 'rxjs';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baserUrl: string = environment.apiUrl;
  members: Member[] = [];
  constructor(private http: HttpClient) { }

  getMembers(userParams: UserParams) {
    let params = this.getPaginationHeaders(userParams);
    params = params.append("minAge", userParams.minAge);
    params = params.append("maxAge", userParams.maxAge);
    params = params.append("gender", userParams.gender);
    params = params.append("orderBy", userParams.orderBy);
    return this.getPaginatedResult<Member[]>(this.baserUrl + 'users', params);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T>
      = new PaginatedResult<T>();
    return this.http.get<T>(url, {
      observe: 'response',
      params
    }).pipe(
      map(response => {
        if (response.body) {
          paginatedResult.result = response.body;
        }
        const pagination = response.headers.get("Pagination");
        if (pagination) {
          paginatedResult.pagination = JSON.parse(pagination);
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(userParams: UserParams) {
    let params = new HttpParams();
    if (userParams.pageNumber && userParams.pageSize) {
      params = params.append("pageNumber", userParams.pageNumber);
      params = params.append("pageSize", userParams.pageSize);
    }
    return params;
  }

  getMember(username: string) {
    const member = this.members
      .find(member => member.userName === username);
    if (member) {
      return of(member);
    }
    return this.http.get<Member>(this.baserUrl + "users/" + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baserUrl + 'users', member)
      .pipe(map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = {
          ...this.members[index], ...member
        };
      }));
  }

  setMainPhoto(photoId: number) {
    return this.http
      .put(this.baserUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baserUrl + 'users/delete-photo/' + photoId);
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GetCatalogResult } from '@app/shared/models/get-catalog-result';
import { Result } from '@app/shared/models/result';
import { environment } from '@environments/environment';
import { Observable, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  readonly apiUrl: string = environment.apiUrl;
  readonly production: boolean = environment.production;

  constructor(private http: HttpClient) {}

  getCatalog(): Observable<Result<GetCatalogResult>> {
    return this.http.get<Result<GetCatalogResult>>(
      `${this.apiUrl}/api/catalog?pageIndex=1&pageSize=10`
    );
  }
}

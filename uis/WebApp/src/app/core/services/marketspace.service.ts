import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {GetCatalogResult} from '@app/shared/models/get-catalog-result';
import {Result} from '@app/shared/models/result';
import {environment} from '@environments/environment';
import {Observable, switchMap} from 'rxjs';
import {CartHandlerRequest} from '@app/shared/models/cart-handler-request';
import {CartHandlerResponse} from '@app/shared/models/cart-handler-response';

@Injectable({
  providedIn: 'root',
})
export class MarketSpaceService {
  readonly apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {
  }

  getCatalog(pageIndex: number = 1, pageSize: number = 10): Observable<Result<GetCatalogResult>> {
    return this.http.get<Result<GetCatalogResult>>(
      `${this.apiUrl}/api/catalog?pageIndex=${pageIndex}&pageSize=${pageSize}`
    );
  }

  cartHandler(request: CartHandlerRequest): Observable<Result<CartHandlerResponse>> {
    return this.http.post<Result<CartHandlerResponse>>(`${this.apiUrl}/api/basket`, request);
  }
}

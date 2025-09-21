import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Customer,
  CreateCustomerRequest,
  UpdateCustomerRequest,
} from '../models/customer.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  constructor(private http: HttpClient) {}

  getCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${environment.apiUrl}/customers`);
  }

  getCustomer(id: number): Observable<Customer> {
    return this.http.get<Customer>(`${environment.apiUrl}/customers/${id}`);
  }

  createCustomer(customer: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(
      `${environment.apiUrl}/customers`,
      customer
    );
  }

  updateCustomer(
    id: number,
    customer: UpdateCustomerRequest
  ): Observable<Customer> {
    return this.http.put<Customer>(
      `${environment.apiUrl}/customers/${id}`,
      customer
    );
  }

  deleteCustomer(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/customers/${id}`);
  }

  getPurchaseHistory(customerId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${environment.apiUrl}/customers/${customerId}/purchase-history`
    );
  }

  addLoyaltyPoints(customerId: number, points: number): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}/customers/${customerId}/add-loyalty-points`,
      points
    );
  }
}

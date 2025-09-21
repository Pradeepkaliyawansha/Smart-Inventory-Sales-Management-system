import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
} from '../models/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${environment.apiUrl}/products`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${environment.apiUrl}/products/${id}`);
  }

  createProduct(product: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(`${environment.apiUrl}/products`, product);
  }

  updateProduct(
    id: number,
    product: UpdateProductRequest
  ): Observable<Product> {
    return this.http.put<Product>(
      `${environment.apiUrl}/products/${id}`,
      product
    );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/products/${id}`);
  }

  getLowStockProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${environment.apiUrl}/products/low-stock`);
  }

  updateStock(
    productId: number,
    quantity: number,
    movementType: number,
    reference?: string
  ): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}/products/${productId}/update-stock`,
      {
        quantity,
        movementType,
        reference,
      }
    );
  }
}

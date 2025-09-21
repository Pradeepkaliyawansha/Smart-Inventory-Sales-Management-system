export interface Product {
  id: number;
  name: string;
  description?: string;
  sku: string;
  barcode: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  minStockLevel: number;
  categoryId: number;
  categoryName: string;
  supplierId: number;
  supplierName: string;
  imageUrl?: string;
  isActive: boolean;
  isLowStock: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  sku: string;
  barcode: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  minStockLevel: number;
  categoryId: number;
  supplierId: number;
  imageUrl?: string;
}

export interface UpdateProductRequest extends CreateProductRequest {}

export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface Supplier {
  id: number;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  isActive: boolean;
  createdAt: Date;
}

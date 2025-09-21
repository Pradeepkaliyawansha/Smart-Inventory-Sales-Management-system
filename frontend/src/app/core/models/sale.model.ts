export interface Sale {
  id: number;
  invoiceNumber: string;
  customerId: number;
  customerName: string;
  userId: number;
  salesPersonName: string;
  saleDate: Date;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  paymentMethod: PaymentMethod;
  notes?: string;
  isCompleted: boolean;
  saleItems: SaleItem[];
}

export interface SaleItem {
  id: number;
  saleId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  totalPrice: number;
}

export interface CreateSaleRequest {
  customerId: number;
  paymentMethod: PaymentMethod;
  notes?: string;
  saleItems: CreateSaleItemRequest[];
}

export interface CreateSaleItemRequest {
  productId: number;
  quantity: number;
  unitPrice: number;
  discountPercentage?: number;
}

export enum PaymentMethod {
  Cash = 1,
  Card = 2,
  BankTransfer = 3,
  Check = 4,
  Credit = 5,
}

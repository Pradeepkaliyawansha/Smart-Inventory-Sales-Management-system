export interface Customer {
  id: number;
  name: string;
  email?: string;
  phone?: string;
  address?: string;
  loyaltyPoints: number;
  creditBalance: number;
  isActive: boolean;
  createdAt: Date;
  lastPurchaseDate?: Date;
}

export interface CreateCustomerRequest {
  name: string;
  email?: string;
  phone?: string;
  address?: string;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {}

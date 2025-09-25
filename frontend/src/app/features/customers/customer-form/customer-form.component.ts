import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CustomerService } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './customer-form.component.html',
  styleUrls: ['./customer-form.component.scss'],
})
export class CustomerFormComponent implements OnInit {
  customerForm!: FormGroup;
  loading = false;
  isEditMode = false;
  customerId: number | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private customerService: CustomerService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.checkEditMode();
  }

  private createForm(): void {
    this.customerForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      phone: ['', Validators.maxLength(20)],
      address: ['', Validators.maxLength(255)],
    });
  }

  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.customerId = +id;
      this.loadCustomer(this.customerId);
    }
  }

  private loadCustomer(id: number): void {
    this.loading = true;
    this.customerService.getCustomer(id).subscribe({
      next: (customer) => {
        this.customerForm.patchValue({
          name: customer.name,
          email: customer.email,
          phone: customer.phone,
          address: customer.address,
        });
        this.loading = false;
      },
      error: () => {
        this.notificationService.showError('Error loading customer');
        this.router.navigate(['/customers']);
      },
    });
  }

  onSubmit(): void {
    if (this.customerForm.valid) {
      this.loading = true;
      const formData = this.customerForm.value;

      const request$ = this.isEditMode
        ? this.customerService.updateCustomer(this.customerId!, formData)
        : this.customerService.createCustomer(formData);

      request$.subscribe({
        next: () => {
          const message = this.isEditMode
            ? 'Customer updated successfully'
            : 'Customer created successfully';
          this.notificationService.showSuccess(message);
          this.router.navigate(['/customers']);
        },
        error: () => {
          const message = this.isEditMode
            ? 'Error updating customer'
            : 'Error creating customer';
          this.notificationService.showError(message);
          this.loading = false;
        },
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.customerForm.controls).forEach((field) => {
      const control = this.customerForm.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  onCancel(): void {
    this.router.navigate(['/customers']);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.customerForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    if (field?.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `${this.getFieldDisplayName(
        fieldName
      )} cannot exceed ${maxLength} characters`;
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      name: 'Customer Name',
      email: 'Email',
      phone: 'Phone',
      address: 'Address',
    };
    return displayNames[fieldName] || fieldName;
  }
}

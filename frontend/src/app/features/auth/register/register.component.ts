import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserRole } from '../../../core/models/user.model';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  loading = false;
  hidePassword = true;
  hideConfirmPassword = true;
  userRoles = UserRole;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    // If already logged in, redirect to dashboard
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnInit(): void {
    this.createForm();
  }

  private createForm(): void {
    this.registerForm = this.formBuilder.group(
      {
        fullName: [
          '',
          [
            Validators.required,
            Validators.minLength(2),
            Validators.maxLength(100),
          ],
        ],
        username: [
          '',
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(50),
            Validators.pattern(/^[a-zA-Z0-9_]+$/),
          ],
        ],
        email: [
          '',
          [Validators.required, Validators.email, Validators.maxLength(100)],
        ],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            this.strongPasswordValidator,
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        role: [UserRole.SalesStaff, [Validators.required]],
      },
      {
        validators: this.passwordMatchValidator,
      }
    );
  }

  // Custom validator for strong password
  private strongPasswordValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const password = control.value;
    if (!password) {
      return null;
    }

    const hasNumber = /[0-9]/.test(password);
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasSpecial = /[#?!@$%^&*-]/.test(password);

    const valid = hasNumber && hasUpper && hasLower && hasSpecial;

    if (!valid) {
      return { weakPassword: true };
    }

    return null;
  }

  // Custom validator for password confirmation
  private passwordMatchValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    // Clear the error if passwords match
    if (confirmPassword.errors?.['passwordMismatch']) {
      delete confirmPassword.errors['passwordMismatch'];
      if (Object.keys(confirmPassword.errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }

    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.loading = true;

      const registerRequest = {
        fullName: this.registerForm.value.fullName,
        username: this.registerForm.value.username,
        email: this.registerForm.value.email,
        password: this.registerForm.value.password,
        role: this.registerForm.value.role,
      };

      this.authService.register(registerRequest).subscribe({
        next: (response) => {
          this.notificationService.showSuccess(
            `Account created successfully! Welcome, ${response.user.fullName}!`
          );
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.loading = false;
          const errorMessage =
            error.error?.message || 'Registration failed. Please try again.';
          this.notificationService.showError(errorMessage);
        },
        complete: () => {
          this.loading = false;
        },
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach((field) => {
      const control = this.registerForm.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }

    if (field?.hasError('email')) {
      return 'Please enter a valid email address';
    }

    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `${this.getFieldDisplayName(
        fieldName
      )} must be at least ${minLength} characters`;
    }

    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `${this.getFieldDisplayName(
        fieldName
      )} cannot exceed ${maxLength} characters`;
    }

    if (field?.hasError('pattern')) {
      if (fieldName === 'username') {
        return 'Username can only contain letters, numbers, and underscores';
      }
    }

    if (field?.hasError('weakPassword')) {
      return 'Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character';
    }

    if (field?.hasError('passwordMismatch')) {
      return 'Passwords do not match';
    }

    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      fullName: 'Full Name',
      username: 'Username',
      email: 'Email',
      password: 'Password',
      confirmPassword: 'Confirm Password',
      role: 'Role',
    };
    return displayNames[fieldName] || fieldName;
  }

  getRoleDisplayName(role: UserRole): string {
    const roleNames: { [key: number]: string } = {
      [UserRole.Admin]: 'Administrator',
      [UserRole.Manager]: 'Manager',
      [UserRole.SalesStaff]: 'Sales Staff',
    };
    return roleNames[role] || 'Unknown';
  }
}

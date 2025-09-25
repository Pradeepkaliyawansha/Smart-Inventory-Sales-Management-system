import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmationData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'warning' | 'error' | 'info';
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h1 mat-dialog-title>
      <mat-icon *ngIf="data.type" [class]="'dialog-icon ' + data.type">
        {{ getIcon() }}
      </mat-icon>
      {{ data.title }}
    </h1>

    <div mat-dialog-content>
      <p>{{ data.message }}</p>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">
        {{ data.cancelText || 'Cancel' }}
      </button>
      <button
        mat-raised-button
        [color]="data.type === 'error' ? 'warn' : 'primary'"
        (click)="onConfirm()"
      >
        {{ data.confirmText || 'Confirm' }}
      </button>
    </div>
  `,
  styles: [
    `
      .dialog-icon {
        margin-right: 8px;
        vertical-align: middle;
      }
      .dialog-icon.warning {
        color: #ff9800;
      }
      .dialog-icon.error {
        color: #f44336;
      }
      .dialog-icon.info {
        color: #2196f3;
      }
    `,
  ],
})
export class ConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      case 'info':
        return 'info';
      default:
        return 'help';
    }
  }
}

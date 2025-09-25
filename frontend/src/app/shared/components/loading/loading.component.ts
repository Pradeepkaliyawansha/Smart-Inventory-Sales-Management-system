import { Component, Input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="loading-container">
      <mat-progress-spinner
        [diameter]="diameter"
        [strokeWidth]="strokeWidth"
        mode="indeterminate"
      >
      </mat-progress-spinner>
      <p *ngIf="message" class="loading-message">{{ message }}</p>
    </div>
  `,
  styles: [
    `
      .loading-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 20px;
      }

      .loading-message {
        margin-top: 16px;
        color: #666;
        font-size: 0.9rem;
      }
    `,
  ],
})
export class LoadingComponent {
  @Input() diameter = 40;
  @Input() strokeWidth = 4;
  @Input() message?: string;
}

import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  template: `
    <div class="loading-container">
      <mat-spinner
        [diameter]="diameter"
        [strokeWidth]="strokeWidth"
      ></mat-spinner>
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

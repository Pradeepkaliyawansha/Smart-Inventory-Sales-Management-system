import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Angular Material (commonly used modules)
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';

// Shared Components
import { LoadingComponent } from './components/loading/loading.component';
import { ConfirmationDialogComponent } from './components/confirmation-dialog/confirmation-dialog.component';

// Pipes
import { CurrencyFormatPipe } from './pipes/currency-format.pipe';
import { DateFormatPipe } from './pipes/date-format.pipe';

@NgModule({
  declarations: [
    LoadingComponent,
    ConfirmationDialogComponent,
    CurrencyFormatPipe,
    DateFormatPipe,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,

    // Angular Material modules
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatGridListModule,
  ],
  exports: [
    // Export what other modules need
    CommonModule,
    FormsModule,
    ReactiveFormsModule,

    // Export commonly used Material modules
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatGridListModule,

    // Export our custom components and pipes
    LoadingComponent,
    CurrencyFormatPipe,
    DateFormatPipe,

    // Import and re-export standalone component
    ConfirmationDialogComponent,
  ],
})
export class SharedModule {}

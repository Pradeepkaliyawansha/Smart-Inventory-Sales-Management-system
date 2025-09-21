import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable({
  providedIn: 'root',
})
export class RoleGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const expectedRoles = route.data['expectedRoles'] as string[];
    const user = this.authService.getCurrentUser();

    if (!user) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    if (expectedRoles && expectedRoles.length > 0) {
      const hasRole = expectedRoles.some((role) =>
        this.authService.hasRole(role)
      );
      if (!hasRole) {
        this.notificationService.showError(
          'You do not have permission to access this page'
        );
        this.router.navigate(['/dashboard']);
        return false;
      }
    }

    return true;
  }
}

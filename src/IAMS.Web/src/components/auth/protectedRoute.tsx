// src/components/auth/ProtectedRoute.tsx
import React from 'react';
import { Route, Redirect, RouteProps } from 'react-router-dom';
import authService from '../../services/authService';

interface ProtectedRouteProps extends RouteProps {
    roles?: string[];
    permissions?: string[];
    moduleName?: string;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
    roles,
    permissions,
    moduleName,
    children,
    ...rest
}) => {
    const checkAccess = () => {
        // First check if user is authenticated
        if (!authService.isAuthenticated()) {
            return false;
        }

        // If no restrictions are specified, allow access
        if (!roles && !permissions && !moduleName) {
            return true;
        }

        // Check roles if specified
        if (roles && roles.length > 0) {
            if (!authService.hasAnyRole(roles)) {
                return false;
            }
        }

        // Check permissions if specified
        if (permissions && permissions.length > 0) {
            if (!authService.hasAnyPermission(permissions)) {
                return false;
            }
        }

        // Check module access if specified
        if (moduleName) {
            // This would need to be implemented separately to check if user's tenant
            // has the specified module enabled
            // You could add this to authService or create a separate moduleService
            const moduleService = {
                isModuleEnabled: (name: string) => {
                    // Implementation here - you would need to check with your API
                    // This is a placeholder
                    return true;
                }
            };

            if (!moduleService.isModuleEnabled(moduleName)) {
                return false;
            }
        }

        // If all checks pass, allow access
        return true;
    };

    return (
        <Route
      { ...rest }
      render = {({ location }) =>
checkAccess() ? (
    children
) : (
    <Redirect
            to= {{
    pathname: '/login',
        state: { from: location }
}}
/>
        )
      }
/>
  );
};

export default ProtectedRoute;
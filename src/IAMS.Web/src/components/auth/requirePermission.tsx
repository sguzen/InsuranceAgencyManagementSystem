// src/components/auth/RequirePermission.tsx
import React, { ReactNode } from 'react';
import authService from '../../services/authService';

interface RequirePermissionProps {
    permission: string;
    children: ReactNode;
    fallback?: ReactNode;
}

const RequirePermission: React.FC<RequirePermissionProps> = ({
    permission,
    children,
    fallback = null
}) => {
    if (authService.hasPermission(permission)) {
        return <>{children}</>;
    }

    return <>{fallback}</>;
};

export default RequirePermission;
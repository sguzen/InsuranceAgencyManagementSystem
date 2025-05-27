// src/components/auth/RequireModule.tsx
import React, { ReactNode } from 'react';

interface RequireModuleProps {
    moduleName: string;
    children: ReactNode;
    fallback?: ReactNode;
}

const RequireModule: React.FC<RequireModuleProps> = ({
    moduleName,
    children,
    fallback = null
}) => {
    // You would need to implement moduleService to check if the module is enabled
    const moduleService = {
        isModuleEnabled: (name: string) => {
            // Implementation here - you would need to check with your API
            // This is a placeholder
            return true;
        }
    };

    if (moduleService.isModuleEnabled(moduleName)) {
        return <>{children}</>;
    }

    return <>{fallback}</>;
};

export default RequireModule;
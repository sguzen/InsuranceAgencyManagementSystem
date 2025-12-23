# Kubernetes Deployment Guide for IAMS

This directory contains Kubernetes manifests for deploying IAMS to a production Kubernetes cluster.

## Prerequisites

1. **Kubernetes Cluster** (v1.24+)
   - Azure Kubernetes Service (AKS)
   - Google Kubernetes Engine (GKE)
   - Amazon EKS
   - Self-managed Kubernetes

2. **Tools Required**
   ```bash
   # kubectl (Kubernetes CLI)
   kubectl version

   # Helm (optional, for installing dependencies)
   helm version

   # cert-manager (for SSL certificates)
   kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
   ```

3. **NGINX Ingress Controller**
   ```bash
   helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
   helm repo update
   helm install ingress-nginx ingress-nginx/ingress-nginx \
     --namespace ingress-nginx \
     --create-namespace \
     --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
   ```

## Quick Start

### 1. Create Namespace
```bash
kubectl apply -f namespace.yml
```

### 2. Create Secrets
```bash
# Copy example secrets file
cp secrets.example.yml secrets.yml

# Edit secrets.yml with your actual values (base64 encoded)
# IMPORTANT: Never commit secrets.yml to Git!

# Apply secrets
kubectl apply -f secrets.yml
```

### 3. Create ConfigMap
```bash
kubectl apply -f configmap.yml
```

### 4. Create Image Pull Secret (if using private registry)
```bash
kubectl create secret docker-registry acr-secret \
  --namespace=iams-production \
  --docker-server=acriamsprod.azurecr.io \
  --docker-username=<your-username> \
  --docker-password=<your-password>
```

### 5. Deploy Application
```bash
# Deploy API
kubectl apply -f api-deployment.yml

# Deploy Web
kubectl apply -f web-deployment.yml

# Deploy Ingress
kubectl apply -f ingress.yml

# Deploy Pod Disruption Budgets
kubectl apply -f poddisruptionbudget.yml

# Deploy Network Policies (optional but recommended)
kubectl apply -f networkpolicy.yml
```

### 6. Verify Deployment
```bash
# Check pods
kubectl get pods -n iams-production

# Check services
kubectl get svc -n iams-production

# Check ingress
kubectl get ingress -n iams-production

# Check HPA
kubectl get hpa -n iams-production

# View logs
kubectl logs -f deployment/iams-api -n iams-production
kubectl logs -f deployment/iams-web -n iams-production
```

## File Descriptions

| File | Description |
|------|-------------|
| `namespace.yml` | Creates the iams-production namespace |
| `configmap.yml` | Application configuration and settings |
| `secrets.example.yml` | Template for secrets (copy to secrets.yml) |
| `api-deployment.yml` | API deployment, service, HPA, and service account |
| `web-deployment.yml` | Web deployment, service, HPA, and service account |
| `ingress.yml` | Ingress configuration with SSL/TLS |
| `poddisruptionbudget.yml` | Pod disruption budgets for high availability |
| `networkpolicy.yml` | Network policies for security |

## Configuration

### Environment-Specific Configuration

Edit `configmap.yml` to customize for your environment:
- API URLs
- Feature flags
- Logging levels
- Cache settings

### Secrets Management

For production, consider using:
- **Azure Key Vault** with Azure Workload Identity
- **HashiCorp Vault**
- **AWS Secrets Manager**
- **External Secrets Operator**

Example with External Secrets Operator:
```yaml
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: iams-secrets
  namespace: iams-production
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: azure-keyvault
    kind: SecretStore
  target:
    name: iams-secrets
  data:
    - secretKey: ConnectionStrings__MasterConnection
      remoteRef:
        key: master-connection-string
```

## Scaling

### Manual Scaling
```bash
# Scale API
kubectl scale deployment iams-api --replicas=10 -n iams-production

# Scale Web
kubectl scale deployment iams-web --replicas=5 -n iams-production
```

### Auto-Scaling (HPA)
HPA is configured automatically in the deployment files. Adjust in `api-deployment.yml` and `web-deployment.yml`:
```yaml
spec:
  minReplicas: 3
  maxReplicas: 30
```

## Updates and Rollouts

### Rolling Update
```bash
# Update image
kubectl set image deployment/iams-api iams-api=acriamsprod.azurecr.io/iams-api:v2.0 -n iams-production

# Watch rollout
kubectl rollout status deployment/iams-api -n iams-production

# Rollback if needed
kubectl rollout undo deployment/iams-api -n iams-production
```

### Blue-Green Deployment
Use labels and services to implement blue-green deployments:
```bash
# Deploy new version with different label
kubectl apply -f api-deployment-blue.yml

# Test the new version
# Switch service selector to new version
kubectl patch service iams-api-service -p '{"spec":{"selector":{"version":"blue"}}}'

# Remove old deployment
kubectl delete -f api-deployment-green.yml
```

## Monitoring

### View Logs
```bash
# Real-time logs
kubectl logs -f deployment/iams-api -n iams-production

# Previous logs (if pod crashed)
kubectl logs deployment/iams-api --previous -n iams-production

# Logs from specific pod
kubectl logs iams-api-7d8f6d4c8-abcde -n iams-production
```

### Check Resources
```bash
# Resource usage
kubectl top pods -n iams-production
kubectl top nodes

# Events
kubectl get events -n iams-production --sort-by='.lastTimestamp'
```

### Health Checks
```bash
# Check pod health
kubectl describe pod <pod-name> -n iams-production

# Test health endpoint
kubectl port-forward deployment/iams-api 8080:8080 -n iams-production
curl http://localhost:8080/health
```

## Troubleshooting

### Pods Not Starting
```bash
# Check pod status
kubectl get pods -n iams-production

# Describe pod for events
kubectl describe pod <pod-name> -n iams-production

# Check logs
kubectl logs <pod-name> -n iams-production
```

### Image Pull Errors
```bash
# Verify image pull secret
kubectl get secret acr-secret -n iams-production

# Check if secret is attached to service account
kubectl get serviceaccount iams-api-sa -n iams-production -o yaml
```

### Database Connection Issues
```bash
# Verify secrets are set correctly
kubectl get secret iams-secrets -n iams-production -o jsonpath='{.data.ConnectionStrings__MasterConnection}' | base64 -d

# Test connection from pod
kubectl exec -it deployment/iams-api -n iams-production -- /bin/bash
# Inside pod:
curl -v telnet://your-sql-server.database.windows.net:1433
```

### SSL Certificate Issues
```bash
# Check certificate status
kubectl get certificate -n iams-production
kubectl describe certificate iams-tls-secret -n iams-production

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager
```

## Security Best Practices

1. **Use Non-Root Containers**: Already configured in deployment files
2. **Network Policies**: Limit pod-to-pod communication
3. **Pod Security Standards**: Apply pod security policies
4. **Secrets Management**: Use external secrets management
5. **RBAC**: Implement role-based access control
6. **Image Scanning**: Scan container images for vulnerabilities
7. **Resource Limits**: Always set resource limits to prevent resource exhaustion

## Cleanup

### Delete All Resources
```bash
# Delete deployments
kubectl delete -f api-deployment.yml
kubectl delete -f web-deployment.yml
kubectl delete -f ingress.yml
kubectl delete -f poddisruptionbudget.yml
kubectl delete -f networkpolicy.yml
kubectl delete -f configmap.yml
kubectl delete -f secrets.yml

# Delete namespace (will delete everything in it)
kubectl delete namespace iams-production
```

## Additional Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Azure AKS Documentation](https://docs.microsoft.com/en-us/azure/aks/)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/)
- [cert-manager](https://cert-manager.io/)
- [Horizontal Pod Autoscaler](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)

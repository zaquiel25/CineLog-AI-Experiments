# Application Insights Setup Guide

## ✅ Local Development - COMPLETED

Application Insights has been successfully integrated for local testing:

- **Package Added**: Microsoft.ApplicationInsights.AspNetCore v2.23.0
- **Configuration**: Uses placeholder connection string in appsettings.Development.json
- **Services**: CineLogTelemetryService configured for comprehensive monitoring
- **Health Checks**: Integrated with health monitoring endpoints
- **Testing**: Local server verified working at http://localhost:5059

## 📋 Production Setup - READY FOR DEPLOYMENT

### Step 1: Create Application Insights Resource in Azure

1. **In Azure Portal**:
   - Go to "Create a resource" → "Application Insights"
   - Name: `CineLog-AppInsights`
   - Resource Group: Same as your existing CineLog resources
   - Region: Same as your app (e.g., East US)
   - Resource Mode: Classic or Workspace-based

2. **Get Connection String**:
   - After creation, go to the Application Insights resource
   - Copy the "Connection String" (not Instrumentation Key)
   - Format: `InstrumentationKey=xxx;IngestionEndpoint=https://xxx;LiveEndpoint=https://xxx`

### Step 2: Add to Azure Key Vault

Add the connection string to your existing Key Vault:

```bash
# Using Azure CLI
az keyvault secret set \
  --vault-name "your-keyvault-name" \
  --name "ApplicationInsights--ConnectionString" \
  --value "InstrumentationKey=xxx;IngestionEndpoint=https://xxx;LiveEndpoint=https://xxx"
```

### Step 3: Deploy to Production

The application is already configured to:
- ✅ Read from Azure Key Vault automatically
- ✅ Use cost-optimized sampling (5 events/second max)
- ✅ Track custom CineLog metrics
- ✅ Monitor the 70-90% performance improvements
- ✅ Provide health check endpoints

## 🎯 What You'll Get

### Performance Monitoring
- Database query performance validation
- TMDB API response times
- Page load performance
- Cache hit rates

### User Analytics
- Authentication success rates
- Feature usage patterns
- Session duration tracking
- User journey mapping

### Business Intelligence
- Suggestion system effectiveness
- Popular movie categories
- User engagement metrics
- Error tracking and alerts

### Cost Optimization
- Adaptive sampling enabled
- Error and exception tracking prioritized
- Development vs production configurations

## 🔧 No Code Changes Needed

The integration is complete and production-ready. Just add the Application Insights connection string to Azure Key Vault and redeploy.

## 💰 Expected Costs

With current sampling configuration:
- **Free Tier**: 5GB/month (likely sufficient for CineLog)
- **Paid Tier**: ~$2-10/month depending on usage
- **Sampling**: Configured to minimize costs while preserving critical telemetry
# On-Premise Deployment Checklist
## For: elzem.websuresoft.com

---

## Pre-Deployment (Before Day 1)

### Server Preparation
- [ ] Server installed with Windows Server 2019/2022
- [ ] Server has static IP address: _______________
- [ ] Server has internet connectivity
- [ ] Remote Desktop enabled (for remote management)
- [ ] Administrator password set and documented

### Network & Domain
- [ ] DNS A record created: elzem.websuresoft.com → Server IP
- [ ] DNS propagation verified (use: `nslookup elzem.websuresoft.com`)
- [ ] Firewall allows incoming traffic on ports 80, 443
- [ ] Router port forwarding configured (if behind NAT)

### Software Downloads
- [ ] .NET 8.0 Hosting Bundle downloaded
- [ ] SQL Server 2022 Express downloaded
- [ ] SQL Server Management Studio (SSMS) downloaded
- [ ] IIS URL Rewrite Module downloaded
- [ ] win-acme (for SSL) downloaded

---

## Day 1: Installation & Configuration

### Morning (2-3 hours)

#### Step 1: Install .NET Runtime
- [ ] Install .NET 8.0 Hosting Bundle
- [ ] Restart server
- [ ] Verify: `dotnet --list-runtimes` shows .NET 8.0

#### Step 2: Install IIS
- [ ] Run automated script: `.\scripts\Setup-WindowsServer.ps1`
  - OR manually install IIS via Server Manager
- [ ] Verify IIS is running: Open http://localhost

#### Step 3: Install SQL Server
- [ ] Install SQL Server 2022 Express
- [ ] Enable Mixed Mode authentication
- [ ] Set SA password: _______________
- [ ] Enable TCP/IP protocol
- [ ] Restart SQL Server service
- [ ] Install SSMS

#### Step 4: Install URL Rewrite
- [ ] Install IIS URL Rewrite Module
- [ ] Restart IIS: `iisreset`

### Afternoon (2-3 hours)

#### Step 5: Create Databases
- [ ] Open SSMS, connect to localhost
- [ ] Run: `C:\Scripts\CreateDatabases.sql` (created by setup script)
  - OR manually create:
    - [ ] IAMS_Master database
    - [ ] IAMS_Tenant_elzem database
    - [ ] iams_app login
    - [ ] Grant permissions
- [ ] Test connection: `sqlcmd -S localhost -U iams_app -P [password]`

#### Step 6: Build Application (on dev machine)
- [ ] `dotnet publish src/IAMS.Api -c Release -o ./publish/api`
- [ ] `dotnet publish src/IAMS.Web -c Release -o ./publish/web`
- [ ] Zip published folders

### Evening (2-3 hours)

#### Step 7: Deploy Application
- [ ] Copy publish/api/* to `C:\inetpub\iams-api`
- [ ] Copy publish/web/* to `C:\inetpub\iams-web`
- [ ] Create `appsettings.Production.json` in both folders
- [ ] Update connection strings with SQL password
- [ ] Update JwtSettings__Secret (32+ chars)
- [ ] Update domain to elzem.websuresoft.com

#### Step 8: Configure IIS
- [ ] Verify Application Pools created (by setup script)
- [ ] Verify Sites created (by setup script)
- [ ] Test: `curl http://localhost:5000/health`
- [ ] Test: `curl http://localhost:5001/health`

---

## Day 2: SSL & Security

### Morning (1-2 hours)

#### Step 9: Install SSL Certificate
- [ ] Extract win-acme to `C:\win-acme`
- [ ] Run: `C:\win-acme\wacs.exe`
- [ ] Select option: Create certificate with advanced options
- [ ] Enter domain: elzem.websuresoft.com
- [ ] Choose HTTP validation
- [ ] Bind to IAMS sites
- [ ] Verify certificate installed

#### Step 10: Configure Production Site
- [ ] Stop sites: IAMS_API, IAMS_Web
- [ ] Create main site on port 443 with SSL
- [ ] Add virtual directory for /api
- [ ] Configure URL Rewrite for HTTP→HTTPS redirect
- [ ] Test: https://elzem.websuresoft.com

### Afternoon (1-2 hours)

#### Step 11: Run Database Migrations
- [ ] cd `C:\inetpub\iams-api`
- [ ] Set environment variable for connection string
- [ ] Run: `dotnet ef database update`
- [ ] Verify tables created in SSMS

#### Step 12: Create Initial Tenant
- [ ] Insert tenant record in IAMS_Master
  ```sql
  INSERT INTO Tenants (TenantId, Name, DatabaseName, IsActive, CreatedAt)
  VALUES ('elzem', 'Elzem Insurance Agency', 'IAMS_Tenant_elzem', 1, GETUTCDATE());
  ```
- [ ] Verify tenant created

#### Step 13: Create Admin User
- [ ] Use application's registration API
  - OR create user directly in database
- [ ] Test login

### Evening (1-2 hours)

#### Step 14: Security Hardening
- [ ] Configure Windows Firewall rules
- [ ] Set file permissions (read-only for app files)
- [ ] Secure appsettings.Production.json
- [ ] Disable SQL SA account (if not needed)
- [ ] Review Event Viewer for errors

---

## Day 3: Backup & Testing

### Morning (2-3 hours)

#### Step 15: Configure Backups
- [ ] Verify backup script created: `C:\Scripts\BackupIAMS.ps1`
- [ ] Test backup script manually
- [ ] Create scheduled task (daily at 2 AM)
- [ ] Verify backup files created in `D:\Backups\IAMS`

#### Step 16: Monitoring Setup
- [ ] Configure Windows Event Viewer filtering
- [ ] Create health check script
- [ ] Schedule health check (every 5 minutes)
- [ ] Test email alerts (optional)

### Afternoon (1-2 hours)

#### Step 17: End-to-End Testing
- [ ] Test HTTPS: https://elzem.websuresoft.com
- [ ] Test HTTP redirect: http://elzem.websuresoft.com
- [ ] Test API: https://elzem.websuresoft.com/api/health
- [ ] Test login functionality
- [ ] Test creating a customer
- [ ] Test creating a policy
- [ ] Check application logs: `C:\Logs\IAMS\`
- [ ] Check for errors in Event Viewer

#### Step 18: Performance Testing
- [ ] Test concurrent user load
- [ ] Monitor CPU/memory usage
- [ ] Check database query performance
- [ ] Verify response times acceptable

---

## Post-Deployment

### Documentation
- [ ] Document all passwords securely
- [ ] Document server IP and credentials
- [ ] Document database connection strings
- [ ] Create runbook for common tasks

### Training
- [ ] Train system administrator
- [ ] Train end users
- [ ] Provide user documentation

### Ongoing Maintenance
- [ ] Schedule weekly backup verification
- [ ] Schedule monthly security updates
- [ ] Schedule quarterly full testing
- [ ] Monitor disk space usage

---

## Rollback Plan (If Needed)

If deployment fails:
- [ ] Stop IIS sites
- [ ] Restore previous application files (if updating)
- [ ] Restore database from backup
- [ ] Start IIS sites
- [ ] Verify rollback successful

---

## Support Contacts

**SQL Server Issues:**
- Check: `C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Log\ERRORLOG`

**IIS Issues:**
- Check: `C:\inetpub\logs\LogFiles`
- Check: Event Viewer > Windows Logs > Application

**Application Issues:**
- Check: `C:\Logs\IAMS\log-*.txt`
- Check: `C:\inetpub\iams-api\logs\stdout*.log`

---

## Success Criteria

✅ Deployment is successful when:
- [ ] Application accessible via https://elzem.websuresoft.com
- [ ] SSL certificate valid (no browser warnings)
- [ ] Users can login
- [ ] Database operations work (create/read/update/delete)
- [ ] Backups running automatically
- [ ] No errors in logs
- [ ] Performance acceptable (<2 seconds response time)

---

## Timeline Summary

| Day | Phase | Duration | Status |
|-----|-------|----------|--------|
| Day 1 AM | Software Installation | 2-3 hours | ⬜ |
| Day 1 PM | Database Setup | 2-3 hours | ⬜ |
| Day 1 Eve | Application Deployment | 2-3 hours | ⬜ |
| Day 2 AM | SSL & Domain | 1-2 hours | ⬜ |
| Day 2 PM | Migration & Setup | 1-2 hours | ⬜ |
| Day 2 Eve | Security | 1-2 hours | ⬜ |
| Day 3 AM | Backup & Monitoring | 2-3 hours | ⬜ |
| Day 3 PM | Testing | 1-2 hours | ⬜ |
| **Total** | | **12-20 hours** | |

---

**Prepared for:** elzem.websuresoft.com
**Deployment Date:** _______________
**Deployed By:** _______________

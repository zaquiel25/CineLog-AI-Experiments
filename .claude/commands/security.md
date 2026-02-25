---
description: "Security audit — checks API security, credentials exposure, OWASP risks, and dead code"
allowed-tools: ["Read", "Bash", "Grep", "Glob"]
---

# Security Audit

Run a focused security review of the FrameRoute codebase. This is critical for a public repo.

## Checks to Perform

### 1. Credentials & Secrets Scan
- Grep for patterns: password, secret, token, apikey, connectionstring (case-insensitive) in all tracked files
- Check appsettings*.json for any real values (not placeholders)
- Check Program.cs for hardcoded strings that look like credentials
- Verify .gitignore covers: *.env, settings.local.json, user secrets
- Run `git log --all -S 'password' --oneline` to check git history (sample check)

### 2. API Security
- Verify all controller actions have `[Authorize]` or are intentionally anonymous
- Check for missing `[ValidateAntiForgeryToken]` on POST actions
- Verify AJAX endpoints check `X-Requested-With: XMLHttpRequest` header
- Check CORS configuration if any

### 3. OWASP Top 10 Review
- **Injection**: Check for raw SQL, string concatenation in queries, unsanitized user input
- **Broken Auth**: Verify authentication flow, session management
- **Sensitive Data Exposure**: Check logging for PII, check error pages don't leak stack traces in production
- **XSS**: Check Razor views for `@Html.Raw()` usage, verify encoding
- **CSRF**: Verify anti-forgery tokens on all forms
- **Security Misconfiguration**: Check HTTPS enforcement, security headers

### 4. User Data Isolation (FrameRoute-specific)
- Verify ALL database queries filter by `UserId`
- Check for any endpoints that could leak data across users
- Review any admin/debug endpoints

### 5. Dead Code & Attack Surface
- Identify unused controllers, actions, or API endpoints
- Check for debug/test endpoints that shouldn't be in production
- Look for commented-out code that might contain old security logic

### 6. Dependency Vulnerabilities
- Check for known vulnerable NuGet packages
- Verify authentication packages are up to date

## Output Format

```
## FrameRoute Security Audit — [date]

### Credentials: ✅/❌ (X issues)
[details]

### API Security: ✅/❌ (X issues)
[details per endpoint]

### OWASP: ✅/❌ (X issues)
[details per category]

### User Data Isolation: ✅/❌
[details]

### Dead Code / Attack Surface: X items
[list]

### Dependencies: ✅/❌
[details]

### Risk Level: LOW/MEDIUM/HIGH
### Top Priority Fixes:
1. ...
2. ...
3. ...
```

Read `lessons-learned.md` from memory before starting. After completing, add any new security lessons discovered.

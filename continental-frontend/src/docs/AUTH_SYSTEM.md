# Authentication System with 401 Error Handling

## Overview

This document describes the comprehensive authentication system implemented in the Continental Frontend application, including automatic token refresh, global 401 error handling, and session management.

## Architecture

### Core Components

1. **AuthService** (`src/services/authService.ts`)
   - Handles login, logout, token refresh, and user management
   - Integrates with real backend API endpoints
   - Manages local storage for tokens and user data

2. **HttpClient** (`src/services/httpClient.ts`)
   - Centralized HTTP client with automatic 401 error handling
   - Implements request queuing during token refresh
   - Automatic retry logic for failed requests

3. **SessionContext** (`src/contexts/SessionContext.tsx`)
   - Global session state management
   - Handles session expiration events
   - Provides UI feedback for session issues

4. **SessionExpiredDialog** (`src/components/SessionExpiredDialog.tsx`)
   - User-friendly dialog for session expiration
   - Options to refresh session or redirect to login

## How It Works

### 1. Automatic Token Refresh

When any API request receives a 401 (Unauthorized) response:

1. **Detection**: HttpClient catches the 401 error in `makeRequest()`
2. **Queue Management**: If a refresh is already in progress, the request is queued
3. **Token Refresh**: Calls `POST /Auth/refresh-token` with current token
4. **Retry**: On successful refresh, retries the original request with new token
5. **Failure Handling**: If refresh fails, clears auth state and shows session expired dialog

### 2. Request Queuing

To prevent multiple simultaneous refresh attempts:

- Uses `isRefreshing` flag to track refresh state
- Queues failed requests in `failedQueue` array
- Processes all queued requests once refresh completes
- Ensures consistent behavior across concurrent requests

### 3. Session Management

The SessionContext provides:

- Global session state tracking
- Event-driven session expiration handling
- UI feedback through SessionExpiredDialog
- Manual session refresh capabilities

## API Endpoints

### Authentication Endpoints

- `POST /Auth/login` - User login
- `POST /Auth/refresh-token` - Token refresh
- `GET /api/User/profile` - Get current user profile

### Request/Response Format

```typescript
// Login Request
{
  username: string;
  password: string;
}

// Login Response
{
  success: boolean;
  data: {
    token: string;
    expiration: string;
  }
}

// Refresh Token Response
{
  success: boolean;
  data: {
    token: string;
    expiresIn: number;
  }
}
```

## Usage Examples

### Using the Auth Hook

```typescript
import { useAuth } from '@/hooks/useAuth';

function MyComponent() {
  const { user, isAuthenticated, login, logout } = useAuth();

  const handleLogin = async (credentials) => {
    try {
      await login(credentials);
      // User is now logged in, token refresh is automatic
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  return (
    <div>
      {isAuthenticated ? (
        <p>Welcome, {user?.fullName}</p>
      ) : (
        <LoginForm onSubmit={handleLogin} />
      )}
    </div>
  );
}
```

### Making API Calls

```typescript
import { httpClient } from '@/services/httpClient';

// All API calls automatically handle 401 errors
async function fetchUsers() {
  try {
    const response = await httpClient.get('/api/User/list');
    return response.data;
  } catch (error) {
    // If 401, token refresh happens automatically
    // If refresh fails, session expired dialog shows
    console.error('API call failed:', error);
  }
}
```

### Session Management

```typescript
import { useSession } from '@/contexts/SessionContext';

function MyComponent() {
  const { isSessionExpired, refreshSession } = useSession();

  const handleManualRefresh = async () => {
    try {
      await refreshSession();
      console.log('Session refreshed successfully');
    } catch (error) {
      console.error('Session refresh failed:', error);
    }
  };

  return (
    <div>
      {isSessionExpired && (
        <button onClick={handleManualRefresh}>
          Refresh Session
        </button>
      )}
    </div>
  );
}
```

## Configuration

### Environment Variables

```typescript
// src/config/env.ts
export const env = {
  API_BASE_URL: 'http://localhost:5050', // Backend API URL
  API_TIMEOUT: 30000, // Request timeout in ms
};
```

### Token Storage

Tokens are stored in localStorage with these keys:
- `auth_token`: Current JWT token
- `user`: Complete user object with token

## Error Handling Flow

```
API Request (401) → HttpClient Intercepts → Check if Refreshing
                                          ↓
                                    Queue Request → Refresh Token
                                          ↓              ↓
                                    Success → Retry    Failure → Clear Auth
                                          ↓              ↓
                                    Process Queue    Show Dialog
```

## Security Considerations

1. **Token Storage**: Tokens stored in localStorage (consider httpOnly cookies for production)
2. **Automatic Logout**: Failed refresh attempts clear all auth data
3. **Request Queuing**: Prevents token refresh race conditions
4. **Session Validation**: Tokens validated on app initialization

## Testing

### Manual Testing

1. **Login Flow**: Test login with valid/invalid credentials
2. **Token Expiration**: Wait for token to expire and make API call
3. **Concurrent Requests**: Make multiple API calls simultaneously when token expires
4. **Network Issues**: Test with network disconnected during refresh

### Integration Points

- All protected routes use the authentication system
- User management components (Usuarios.tsx, DetallesUsuario.tsx) benefit from automatic token refresh
- Session expiration provides user-friendly feedback

## Troubleshooting

### Common Issues

1. **Infinite Refresh Loop**: Check that refresh endpoint doesn't return 401
2. **Token Not Updating**: Verify localStorage is being updated after refresh
3. **UI Not Updating**: Ensure components use useAuth hook for reactive state
4. **CORS Issues**: Verify backend allows refresh token endpoint

### Debug Logging

The system includes comprehensive logging:
- Auth actions logged with `logger.authAction()`
- API requests/responses logged with `logger.apiRequest()`/`logger.apiResponse()`
- Errors logged with `logger.error()`

## Future Enhancements

1. **Refresh Token Rotation**: Implement refresh token rotation for enhanced security
2. **Token Expiration Prediction**: Proactively refresh tokens before expiration
3. **Offline Support**: Handle authentication when network is unavailable
4. **Multi-tab Synchronization**: Sync auth state across browser tabs
5. **Session Timeout Warning**: Warn users before session expires

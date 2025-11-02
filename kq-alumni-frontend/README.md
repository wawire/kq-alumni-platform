# KQ Alumni Platform - Frontend

Modern Next.js 14 frontend application for the Kenya Airways Alumni Association registration platform.

## Tech Stack

- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript 5.3+
- **Styling**: Tailwind CSS 3.4
- **Form Management**: React Hook Form 7.65 + Zod 3.25
- **State Management**:
  - Zustand 4.4 (client state with localStorage persistence)
  - TanStack React Query 5.90 (server state)
- **HTTP Client**: Axios 1.12
- **UI Components**: Custom components with CVA (Class Variance Authority)
- **Icons**: Heroicons 2.2
- **Notifications**: Sonner 2.0

## Getting Started

### Prerequisites

- Node.js 18.17 or higher
- npm 9.0 or higher

### Installation

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.local.example .env.local

# Update .env.local with your backend URL
NEXT_PUBLIC_API_URL=http://localhost:5295
```

### Development

```bash
# Start development server
npm run dev

# Access the application
# http://localhost:3000
```

### Build

```bash
# Production build
npm run build

# Start production server
npm start

# Or use standalone mode (recommended for deployment)
npm run build
node .next/standalone/server.js
```

### Code Quality

```bash
# Run ESLint
npm run lint

# TypeScript type checking
npm run type-check

# Format code with Prettier
npm run format

# Check formatting
npm run format:check
```

### Testing

```bash
# Run tests
npm test

# Watch mode
npm run test:watch

# Coverage report
npm run test:coverage
```

## Project Structure

```
src/
├── app/                          # Next.js App Router
│   ├── layout.tsx                # Root layout with fonts and metadata
│   ├── page.tsx                  # Home page
│   ├── register/
│   │   └── page.tsx              # Registration page (wrapped with ErrorBoundary)
│   └── verify/[token]/
│       └── page.tsx              # Email verification page
│
├── components/
│   ├── ui/                       # Base UI components
│   │   ├── button/
│   │   │   ├── Button.tsx        # CVA-based button with variants
│   │   │   └── index.ts
│   │   ├── input/
│   │   │   ├── Input.tsx         # Styled input component
│   │   │   └── index.ts
│   │   ├── label/
│   │   │   ├── Label.tsx         # Form label with required indicator
│   │   │   └── index.ts
│   │   ├── card/
│   │   │   ├── Card.tsx          # Card container component
│   │   │   └── index.ts
│   │   └── index.ts              # Barrel export
│   │
│   ├── forms/                    # Form components (React Hook Form integration)
│   │   ├── FormField/
│   │   │   ├── FormField.tsx     # Text input field with validation
│   │   │   └── index.ts
│   │   ├── FormSelect/
│   │   │   ├── FormSelect.tsx    # Select dropdown with validation
│   │   │   └── index.ts
│   │   ├── FormTextarea/
│   │   │   ├── FormTextarea.tsx  # Textarea with character counter
│   │   │   └── index.ts
│   │   └── index.ts              # Barrel export
│   │
│   ├── registration/             # Registration-specific components
│   │   ├── steps/
│   │   │   ├── PersonalInfoStep.tsx      # Step 1: Personal information
│   │   │   ├── EmploymentStep.tsx        # Step 2: Employment information
│   │   │   └── EngagementStep.tsx        # Step 3: Engagement preferences
│   │   ├── RegistrationForm.tsx          # Main wizard container
│   │   ├── SuccessScreen.tsx             # Post-submission success
│   │   └── ErrorScreen.tsx               # Error state UI
│   │
│   ├── layout/
│   │   ├── Header.tsx            # Application header
│   │   └── index.ts
│   │
│   ├── providers/
│   │   ├── QueryProvider.tsx     # React Query provider wrapper
│   │   └── index.ts
│   │
│   └── ErrorBoundary.tsx         # Error boundary for graceful error handling
│
├── services/
│   ├── api.ts                    # Axios instance with interceptors
│   └── registrationService.ts    # Registration API calls
│
├── store/
│   └── registrationStore.ts      # Zustand store for form state
│
├── hooks/
│   ├── useDebounce.ts            # Debounce hook for input delays
│   ├── useDuplicateCheck.ts      # Real-time duplicate validation
│   └── index.ts
│
├── types/
│   ├── registration.ts           # Registration-related types
│   └── index.ts
│
├── utils/
│   ├── cn.ts                     # clsx + tailwind-merge utility
│   └── validation.ts             # Zod validation schemas
│
└── constants/
    ├── countries.ts              # Country/city data
    ├── departments.ts            # KQ departments
    ├── engagementOptions.ts      # Volunteer/event options
    └── index.ts
```

## Key Features

### Multi-Step Registration Wizard

The registration form is split into three steps with state persistence:

1. **Personal Information** (`PersonalInfoStep.tsx`)
   - Full name, email, phone
   - Country and city selection
   - Real-time duplicate email/phone detection

2. **Employment Information** (`EmploymentStep.tsx`)
   - Staff number, department
   - Employment start/end dates
   - Professional certifications (textarea)

3. **Engagement Preferences** (`EngagementStep.tsx`)
   - Volunteer opportunities (multi-select)
   - Event types of interest (multi-select)
   - Mentorship preferences
   - Newsletter subscription

### State Management

**Zustand Store** (`registrationStore.ts`):
```typescript
// Form state persisted to localStorage
interface RegistrationStore {
  currentStep: number;
  formData: Partial<RegistrationFormData>;
  setCurrentStep: (step: number) => void;
  updateFormData: (data: Partial<RegistrationFormData>) => void;
  resetForm: () => void;
}
```

**React Query** (`QueryProvider.tsx`):
- Server state caching
- Optimistic updates
- Background refetching
- DevTools integration (development only)

### Form Validation

**Zod Schemas** (`validation.ts`):
- Email format validation
- Phone number validation (international format)
- Staff number format (00XXXXX)
- Date range validation
- Required field enforcement
- Character limits for text areas

### Real-Time Validation

**Duplicate Checking** (`useDuplicateCheck.ts`):
```typescript
const { isDuplicate, isChecking } = useDuplicateCheck(
  'email',
  email,
  500 // debounce delay
);
```

Features:
- Debounced API calls (500ms)
- React Query caching
- Loading states
- Error handling

### Component Patterns

**Base UI Components**:
- CVA for type-safe variant management
- Consistent styling with Tailwind
- Accessible by default (ARIA attributes)
- Forward refs for form integration

**Form Components**:
- React Hook Form `Controller` integration
- Error state handling
- Description text support
- Required field indicators
- Consistent styling and behavior

**Registration Steps**:
- `FormProvider` for form context
- Zustand for state persistence
- Navigation controls (Next/Previous/Submit)
- Loading states during submission
- Toast notifications for feedback

## Styling Guide

### Tailwind Configuration

**Colors**:
```javascript
colors: {
  'kq-red': '#E30613',      // Primary brand color
  'kq-blue': '#002F5F',     // Secondary brand color
  'kq-gold': '#C5A572',     // Accent color
}
```

**Fonts**:
```javascript
fontFamily: {
  'cabrito': ['Cabrito', 'sans-serif'],  // Headings
  'roboto': ['Roboto', 'sans-serif'],    // Body text
}
```

### Component Variants (CVA)

**Button Variants**:
```typescript
variant: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'
size: 'sm' | 'md' | 'lg'
```

**Usage**:
```tsx
<Button variant="primary" size="lg" disabled={isLoading}>
  Submit Registration
</Button>
```

## API Integration

### Axios Configuration

**Base Setup** (`api.ts`):
```typescript
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});
```

**Interceptors**:
- Request: Add authorization headers (if needed)
- Response: Handle errors globally
- Transform: camelCase ↔ snake_case conversion

### Registration Service

**Main Endpoints**:
```typescript
// Submit registration
registrationService.create(data)

// Check for duplicates
registrationService.checkDuplicate(field, value)

// Verify email
registrationService.verifyEmail(token)
```

## Environment Variables

### Required Variables

```bash
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5295

# .env.production.local
NEXT_PUBLIC_API_URL=https://api.kqalumni.example.com
```

### Environment Files

- `.env.local` - Local development (git-ignored)
- `.env.development.local` - Development environment (git-ignored)
- `.env.production.local` - Production environment (git-ignored)
- `.env.local.example` - Template for local setup
- `.env.production.local.example` - Template for production

## Deployment

### Production Build

```bash
# Build with standalone output
npm run build

# Standalone server starts automatically
# Output: .next/standalone/
```

### IIS Deployment

1. **Build the application**:
   ```bash
   npm run build
   ```

2. **Copy files to server**:
   - `.next/standalone/` → `C:\inetpub\kqalumni-frontend\`
   - `.next/static/` → `C:\inetpub\kqalumni-frontend\.next\static\`
   - `public/` → `C:\inetpub\kqalumni-frontend\public\`

3. **Configure environment**:
   - Create `.env.production.local` with production API URL
   - Ensure `web.config` is present (already in repo)

4. **Set up Node.js service**:
   ```bash
   # Using pm2 or Windows Service
   node server.js
   ```

5. **Configure IIS reverse proxy** to forward to Node.js server

See `../DEPLOYMENT_GUIDE_IIS.md` for complete instructions.

## Performance Optimization

### Next.js Configuration

**Output Mode**: Standalone
- Minimal deployment size
- Includes only necessary dependencies
- Faster cold starts

**Image Optimization**:
- Next.js Image component for automatic optimization
- Remote patterns configured for external images

### React Query Configuration

```typescript
queryClient: {
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,      // 5 minutes
      cacheTime: 10 * 60 * 1000,     // 10 minutes
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
}
```

### Code Splitting

- Automatic route-based code splitting
- Dynamic imports for heavy components
- Lazy loading for non-critical features

## Troubleshooting

### Common Issues

#### Build Errors

**TypeScript Errors**:
```bash
# Check types
npm run type-check

# Common fix: Clear cache and reinstall
rm -rf .next node_modules
npm install
npm run build
```

**ESLint Errors**:
```bash
# Auto-fix
npm run lint -- --fix

# Disable for specific line
// eslint-disable-next-line @typescript-eslint/no-explicit-any
```

#### Runtime Errors

**Missing Environment Variable Error**:
```
Error: Missing required environment variable: NEXT_PUBLIC_API_URL
Please add it to your .env.local file or environment configuration.
```

**Solution**:
```bash
# Copy the example file to create your local environment file
cp .env.local.example .env.local

# Or if .env.local.example doesn't exist, copy from .env.example
cp .env.example .env.local

# The .env.local file should contain at minimum:
NEXT_PUBLIC_API_URL=http://localhost:5295

# After creating the file, restart your dev server:
npm run dev
```

**API Connection Issues**:
- Verify `NEXT_PUBLIC_API_URL` in `.env.local`
- Check backend is running on correct port
- Inspect browser console for CORS errors
- Check Network tab in DevTools

**State Not Persisting**:
- Check browser localStorage is enabled
- Clear localStorage: `localStorage.clear()`
- Verify Zustand middleware configuration

**Form Validation Issues**:
- Check Zod schema matches form structure
- Verify field names match between schema and form
- Use React DevTools to inspect form state

### Debug Mode

**Enable React Query DevTools**:
```typescript
// Already enabled in development
// Access via floating icon in bottom-left corner
```

**Enable Verbose Logging**:
```typescript
// Add to api.ts
api.interceptors.request.use(config => {
  console.log('API Request:', config);
  return config;
});
```

## Contributing

### Code Style

- Use TypeScript for all new files
- Follow ESLint configuration
- Format with Prettier before committing
- Use barrel exports (index.ts) for cleaner imports

### Component Guidelines

1. **Create new UI components**:
   ```bash
   src/components/ui/my-component/
   ├── MyComponent.tsx
   └── index.ts
   ```

2. **Use CVA for variants**:
   ```typescript
   const myComponentVariants = cva('base-classes', {
     variants: {
       variant: { ... },
       size: { ... },
     },
   });
   ```

3. **Export props interface**:
   ```typescript
   export interface MyComponentProps { ... }
   ```

4. **Add JSDoc comments**:
   ```typescript
   /**
    * MyComponent description
    *
    * @example
    * ```tsx
    * <MyComponent variant="primary" />
    * ```
    */
   ```

### Pull Request Checklist

- [ ] TypeScript: No errors (`npm run type-check`)
- [ ] ESLint: No errors (`npm run lint`)
- [ ] Prettier: Code formatted (`npm run format`)
- [ ] Tests: All passing (`npm test`)
- [ ] Build: Successful (`npm run build`)
- [ ] Manual testing: Feature works as expected
- [ ] Documentation: README updated if needed

## Resources

### Documentation

- [Next.js 14 Documentation](https://nextjs.org/docs)
- [React Hook Form](https://react-hook-form.com/)
- [Zod](https://zod.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
- [Zustand](https://docs.pmnd.rs/zustand/getting-started/introduction)
- [Tailwind CSS](https://tailwindcss.com/docs)

### Internal Documentation

- `../README.md` - Project overview
- `../DEPLOYMENT_GUIDE_IIS.md` - IIS deployment guide
- `../PRE_DEPLOYMENT_CHECKLIST.md` - Deployment checklist

## Support

For frontend-specific issues:
- Check browser console for errors
- Review this README for common solutions
- Contact development team

---

**Version**: 1.0.0
**Last Updated**: 2025-10-25

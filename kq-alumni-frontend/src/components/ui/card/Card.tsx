/**
 * Card Component
 *
 * A flexible container component for grouping related content.
 * Provides consistent spacing, borders, and shadows.
 *
 * @example
 * ```tsx
 * import { Card, CardHeader, CardContent, CardFooter } from '@/components/ui/card';
 *
 * <Card>
 *   <CardHeader>
 *     <h2>Title</h2>
 *   </CardHeader>
 *   <CardContent>
 *     <p>Content goes here</p>
 *   </CardContent>
 *   <CardFooter>
 *     <Button>Action</Button>
 *   </CardFooter>
 * </Card>
 * ```
 */

import { forwardRef } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const cardVariants = cva('bg-white overflow-hidden', {
  variants: {
    variant: {
      default: 'border border-gray-200 shadow-sm',
      elevated: 'shadow-lg border border-gray-100',
      outlined: 'border-2 border-gray-300',
      ghost: 'border-0 shadow-none',
    },

    padding: {
      none: 'p-0',
      sm: 'p-4',
      md: 'p-6',
      lg: 'p-8',
    },

    rounded: {
      none: 'rounded-none',
      sm: 'rounded',
      md: 'rounded-lg',
      lg: 'rounded-xl',
      kq: 'rounded-tl-sm rounded-tr-2xl rounded-bl-2xl rounded-br-sm', // KQ signature style
    },
  },
  defaultVariants: {
    variant: 'default',
    padding: 'md',
    rounded: 'lg',
  },
});

export interface CardProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof cardVariants> {}

const Card = forwardRef<HTMLDivElement, CardProps>(
  ({ className, variant, padding, rounded, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn(cardVariants({ variant, padding, rounded, className }))}
        {...props}
      />
    );
  }
);

Card.displayName = 'Card';

/**
 * CardHeader Component
 * Container for card title and description
 */
const CardHeader = forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn('flex flex-col space-y-1.5', className)}
        {...props}
      />
    );
  }
);

CardHeader.displayName = 'CardHeader';

/**
 * CardTitle Component
 * Main heading for the card
 */
export interface CardTitleProps
  extends React.HTMLAttributes<HTMLHeadingElement> {
  as?: 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6';
}

const CardTitle = forwardRef<HTMLHeadingElement, CardTitleProps>(
  ({ className, as: Component = 'h3', ...props }, ref) => {
    return (
      <Component
        ref={ref}
        className={cn(
          'text-2xl font-cabrito font-bold text-kq-dark leading-none tracking-tight',
          className
        )}
        {...props}
      />
    );
  }
);

CardTitle.displayName = 'CardTitle';

/**
 * CardDescription Component
 * Subtext or description for the card
 */
const CardDescription = forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => {
    return (
      <p
        ref={ref}
        className={cn('text-sm text-gray-600', className)}
        {...props}
      />
    );
  }
);

CardDescription.displayName = 'CardDescription';

/**
 * CardContent Component
 * Main content area of the card
 */
const CardContent = forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => {
    return <div ref={ref} className={cn('pt-0', className)} {...props} />;
  }
);

CardContent.displayName = 'CardContent';

/**
 * CardFooter Component
 * Footer area for actions or additional content
 */
const CardFooter = forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn('flex items-center pt-0', className)}
        {...props}
      />
    );
  }
);

CardFooter.displayName = 'CardFooter';

export {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
  cardVariants,
};

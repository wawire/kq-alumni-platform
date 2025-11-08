interface ProgressIndicatorProps {
  currentStep: number;
  totalSteps: number;
}

export default function ProgressIndicator({ currentStep, totalSteps }: ProgressIndicatorProps) {
  return (
    <div className="flex items-center justify-end gap-2 mb-6">
      {Array.from({ length: totalSteps }, (_, index) => {
        const stepNumber = index + 1;
        const isActive = stepNumber === currentStep;
        const isCompleted = stepNumber < currentStep;

        return (
          <div
            key={stepNumber}
            className={`
              w-2 h-2 rounded-full transition-all duration-300
              ${isActive ? 'bg-kq-red w-8' : ''}
              ${isCompleted ? 'bg-kq-red' : ''}
              ${!isActive && !isCompleted ? 'bg-gray-300' : ''}
            `}
          />
        );
      })}
    </div>
  );
}

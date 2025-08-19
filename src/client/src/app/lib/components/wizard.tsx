import React, {
  Children,
  cloneElement,
  FunctionComponentElement,
  ReactNode,
  useState,
} from 'react';
import { WizardStepProps } from './wizard-step';
import { useRouter } from 'next/navigation';

export interface InjectedWizardProps {
  stepNumber: number;
  currentStep: number;
  isActive: boolean;
  setCurrentStep(step: number): void;
  goToNextStep(): void;
  goToPreviousStep(): void;
  goToLastStep(): void;
  returnRouteUponCancellation: string;
  pendingSubmit?: boolean;
}

interface Props {
  children: ReactNode;
  currentStep?: number;
  initialStep?: number;
  id: string;
  returnRouteUponCancellation: string;
  onCompleteFinalStep: () => void;
  pendingSubmit?: boolean;
}

const Wizard = ({
  children,
  initialStep = 1,
  id,
  returnRouteUponCancellation,
  onCompleteFinalStep,
  pendingSubmit,
}: Props) => {
  const router = useRouter();
  const [activeStep, setActiveStepState] = useState(initialStep);

  const filteredChildren = Children.toArray(
    children,
  ) as FunctionComponentElement<WizardStepProps & InjectedWizardProps>[];
  const lastStep = filteredChildren.length;

  const setActiveStep = async (incomingStep: number) => {
    if (incomingStep > lastStep) {
      onCompleteFinalStep();
      return;
    }

    if (incomingStep < 1) {
      router.push(returnRouteUponCancellation);
      return;
    }

    setActiveStepState(incomingStep);
  };

  return (
    <div id={id}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps & InjectedWizardProps>(child, {
          stepNumber,
          returnRouteUponCancellation,
          currentStep: activeStep,
          setCurrentStep: setActiveStep,
          id: `${id}-step-${stepNumber}`,
          async goToPreviousStep() {
            await setActiveStep(stepNumber - 1);
          },
          async goToNextStep() {
            await setActiveStep(
              stepNumber + 1 > lastStep ? lastStep : stepNumber + 1,
            );
          },
          async goToLastStep() {
            await setActiveStep(lastStep);
          },
          pendingSubmit,
        });
      })}
    </div>
  );
};

export default Wizard;

import React, {
  Children,
  cloneElement,
  FunctionComponentElement,
  ReactNode,
  useState,
} from 'react';
import { WizardStepProps } from './wizard-step';

export interface InjectedWizardProps {
  stepNumber: number;
  currentStep: number;
  isActive: boolean;
  setCurrentStep(step: number): void;
  goToNextStep(): void;
  goToPreviousStep(): void;
  goToLastStep(): void;
  returnRouteUponCancellation?: string;
}

interface Props {
  children: ReactNode;
  currentStep?: number;
  initialStep?: number;
  id: string;
  returnRouteUponCancellation: string;
  onCompleteFinalStep: () => void;
}

const Wizard = ({
  children,
  initialStep = 1,
  id,
  returnRouteUponCancellation,
  onCompleteFinalStep,
}: Props) => {
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

    setActiveStepState(incomingStep);
  };

  return (
    <div id={id}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps & InjectedWizardProps>(child, {
          stepNumber,
          returnRouteUponCancellation:
            stepNumber === 1 ? returnRouteUponCancellation : undefined,
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
        });
      })}
    </div>
  );
};

export default Wizard;

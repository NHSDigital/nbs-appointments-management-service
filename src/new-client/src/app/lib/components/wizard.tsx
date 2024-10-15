import React, {
  Children,
  cloneElement,
  FunctionComponentElement,
  ReactNode,
  useEffect,
  useState,
} from 'react';
import { WizardStepProps } from './wizard-step';

export interface InjectedWizardProps {
  stepNumber: number;
  currentStep: number;
  isActive: boolean;
  setCurrentStep(step: number, task?: () => Promise<void>): void;
  goToNextStep(task?: () => Promise<void>): void;
  goToPreviousStep(task?: () => Promise<void>): void;
  transitionToOnCancel?: string;
}

interface Props {
  children: ReactNode;
  currentStep?: number;
  initialStep?: number;
  id: string;
  onCancelOutOfWizard: () => void;
  transitionToOnCancel: string;
  onCompleteFinalStep: () => void;
}

const Wizard = ({
  children,
  initialStep = 1,
  id,
  onCancelOutOfWizard,
  transitionToOnCancel,
  onCompleteFinalStep,
}: Props) => {
  const [activeStep, setActiveStepState] = useState(initialStep);

  const filteredChildren = Children.toArray(
    children,
  ) as FunctionComponentElement<WizardStepProps & InjectedWizardProps>[];
  const lastStep = filteredChildren.length;

  const setActiveStep = async (incomingStep: number) => {
    if (incomingStep < 1) {
      onCancelOutOfWizard();
      return;
    }

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
          transitionToOnCancel:
            stepNumber === 1 ? transitionToOnCancel : undefined,
          currentStep: activeStep,
          setCurrentStep: setActiveStep,
          id: `${id}-step-${stepNumber}`,
          async goToPreviousStep() {
            await setActiveStep(stepNumber - 1);
          },
          async goToNextStep(task) {
            await setActiveStep(
              stepNumber + 1 > lastStep ? lastStep : stepNumber + 1,
            );
          },
        });
      })}
    </div>
  );
};

export default Wizard;

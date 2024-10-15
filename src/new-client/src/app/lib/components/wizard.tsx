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
}

interface Props {
  children: ReactNode;
  currentStep?: number;
  initialStep?: number;
  id: string;
  scrollOnMount?: boolean;
  onStepChange?: (
    nextStep: number,
    previousStep: number,
  ) => number | Promise<number>;
}

const Wizard = ({
  children,
  currentStep,
  initialStep = 1,
  id,
  onStepChange,
}: Props) => {
  const [activeStep, setActiveStepState] = useState(initialStep);

  const filteredChildren = Children.toArray(
    children,
  ) as FunctionComponentElement<WizardStepProps & InjectedWizardProps>[];
  const lastStep = filteredChildren.length;

  const setActiveStep = async (
    nextStep: number,
    task?: () => Promise<void>,
  ) => {
    if (nextStep > lastStep || nextStep < 1) {
      return;
    }

    const current = activeStep;
    let next = nextStep;

    if (onStepChange) {
      next = await onStepChange(nextStep, activeStep);
    }

    const stepElement = filteredChildren[next - 1];

    if (next < current && stepElement?.props?.onBack) {
      await stepElement.props.onBack();
    }

    // if (task) {
    //   await task();
    // }

    setActiveStepState(next);
  };

  useEffect(() => {
    if (currentStep) {
      setActiveStepState(currentStep);
    }
  }, [currentStep]);

  return (
    <ol id={id} style={{ listStyle: 'none', paddingLeft: 0 }}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps & InjectedWizardProps>(child, {
          stepNumber,
          currentStep: activeStep,
          setCurrentStep: setActiveStep,
          id: child.props.id || `${id}-step-${stepNumber}`,
          async goToPreviousStep(task) {
            await setActiveStep(stepNumber - 1 < 1 ? 1 : stepNumber - 1, task);
          },
          async goToNextStep(task) {
            await setActiveStep(
              stepNumber + 1 > lastStep ? lastStep : stepNumber + 1,
              task,
            );
          },
        });
      })}
    </ol>
  );
};

export default Wizard;

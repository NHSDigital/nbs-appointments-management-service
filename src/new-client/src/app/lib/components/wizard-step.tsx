'use client';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './wizard';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  id?: string;
}

const WizardStep = ({ children, id, ...rest }: WizardStepProps) => {
  const injectedWizardProps = rest as InjectedWizardProps;
  const { stepNumber, currentStep } = injectedWizardProps;

  if (stepNumber !== currentStep) {
    return null;
  }

  return (
    <div id={id}>
      {typeof children === 'function'
        ? children(injectedWizardProps)
        : children}
    </div>
  );
};

export default WizardStep;

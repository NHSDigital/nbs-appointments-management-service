'use client';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './wizard';
import { BackLink } from '@components/nhsuk-frontend';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  id?: string;
}

const WizardStep = ({ children, id, ...rest }: WizardStepProps) => {
  const injectedWizardProps = rest as InjectedWizardProps;
  const {
    stepNumber,
    currentStep,
    returnRouteUponCancellation,
    goToPreviousStep,
  } = injectedWizardProps;

  if (stepNumber !== currentStep) {
    return null;
  }

  return (
    <div id={id}>
      {stepNumber === 1 ? (
        <BackLink
          href={returnRouteUponCancellation ?? '/'}
          renderingStrategy="server"
        />
      ) : (
        <BackLink onClick={goToPreviousStep} renderingStrategy="client" />
      )}
      {typeof children === 'function'
        ? children(injectedWizardProps)
        : children}
    </div>
  );
};

export default WizardStep;

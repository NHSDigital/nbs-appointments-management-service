/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@nhsuk-frontend-components';
import {
  AccessibilityDefinition,
  AccessibilityValue,
  SetAccessibilitiesRequest,
} from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { saveSiteAccessibilityValues } from '@services/appointmentsService';

type FormFields = {
  accessibilityValues: string[];
};

const AddAccessibilitiesForm = ({
  accessibilityDefinitions,
  site,
  accessibilityValues,
}: {
  accessibilityDefinitions: AccessibilityDefinition[];
  site: string;
  accessibilityValues: AccessibilityValue[];
}) => {
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: {
      accessibilityValues: accessibilityValues
        .filter(ac => ac.value === 'true')
        .map(ac => ac.id),
    },
  });

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetAccessibilitiesRequest = {
      accessibilityValues: accessibilityDefinitions.map(ad => ({
        id: ad.id,
        value:
          form.accessibilityValues.find((fv: string) => ad.id === fv) !==
          undefined
            ? 'true'
            : 'false',
      })),
    };
    await saveSiteAccessibilityValues(site, payload);

    replace(`/site/${site}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup legend="Access needs">
        <div className="nhsuk-hint" id="example-hint">
          Select all options that the current site offers
        </div>
        <CheckBoxes>
          {accessibilityDefinitions.map(ad => (
            <CheckBox
              id={ad.id}
              label={ad.displayName}
              key={`checkbox-key-${ad.id}`}
              value={ad.id}
              {...register('accessibilityValues')}
            />
          ))}
        </CheckBoxes>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Confirm site details</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default AddAccessibilitiesForm;

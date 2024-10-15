/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
} from '@nhsuk-frontend-components';
import { AttributeDefinition, AttributeValue } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { saveSiteAttributeValues } from '@services/appointmentsService';

type FormFields = {
  attributeValues: string[];
};

const AddAttributesForm = ({
  attributeDefinitions,
  site,
  attributeValues,
}: {
  attributeDefinitions: AttributeDefinition[];
  site: string;
  attributeValues: AttributeValue[];
}) => {
  const { replace } = useRouter();
  const { register, handleSubmit } = useForm<FormFields>({
    defaultValues: {
      attributeValues: attributeValues
        .filter(av => av.value === 'true')
        .map(av => av.id),
    },
  });

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: AttributeValue[] = attributeDefinitions.map(ad => ({
      id: ad.id,
      value:
        form.attributeValues.find((fv: string) => ad.id === fv) !== undefined
          ? 'true'
          : 'false',
    }));
    await saveSiteAttributeValues(site, payload);

    replace(`/site/${site}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup legend="Access needs">
        <div className="nhsuk-hint" id="example-hint">
          Select all options that the current site offers
        </div>
        <CheckBoxes>
          {attributeDefinitions.map(ad => (
            <CheckBox
              id={ad.id}
              label={ad.displayName}
              key={`checkbox-key-${ad.id}`}
              value={ad.id}
              {...register('attributeValues')}
            />
          ))}
        </CheckBoxes>
      </FormGroup>

      <ButtonGroup>
        <Button type="submit">Confirm site details</Button>
        <Button styleType="secondary" onClick={cancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default AddAttributesForm;

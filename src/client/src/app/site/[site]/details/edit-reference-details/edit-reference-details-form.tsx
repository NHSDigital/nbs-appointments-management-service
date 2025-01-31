/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import {
  Button,
  FormGroup,
  ButtonGroup,
  SmallSpinnerWithText,
  TextInput,
  Select,
  SelectOption,
} from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import {
  SetSiteReferenceDetailsRequest,
  SiteWithAttributes,
  WellKnownOdsEntry,
} from '@types';
import { saveSiteReferenceDetails } from '@services/appointmentsService';

type FormFields = {
  odsCode: string;
  icb: string;
  region: string;
};

const EditReferenceDetailsForm = ({
  siteWithAttributes,
  wellKnownOdsCodeEntries,
}: {
  siteWithAttributes: SiteWithAttributes;
  wellKnownOdsCodeEntries: WellKnownOdsEntry[];
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: {
      odsCode: siteWithAttributes.odsCode,
      icb: siteWithAttributes.integratedCareBoard,
      region: siteWithAttributes.region,
    },
  });

  const mapWellKnownCodes = (type: string): SelectOption[] =>
    wellKnownOdsCodeEntries
      .filter(e => e.type === type)
      .map(x => {
        return {
          value: x.odsCode,
          label: x.displayName,
        };
      });

  const validateWellKnownCode = (value: string, type: string): boolean => {
    const wellKnownICB = wellKnownOdsCodeEntries.find(
      x => x.type === type && x.odsCode === value,
    );

    return wellKnownICB !== undefined;
  };

  const { replace } = useRouter();

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetSiteReferenceDetailsRequest = {
      odsCode: form.odsCode,
      icb: form.icb,
      region: form.region,
    };
    await saveSiteReferenceDetails(siteWithAttributes.id, payload);

    replace(`/site/${siteWithAttributes.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        error={errors.odsCode ? 'You have not entered an ODS code' : undefined}
      >
        <TextInput
          id="odsCode"
          label="ODS code"
          {...register('odsCode', {
            required: true,
          })}
        ></TextInput>
      </FormGroup>
      <FormGroup
        error={errors.icb ? 'You have not selected an ICB' : undefined}
      >
        <Select
          id="icb"
          label="ICB"
          options={mapWellKnownCodes('icb')}
          {...register('icb', {
            required: true,
            validate: e => validateWellKnownCode(e, 'icb'),
          })}
        ></Select>
      </FormGroup>
      <FormGroup
        error={errors.region ? 'You have not selected a Region' : undefined}
      >
        <Select
          id="region"
          label="Region"
          options={mapWellKnownCodes('region')}
          {...register('region', {
            required: true,
            validate: e => validateWellKnownCode(e, 'region'),
          })}
        ></Select>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Updating reference details..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Save and continue</Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default EditReferenceDetailsForm;

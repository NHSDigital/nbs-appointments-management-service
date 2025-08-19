/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React, { useTransition } from 'react';
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
  Site,
  WellKnownOdsEntry,
} from '@types';
import { saveSiteReferenceDetails } from '@services/appointmentsService';

type FormFields = {
  odsCode: string;
  icb: string;
  region: string;
};

const EditReferenceDetailsForm = ({
  site,
  wellKnownOdsCodeEntries,
}: {
  site: Site;
  wellKnownOdsCodeEntries: WellKnownOdsEntry[];
}) => {
  const [pendingSubmit, startTransition] = useTransition();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>({
    defaultValues: {
      odsCode: site.odsCode,
      icb: site.integratedCareBoard,
      region: site.region,
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
    startTransition(async () => {
      const payload: SetSiteReferenceDetailsRequest = {
        odsCode: form.odsCode.trim(),
        icb: form.icb,
        region: form.region,
      };
      await saveSiteReferenceDetails(site.id, payload);

      replace(`/site/${site.id}/details`);
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup error={errors.odsCode?.message}>
        <TextInput
          id="odsCode"
          label="ODS code"
          {...register('odsCode', {
            required: {
              value: true,
              message: 'Enter an ODS code',
            },
          })}
        ></TextInput>
      </FormGroup>
      <FormGroup error={errors.icb?.message}>
        <Select
          id="icb"
          label="ICB"
          options={mapWellKnownCodes('icb')}
          {...register('icb', {
            required: {
              value: true,
              message: 'Select an ICB',
            },
            validate: e => validateWellKnownCode(e, 'icb'),
          })}
        ></Select>
      </FormGroup>
      <FormGroup error={errors.region?.message}>
        <Select
          id="region"
          label="Region"
          options={mapWellKnownCodes('region')}
          {...register('region', {
            required: {
              value: true,
              message: 'Select a region',
            },
            validate: e => validateWellKnownCode(e, 'region'),
          })}
        ></Select>
      </FormGroup>

      {pendingSubmit ? (
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

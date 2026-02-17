/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React, { useTransition } from 'react';
import {
  Button,
  FormGroup,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import {
  SetSiteReferenceDetailsRequest,
  Site,
  WellKnownOdsEntry,
} from '@types';
import {
  fetchPermissions,
  saveSiteReferenceDetails,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';
import { Select, TextInput } from 'nhsuk-react-components';

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
    formState: { errors, dirtyFields },
  } = useForm<FormFields>({
    defaultValues: {
      odsCode: site.odsCode,
      icb: site.integratedCareBoard,
      region: site.region,
    },
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
      await fromServer(saveSiteReferenceDetails(site.id, payload));

      if (dirtyFields.icb || dirtyFields.region) {
        const permissionsAfterUpdate = await fromServer(
          fetchPermissions(site.id),
        );

        const userStillHasAccessToThisSite =
          permissionsAfterUpdate.includes('site:view') &&
          permissionsAfterUpdate.includes('site:get-meta-data');

        if (!userStillHasAccessToThisSite) {
          replace('/sites');
          return;
        }
      }

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
          {...register('icb', {
            required: {
              value: true,
              message: 'Select an ICB',
            },
            validate: e => validateWellKnownCode(e, 'icb'),
          })}
        >
          {wellKnownOdsCodeEntries
            .filter(e => e.type === 'icb')
            .map(x => (
              <Select.Option key={x.odsCode} value={x.odsCode}>
                {x.displayName}
              </Select.Option>
            ))}
        </Select>
      </FormGroup>
      <FormGroup error={errors.region?.message}>
        <Select
          id="region"
          label="Region"
          {...register('region', {
            required: {
              value: true,
              message: 'Select a region',
            },
            validate: e => validateWellKnownCode(e, 'region'),
          })}
        >
          {wellKnownOdsCodeEntries
            .filter(e => e.type === 'region')
            .map(x => (
              <Select.Option key={x.odsCode} value={x.odsCode}>
                {x.displayName}
              </Select.Option>
            ))}
        </Select>
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

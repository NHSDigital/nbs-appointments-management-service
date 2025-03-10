﻿using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record ConsentToEulaRequest(
    [property:JsonProperty("versionDate", Required = Required.Always)]
    DateOnly VersionDate
    );


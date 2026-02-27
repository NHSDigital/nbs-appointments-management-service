using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;

namespace Nhs.Appointments.Api.Integration;

public class FeatureCollectionTests
{
    [Fact]
    public void AllDefinedFeatureFlags_CanExistInAtMostOne_CollectionDefinition()
    {
        var allKnownFeatureFlags = typeof(Flags)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue())
            .ToList();
        
        var allKnownFeatureCollections = typeof(FeatureToggleCollectionNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue())
            .ToList();

        var flagsContainedInACollection = new List<string>();

        //all feature collections must consist of valid flag names
        foreach (var featureCollection in allKnownFeatureCollections)
        {
            var collectionFlags = featureCollection.Split('_')[0].Split('|');
            
            foreach (var flag in collectionFlags)
            {
                allKnownFeatureFlags.Should().Contain(flag);
                flagsContainedInACollection.Add(flag);
            }
        }
        
        //every flag should exist in AT MOST ONE collection
        //you MAY need to combine any affected flags into to a new multiple flag collection if this fails...
        flagsContainedInACollection.Count.Should().Be(flagsContainedInACollection.Distinct().Count());
    }
}

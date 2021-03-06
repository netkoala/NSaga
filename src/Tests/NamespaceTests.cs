﻿using System;
using System.Linq;
using FluentAssertions;
using NSaga;
using PetaPoco;
using Xunit;

namespace Tests
{
    public class NamespaceTests
    {
        [Fact]
        public void NSaga_Contains_Only_One_Namespace()
        {
            //Arrange
            var assembly = typeof(ISagaMediator).Assembly;

            // Act
            var namespaces = assembly.GetTypes()
                                     .Where(t => t.IsPublic)
                                     .Select(t => t.Namespace)
                                     .Distinct()
                                     .ToList();

            // Assert
            var names = String.Join(", ", namespaces);
            namespaces.Should().HaveCount(1, $"Should only contain 'NSaga' namespace, but found '{names}'");
        }

        [Fact]
        public void PetaPocoNamespace_Stays_Internal()
        {
            //Arrange
            var petapocoTypes = typeof(SqlSagaRepository).Assembly
                                .GetTypes()
                                .Where(t => !String.IsNullOrEmpty(t.Namespace))
                                .Where(t => t.Namespace.StartsWith("PetaPoco", StringComparison.OrdinalIgnoreCase))
                                .Where(t => t.IsPublic)
                                .ToList();

            petapocoTypes.Should().BeEmpty();
        }

        [Fact]
        public void TinyIoc_Stays_Internal()
        {
            typeof(TinyIoCContainer).IsPublic.Should().BeFalse();
        }


        [Fact]
        public void PetaPoco_Stays_Internal()
        {
            typeof(Database).IsPublic.Should().BeFalse();
        }


        [Fact]
        public void AllPublicClasses_Are_Sealed()
        {
            //Arrange
            var unsealedTypes = typeof(SagaMediator).Assembly.GetTypes()
                            .Where(t => t.IsClass)
                            .Where(t => t.IsPublic)
                            .Where(t => !t.IsAbstract)
                            .Where(t => !t.IsSealed)
                            .ToList();

            var message = $"No public classes should be unsealed. Found unsealed: {String.Join(", ", unsealedTypes)}";
            unsealedTypes.Should().BeEmpty(message);
        }
    }
}

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IAMS.IntegrationTests.Controllers;

/// <summary>
/// Audit for issue #504: every API controller action must require authorization.
/// Anonymous endpoints are only allowed when explicitly listed here with a reason.
/// (A global FallbackPolicy also enforces this at runtime; this test catches
/// accidental [AllowAnonymous] and documents the intended exceptions.)
/// </summary>
public class ControllerAuthorizationAuditTests
{
    // Endpoints that are anonymous by design.
    private static readonly HashSet<string> AllowedAnonymousActions = new()
    {
        "AuthController.Login",          // entry point — issues the JWT
        "AuthController.RefreshToken",   // authenticates via the refresh token itself
        "NexusLeadsController.ReceiveNexusLead", // authenticates via X-Nexus-Signature (fails closed without a configured secret)
    };

    private static IEnumerable<Type> ApiControllers =>
        typeof(Program).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract);

    [Fact]
    public void EveryController_RequiresAuthorization_OrIsExplicitlyAllowListed()
    {
        var violations = new List<string>();

        foreach (var controller in ApiControllers)
        {
            var controllerHasAuthorize = controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any();

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetCustomAttributes<NonActionAttribute>().Any() == false);

            foreach (var action in actions)
            {
                var name = $"{controller.Name}.{action.Name}";
                var actionHasAuthorize = action.GetCustomAttributes<AuthorizeAttribute>().Any();
                var actionAllowsAnonymous = action.GetCustomAttributes<AllowAnonymousAttribute>().Any();

                if (actionAllowsAnonymous && !AllowedAnonymousActions.Contains(name))
                {
                    violations.Add($"{name} is [AllowAnonymous] but not in the reviewed allow-list");
                }
                else if (!actionAllowsAnonymous && !controllerHasAuthorize && !actionHasAuthorize)
                {
                    violations.Add($"{name} has no [Authorize] on the action or its controller");
                }
            }
        }

        violations.Should().BeEmpty(
            "every API endpoint must be protected, or its anonymity must be an explicit, reviewed decision");
    }

    [Fact]
    public void AllowListedAnonymousActions_StillExist()
    {
        var knownActions = ApiControllers
            .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(m => $"{c.Name}.{m.Name}"))
            .ToHashSet();

        AllowedAnonymousActions.Should().BeSubsetOf(knownActions,
            "remove entries from the allow-list when the endpoint is deleted or renamed");
    }
}

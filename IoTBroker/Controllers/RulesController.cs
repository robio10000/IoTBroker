using IoTBroker.Models;
using IoTBroker.Rules;
using IoTBroker.Rules.Models;
using Microsoft.AspNetCore.Mvc;

namespace IoTBroker.Controllers;

/// <summary>
///     Controller to manage sensor rules
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RulesController : BaseApiController
{
    private readonly IRuleService _ruleService;

    public RulesController(IRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    /// <summary>
    ///     Create a new sensor rule
    /// </summary>
    /// <param name="rule">The sensor rule to create</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPost]
    public IActionResult CreateRule([FromBody] SensorRule rule)
    {
        var clientId = GetClientId();
        rule.ClientId = clientId;

        _ruleService.AddRule(rule);
        return CreatedAtAction(nameof(GetRules), new { }, rule);
    }

    /// <summary>
    ///     Get all sensor rules for the authenticated client
    /// </summary>
    /// <returns>List of sensor rules</returns>
    [HttpGet]
    public IActionResult GetRules()
    {
        var clientId = GetClientId();
        return Ok(_ruleService.GetRulesByClient(clientId));
    }

    /// <summary>
    ///     Delete a specific sensor rule by ID
    /// </summary>
    /// <param name="id">The ID of the rule to delete</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteRule(string id)
    {
        var clientId = GetClientId();
        if (_ruleService.DeleteRule(clientId, id)) return NoContent();
        return NotFound();
    }
}
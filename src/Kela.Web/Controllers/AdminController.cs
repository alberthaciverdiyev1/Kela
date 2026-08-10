using System.Linq;
using System.Security.Claims;
using Kela.Web.Helpers;
using Kela.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleAdmin)]
public sealed class AdminController(IApiClient api, Localizer L) : Controller
{
    private int UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public IActionResult Dashboard() => View();

    [HttpGet("admin/questions")]
    public IActionResult Questions() => View("~/Views/Teacher/Questions/Index.cshtml");

    [HttpGet("admin/questions/list")]
    public async Task<IActionResult> QuestionsList(CancellationToken ct)
    {
        var result = await api.GetQuestionsAsync(UserId, ct);
        return Json(result.Data ?? []);
    }

    [HttpPost("admin/questions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest model, CancellationToken ct)
    {
        var result = await api.CreateQuestionAsync(new CreateQuestionRequest(
            UserId, model.Text, model.OptionA, model.OptionB, model.OptionC, model.OptionD, model.OptionE, model.CorrectOption), ct);
        return result.Success ? Json(new { success = true, id = result.Data }) : ApiError(result);
    }

    [HttpPut("admin/questions/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionRequest model, CancellationToken ct)
    {
        var result = await api.UpdateQuestionAsync(id, model, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpDelete("admin/questions/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, CancellationToken ct)
    {
        var result = await api.DeleteQuestionAsync(id, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    private IActionResult ApiError<T>(ApiResult<T> result)
    {
        var message = result.Errors is { Count: > 0 }
            ? result.Errors.First()
            : result.Message
              ?? L.T("common.error");
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return Json(new { success = false, message });
    }
}

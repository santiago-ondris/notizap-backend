using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => $"{x.Key}: {x.Value?.Errors.First().ErrorMessage}")
                .ToList();

            var response = ApiResponse<List<string>>.ErrorResponse("Errores de validación.");
            response.Data = errors;

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

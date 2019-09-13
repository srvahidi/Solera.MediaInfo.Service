using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Solera.MediaInfo.Service.Models;
using System.Linq;

namespace Solera.MediaInfo.Service.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Keys
                .SelectMany(key => context.ModelState[key].Errors.Select(x => new Error(key, x.ErrorMessage))).ToArray();

                context.Result = new BadRequestResponse(errors.First().Message, errors);
                var errorDetails = string.Join(", ", errors.Select(e => string.Join(", ", e.Field + ": " + e.Message)));
                _logger.LogError($"Validation Failed - {errorDetails}");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Solera.MediaInfo.Service.Models
{
    public class DeleteFileRequest : IValidatableObject
    {
        [FromBody]
        public RemoveFileBody Body { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Body.TargetPaths.Length == 0)
                yield return new ValidationResult("Must contain at least one target path.", new[] { nameof(Body.TargetPaths) });

            foreach (var path in Body.TargetPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    yield return new ValidationResult("Must not contain an empty target path.", new[] { nameof(Body.TargetPaths) });
                }
            }
        }
    }

    public class RemoveFileBody
    {
        [Required]
        public string[] TargetPaths { get; set; }
    }
}

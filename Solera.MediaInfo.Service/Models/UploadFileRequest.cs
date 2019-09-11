using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Solera.MediaInfo.Service.Models
{
    public class UploadFileRequest : IValidatableObject
    {
        [Required]
        [FromForm]
        public string TargetPath { get; set; }

        [Required]
        [FromForm]
        public IFormFile File { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (File == null || File.Length == 0)
                yield return new ValidationResult("Cannot be null or empty.", new[] { nameof(File) });
        }
    }
}

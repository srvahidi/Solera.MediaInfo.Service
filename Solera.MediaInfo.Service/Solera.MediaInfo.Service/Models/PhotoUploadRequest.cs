using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solera.MediaInfo.Service.Models
{
    public class PhotoUploadRequest
    {
        public IFormFile photo { get; set; }
        public string FileName { get; set; }
        public string ValuationNumber { get; set; }
    }
}

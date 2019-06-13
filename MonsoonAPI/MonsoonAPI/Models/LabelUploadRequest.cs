using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class LabelUploadRequest
    {
        public bool Replace { get; set; }

        public IFormFile LabelsCsv { get; set; }
    }
}

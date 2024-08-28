using System.Collections.Generic;

namespace Lueben.Microservice.GenericEmail.Models.Responses
{
    public class ApiResponse
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public List<ApiErrorResponse> Errors { get; set; }
    }
}
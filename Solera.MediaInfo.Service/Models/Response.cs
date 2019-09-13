using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Solera.MediaInfo.Service.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Response<T> where T : class
    {
        public Response(bool isSuccess, int? statusCode, T data = null, string message = null, Error[] errors = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Data = data;
            Message = message;
            Errors = errors;
        }

        /// <summary>
        /// This property represents the response HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="T"/> is success.
        /// </summary>
        /// <value><c>true</c> if is success; otherwise, <c>false</c>.</value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// This property represents the response data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// This property represents the main error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// This property contains the error messages.
        /// </summary>
        public Error[] Errors { get; }
    }

    public class OkResponse : ObjectResult
    {
        public OkResponse(object data)
            : base(new Response<object>(true, StatusCodes.Status200OK, data))
        {
            StatusCode = StatusCodes.Status200OK;
        }
    }

    public class BadRequestResponse : ObjectResult
    {
        public BadRequestResponse(string message, Error[] errors = null)
            : base(new Response<string>(false, StatusCodes.Status400BadRequest, null, message, errors))
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    public class InternalServerErrorResponse : ObjectResult
    {
        public InternalServerErrorResponse(string message)
            : base(new Response<object>(false, StatusCodes.Status500InternalServerError, null, message))
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Error
    {
        public string Field { get; }

        public string Message { get; }

        public Error(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
}

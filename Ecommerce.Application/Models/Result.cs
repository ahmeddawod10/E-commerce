using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Models
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public static Result<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Data = data, Message = message, StatusCode = 200 };

        public static Result<T> BadRequest(string message = "Bad Request") =>
            new() { Success = false, Message = message, StatusCode = 400 };

        public static Result<T> Unauthorized(string message = "Unauthorized") =>
            new() { Success = false, Message = message, StatusCode = 401 };

        public static Result<T> Forbidden(string message = "Forbidden") =>
            new() { Success = false, Message = message, StatusCode = 403 };

        public static Result<T> NotFound(string message = "Not Found") =>
            new() { Success = false, Message = message, StatusCode = 404 };

        public static Result<T> Internal(string message = "Internal Server Error") =>
            new() { Success = false, Message = message, StatusCode = 500 };
    }

}

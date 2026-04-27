namespace BikeService.Application.Wrappers
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }


        public Dictionary<string, string>? FieldErrors { get; set; }

        public static Result<T> FailField(string field, string error)
        {
            return new Result<T>
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>
                {
                    { field, error }
                }
            };
        }



        // Success
        public static Result<T> Ok(T? data, string? message = null)
        {
            return new Result<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        // Failure - single error
        public static Result<T> Fail(string error, string? message = null)
        {
            return new Result<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Message = message
            };
        }

        // Failure - multiple errors
        public static Result<T> Fail(List<string> errors, string? message = null)
        {
            return new Result<T>
            {
                Success = false,
                Errors = errors,
                Message = message
            };
        }

        // Manual creation
        public static Result<T> From(bool success, T? data = default, string? message = null, List<string>? errors = null)
        {
            return new Result<T>
            {
                Success = success,
                Data = data,
                Message = message,
                Errors = errors
            };
        }
    }
}

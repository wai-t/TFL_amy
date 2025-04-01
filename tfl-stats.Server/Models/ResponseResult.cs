namespace tfl_stats.Server.Models
{
    public class ResponseResult<T>
    {
        public bool IsSuccessful { get; set; }
        public T? Data { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        //public string Message { get; set; }

        public ResponseResult(bool isSuccessful, T? data, ResponseStatus responseStatus)
        {
            IsSuccessful = isSuccessful;
            Data = data;
            ResponseStatus = responseStatus;
            //Message = message;
        }
    }

    public enum ResponseStatus
    {
        Ok,
        BadRequest,
        NotFound,
        InternalServerError
    }
}

namespace UserAPI.Services
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public T? Data { get; private set; }

        private ServiceResult(bool success, T? data, string? errorMessage)
        {
            IsSuccess = success;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>(true, data, null);
        }

        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>(false, default, errorMessage);
        }
    }
}

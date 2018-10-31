namespace KeysPlus.Service.Models
{
    public class ServiceResponseResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public object NewObject { get; set; }
    }

    public class ServiceResponseResult<T> : ServiceResponseResult
    {
        public T Result { get; set; }
    }
}

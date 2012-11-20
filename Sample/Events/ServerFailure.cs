using CQRS.Sample.Store;

namespace CQRS.Sample.Events
{
    public class ServerFailure : StoreEvent
    {
        public string Message { get; set; }
    }
}
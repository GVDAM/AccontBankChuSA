using AccountsChu.Domain.Commands.Contracts;

namespace AccountsChu.Domain.Commands
{
    public class GenericCommandResult : ICommandResult
    {
        public GenericCommandResult() { }

        public GenericCommandResult(bool success, string message, object data)
        {
            IsSuccess = success;
            Message = message;
            Data = data;
        }

        public GenericCommandResult(bool success)
        {
            IsSuccess = success;
        }

        public GenericCommandResult(bool success, string message)
        {
            IsSuccess = success;
            Message = message;
        }


        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}

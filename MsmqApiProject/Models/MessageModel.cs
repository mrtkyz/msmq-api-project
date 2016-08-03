namespace MsmqApiProject.Models
{
    public class MessageModel
    {
        public object Body { get; set; }

        public string Label { get; set; }

        public string QueueName { get; set; }

        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; internal set; }
    }
}

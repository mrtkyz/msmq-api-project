using MsmqApiProject.Models;
using System;
using System.Collections.Generic;
using System.Messaging;
using System.Web.Http;

namespace MsmqApiProject.Controllers
{
    public class HomeController : ApiController
    {
        MessageQueue _ClientQueue;

        public HomeController()
        {
            _ClientQueue = new MessageQueue();
        }

        [HttpGet]
        [Route("message/{queuename}")]
        public MessageModel GetMessageOneByOne(string queuename)
        {
            Message message = null;
            try
            {
                _ClientQueue.Path = @".\private$\" + queuename;
                message = _ClientQueue.Receive();
                message.Formatter = new BinaryMessageFormatter();
                return new MessageModel { IsSuccess = true, QueueName = queuename, Label = message.Label, Body = message.Body };
            }
            catch (Exception ex)
            {
                if (message != null)
                {
                    _ClientQueue.Send(message);
                }

                return new MessageModel { IsSuccess = false, ResponseMessage = ex.ToString() };
            }
            finally
            {
                _ClientQueue.Close();
            }
        }

        [HttpPost]
        [Route("message")]
        public MessageModel InsertMessage(MessageModel messageValue)
        {
            if (!MessageQueue.Exists(@".\private$\" + messageValue.QueueName))
            {
                MessageQueue.Create(@".\private$\" + messageValue.QueueName);
            }
            
            _ClientQueue.Path = @".\private$\" + messageValue.QueueName;
            Message _newMessage = new Message();
            _newMessage.Formatter = new BinaryMessageFormatter();
            _newMessage.Label = messageValue.Label;
            _newMessage.Body = messageValue.Body;
            _ClientQueue.Send(_newMessage);
            _ClientQueue.Close();

            return messageValue;
        }

        [HttpGet]
        [Route("message/getall/{queuename}")]
        public List<MessageModel> GetAllMessageByQueueName(string queuename)
        {
            var list = new List<MessageModel>();
            
            _ClientQueue.Path = @".\private$\" + queuename;
            MessageEnumerator enumerator = _ClientQueue.GetMessageEnumerator2();
            List<Message> queueMessages = new List<Message>();

            while (enumerator.MoveNext(new TimeSpan(0, 0, 1)))
            {
                queueMessages.Add(enumerator.Current);
                Message message = _ClientQueue.ReceiveById(enumerator.Current.Id);
                message.Formatter = new BinaryMessageFormatter();
                try
                {
                    list.Add(new MessageModel { Label = message.Label, Body = message.Body, QueueName = queuename, IsSuccess = true });
                }
                catch (Exception ex)
                {
                    _ClientQueue.Send(message);
                }
            }
            _ClientQueue.Close();

            return list;
        }
    }
    
}
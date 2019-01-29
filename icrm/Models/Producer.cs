using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using RabbitMQ.Client;

namespace icrm.Models
{
    public class Producer:IConnectToRabbitMQ
    {

        

        public Producer(string exchange, string exchangeType) : base(exchange, exchangeType)
        {
        }

        public bool send(Message message)
        {
            try
            {
               Debug.Print(Model+"------model is here----");
                Model.ExchangeDeclare("messageexchange", ExchangeType.Direct);
                Model.QueueDeclare(message.Reciever.UserName, true, false, false, null);
                Model.QueueBind(message.Reciever.UserName, "messageexchange", message.Reciever.UserName, null);
                IBasicProperties basicProperties = Model.CreateBasicProperties();
                basicProperties.Headers = new Dictionary<string, object>();
                // Debug.Print("---before sending-----"+chatViewModel.ToString());
                basicProperties.Headers.Add("msgId", message.Id);
                var msg = Encoding.UTF8.GetBytes(message.Text);

                Model.BasicPublish("messageexchange", message.Reciever.UserName, basicProperties, msg);
            }
            catch (Exception e)
            {
                Debug.Print("----exceprtiondfjnsjkj");
                Debug.Print(e.StackTrace);
            }
            return true;

        }
    }
}
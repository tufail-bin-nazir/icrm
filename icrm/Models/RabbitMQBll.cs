using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using RabbitMQ.Client;
using RabbitMQ.Util;
using RabbitMQ.Client.Events;
using System.Text;
using System.Web.SessionState;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Newtonsoft.Json;
using RabbitMQ.Client.Framing;

namespace icrm.Models
{
    public class RabbitMQBll
    {
        private ApplicationDbContext db;
        private MessageInterface messageService;
        public RabbitMQBll()
        {
            db = new ApplicationDbContext();
            messageService = new MessageRepository();
        }

        public IConnection GetConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.HostName = "localhost";

            Debug.Print("--------"+factory);
            return factory.CreateConnection();
        }
        public bool send(IConnection con,Message message)
        {
            try
            {
                Debug.Print("---ferorororor---------");
                //Debug.Print(message+"--message----"+message.Reciever.UserName+"---rec--id>>>>>>>>"+message.Id+"---send>>>>>"+message.Sender.UserName+"----text>>>"+message.Text+"---chatid>>>>>>>"+message.ChatId);
                IModel channel = con.CreateModel();
                channel.ExchangeDeclare("messageexchange", ExchangeType.Direct);
                channel.QueueDeclare(message.Reciever.UserName, true, false, false, null);
                channel.QueueBind(message.Reciever.UserName, "messageexchange", message.Reciever.UserName, null);
                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.Headers = new Dictionary<string, object>();
               // Debug.Print("---before sending-----"+chatViewModel.ToString());
                basicProperties.Headers.Add("msgId",message.Id);
                var msg = Encoding.UTF8.GetBytes(message.Text);

                channel.BasicPublish("messageexchange", message.Reciever.UserName, basicProperties, msg);
                Debug.Print("-------after pibubfmnbdsmnfbmn");
            }
            catch (Exception e)
            {
                Debug.Print("----exceprtiondfjnsjkj");
                Debug.Print(e.StackTrace);
            }
            return true;

        }
        public Message receive(IConnection con,string myqueue)
        {
            var messageList = new ArrayList();

            try
            {
                string queue = myqueue;
                IModel channel = con.CreateModel();
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);

                BasicGetResult result = channel.BasicGet(queue: queue, autoAck: true);
                if (result != null)
                {

                    
                    int messageId = (int)result.BasicProperties.Headers["msgId"];
                    //Debug.Print("sender0------"+sender+"-------msg----"+ System.Text.Encoding.UTF8.GetString(result.Body));
                   Message message = messageService.UpdateMessage(messageId);
                   Debug.Print(message.RecieveTime+"----recievetime---"+message.Id);
                    return message;
                    /*consumer.Received += (model, ea) =>
                    {
                        
                        var body = ea.Body;
                        int messageId = (int)ea.BasicProperties.Headers["messageId"];
                        Message message =  messageService.UpdateMessage(messageId);
                        messages.Add(message);
                        Debug.Print("we will check size of list");
                        foreach (var msg in messages)
                        {
                            Debug.Print("message---"+msg.Id);
                        }
                        if (Constants.messages[myqueue] == null)
                        {
                            messages.Add(message);
                            Constants.messages.Add(myqueue,messages);
                        }
                        else
                        {

                        }
                        {
                            messages.Add(message);
                            Constants.messages[myqueue] = messages;
                        }

                    };
                    channel.BasicConsume(queue: queue,
                        autoAck: true,
                        consumer: consumer);
                    Debug.Print("returning messages--=-=-==-"+messageList.Count);
                    Debug.Print("Now we will return");
                    return null;

                */
                }
                else
                {
                    Debug.Print(result + "--------- result is null");
                    return null;
                }
               
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                Debug.Print("In exceprioncbsmb");
                return null;
                
            }

        }
    }

}
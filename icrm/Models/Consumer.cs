using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
//using Microsoft.AspNet.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using RabbitMQ.Client.MessagePatterns;

namespace icrm.Models
{
    using icrm.Events;
    using icrm.RepositoryImpl;
    using icrm.RepositoryInterface;

    public class Consumer:IConnectToRabbitMQ
    {
        public EventService eventService;
        private MessageInterface messageService;

        protected bool isConsuming;

        private string exchange;

       // protected string QueueName;

        // used to pass messages back to UI for processing
        public delegate void onReceiveMessage(byte[] message);

        public Consumer(string exchange, string exchangeType) : base(exchange, exchangeType)
        {
            this.exchange = exchange;
            eventService= new EventService();
            messageService = new MessageRepository();
        }

        //internal delegate to run the consuming queue on a seperate thread
        private delegate void ConsumeDelegate(String queueName);

        public void StartConsuming(string queueName)
        {
            Debug.Print(queueName + "-------queueneame");
            //Debug.Print("-----"+Model);
            Model.BasicQos(0, 1, false);
            Model.QueueDeclare(queueName,true,false,false,null);
            Model.QueueBind(queueName, exchange, queueName);
            isConsuming = true;
            ConsumeDelegate c = new ConsumeDelegate(Consume);
            c.BeginInvoke(queueName, null, null);
        }

        protected Subscription mSubscription { get; set; }

        private void Consume(String queueName)
        {
            bool autoAck = false;
            Debug.Print(queueName + "-------queueneame2");
            //create a subscription
            mSubscription = new Subscription(Model, queueName, autoAck);
            Debug.Print("--- initialized subs-------"+ mSubscription.QueueName);
            
            while (isConsuming)
            {                
                BasicDeliverEventArgs e= mSubscription.Next();
                Debug.Print("checking e----" + e);
                if (e != null) {
                    int messageId = (int)e.BasicProperties.Headers["msgId"];
                    Debug.Print("REcieved message----"+messageId);
                    mSubscription.Ack(e);
                    HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                    {
                       messageService.UpdateRecieveTimeOfMessage(messageId);
                    });
                }
                else
                {
                    mSubscription.Ack(e);
                }

            }
        }

        public void Dispose()
        {
            Debug.Print("--disposed consume--------"+mSubscription.QueueName);
            isConsuming = false;
            base.Dispose();
        }
    }
}
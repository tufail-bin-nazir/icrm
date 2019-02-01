using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using RabbitMQ.Client;
using RabbitMQ.Util;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;
using RabbitMQ.Client.Framing.Impl;
using RabbitMQ.Client.MessagePatterns;

namespace icrm.Models
{

    public abstract class IConnectToRabbitMQ : IDisposable
    {

        public string Server { get; set; } 
        public string ExchangeName { get; set; }

        protected IModel Model { get; set; }
        protected IConnection Connection { get; set; }
        protected string ExchangeTypeName { get; set; }

        public IConnectToRabbitMQ(string exchange, string exchangeType)
        {
            ExchangeName = exchange;
            ExchangeTypeName = exchangeType;
        }

        //Create the Connection, Model and Exchange(if one is required)
        public bool ConnectToRabbitMQ()
        {
            try
            {
                var connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = "localhost";
                connectionFactory.UserName = "guest";
                connectionFactory.Password = "guest";
                Connection = connectionFactory.CreateConnection();
                Debug.Print(Connection+"-------connectionhere---");
                Model = Connection.CreateModel();
                Debug.Print(Model+"------model here");
                bool durable = true;
                if (!String.IsNullOrEmpty(ExchangeName))
                    Model.ExchangeDeclare(ExchangeName, ExchangeTypeName, false);
                return true;
            }
            catch (BrokerUnreachableException e)
            {
                return false;
            }
        }

        public void Dispose()
        {
            Debug.Print(Connection+"--------in iconnect rabbitmq-------"+Model);
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;

            }

            if (Model != null)
            {
                Model.Abort();
                Model = null;
            }
               

            Debug.Print(Connection + "--------in iconnect rabbitmq-------" + Model);
        }
    }
}
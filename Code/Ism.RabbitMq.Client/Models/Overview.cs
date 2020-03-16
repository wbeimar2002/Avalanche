using System;
using System.Collections.Generic;

namespace Ism.Telemetry.RabbitMq.Models
{
    public class SampleRetentionPolicies
    {
        public List<int> global { get; set; }
        public List<int> basic { get; set; }
        public List<int> detailed { get; set; }
    }

    public class ExchangeType
    {
        public string name { get; set; }
        public string description { get; set; }
        public bool enabled { get; set; }
    }

    public class MessageStats
    {
    }

    public class ChannelClosedDetails
    {
        public double rate { get; set; }
    }

    public class ChannelCreatedDetails
    {
        public double rate { get; set; }
    }

    public class ConnectionClosedDetails
    {
        public double rate { get; set; }
    }

    public class ConnectionCreatedDetails
    {
        public double rate { get; set; }
    }

    public class QueueCreatedDetails
    {
        public double rate { get; set; }
    }

    public class QueueDeclaredDetails
    {
        public double rate { get; set; }
    }

    public class QueueDeletedDetails
    {
        public double rate { get; set; }
    }

    public class ChurnRates
    {
        public int channel_closed { get; set; }
        public ChannelClosedDetails channel_closed_details { get; set; }
        public int channel_created { get; set; }
        public ChannelCreatedDetails channel_created_details { get; set; }
        public int connection_closed { get; set; }
        public ConnectionClosedDetails connection_closed_details { get; set; }
        public int connection_created { get; set; }
        public ConnectionCreatedDetails connection_created_details { get; set; }
        public int queue_created { get; set; }
        public QueueCreatedDetails queue_created_details { get; set; }
        public int queue_declared { get; set; }
        public QueueDeclaredDetails queue_declared_details { get; set; }
        public int queue_deleted { get; set; }
        public QueueDeletedDetails queue_deleted_details { get; set; }
    }

    public class QueueTotals
    {
    }

    public class ObjectTotals
    {
        public int channels { get; set; }
        public int connections { get; set; }
        public int consumers { get; set; }
        public int exchanges { get; set; }
        public int queues { get; set; }
    }

    public class Listener
    {
        public string node { get; set; }
        public string protocol { get; set; }
        public string ip_address { get; set; }
        public int port { get; set; }
        public object socket_opts { get; set; }
    }

    public class Context
    {
        public List<object> ssl_opts { get; set; }
        public string node { get; set; }
        public string description { get; set; }
        public string path { get; set; }
        public string port { get; set; }
    }

    public class Overview
    {
        public string management_version { get; set; }
        public string rates_mode { get; set; }
        public SampleRetentionPolicies sample_retention_policies { get; set; }
        public List<ExchangeType> exchange_types { get; set; }
        public string rabbitmq_version { get; set; }
        public string cluster_name { get; set; }
        public string erlang_version { get; set; }
        public string erlang_full_version { get; set; }
        public MessageStats message_stats { get; set; }
        public ChurnRates churn_rates { get; set; }
        public QueueTotals queue_totals { get; set; }
        public ObjectTotals object_totals { get; set; }
        public int statistics_db_event_queue { get; set; }
        public string node { get; set; }
        public List<Listener> listeners { get; set; }
        public List<Context> contexts { get; set; }
    }
}

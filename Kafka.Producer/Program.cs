﻿using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Kafka.Data.Models;
using Newtonsoft.Json;

namespace Kafka.Producer
{
    class Program
    {
        private readonly static Dictionary<string, object> Config = new Dictionary<string, object> {
            { "bootstrap.servers", "127.0.0.1:9092" },
            { "acks", "all" },
            { "batch.num.messages", 1 },
            { "linger.ms", 0 },
            { "compression.codec", "gzip" }
        };

        static void Main(string[] args)
        {
            try
            {
                // banner
                Console.WriteLine("Kafka Producer Sample.");
                Console.WriteLine("Press CTRL+C to exit");

                while (!Console.KeyAvailable)
                {
                    // allow user to exit
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        Console.WriteLine("Exiting...");
                        Environment.Exit(0);
                    };

                    // get name
                    Console.WriteLine("\nWhat is your name?");
                    var name = Console.ReadLine();

                    // get message
                    Console.WriteLine("\nWhat is your message?");
                    var message = Console.ReadLine();

                    // set time stamp
                    var msgTimeStamp = DateTime.Now;

                    Msg msg = new Msg();
                    msg.User = name;
                    msg.Message = message;
                    msg.TimeStamp = msgTimeStamp;

                    // send to Kafka
                    ProduceToKafka(msg);

                    // produce message to Kafka
                    Console.WriteLine("Message published to Kafka.");
                    Console.WriteLine("\n");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void ProduceToKafka(Msg msg)
        {
            using (var producer = new Producer<Null, string>(Config, null, new StringSerializer(Encoding.UTF8)))
            {
                // convert the Msg model to json
                var jsonMsg = JsonConvert.SerializeObject(msg);

                // start watch
                var watch = System.Diagnostics.Stopwatch.StartNew();

                // push to Kafka
                var dr = producer.ProduceAsync("topic_messages", null, jsonMsg).Result;
                producer.Flush(1);

                // stop watch and write to console
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset} in {elapsedMs}ms");
            }
        }
    }
}
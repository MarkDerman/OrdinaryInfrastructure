using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Odin.Messaging.RabbitMq;

namespace Tests.Odin.Messaging.RabbitMq;

public class RabbitConnectionServiceTests
{
    public record TestMessage
    {
        public required long MessageNumber { get; init; }
        public bool? Redelivered { get; set; }
        public required string ThreadIdentifier { get; init; }
    }

    [Fact(Skip = "So far, RabbitConnectionService is tested manually only.")]
    public async Task Publish_Works()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        RabbitConnectionService box = new RabbitConnectionService(new RabbitConnectionServiceSettings
        {
            Host = "localhost",
            VirtualHost = "odin-rabbitbox",
            Username = "rabbitbox-test",
            UserPassword = "rabbitbox-test-01",
            Port = 5672,
            ConnectionName = "ManualIntegTests",
            MaxChannels = 10,
            SendTimeoutMillis = 1000,
        });

        List<string> threadIdentifiers = new();
        List<int> threadIdentifiersIndexes = new();

        HashSet<string> activeCallers = new();

        object activeCallersLock = new();

        for (int i = 0; i < 100; i++)
        {
            threadIdentifiersIndexes.Add(i);
            threadIdentifiers.Add(Guid.NewGuid().ToString().Substring(0, 4));
        }

        threadIdentifiers[0] = "MONITOR";

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        IEnumerable<Task> sendingTasks = threadIdentifiersIndexes.Select(i => Task.Run(async () =>
        {
            string s = threadIdentifiers[i];

            await Task.Delay(i * 15, cancellationToken);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long millis = 0;
            long sendAsyncTotalMillis = 0;
            long iteration = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                // await Task.Delay(TimeSpan.FromSeconds(0.001), cancellationToken);
                iteration++;
                byte[] body = JsonSerializer.SerializeToUtf8Bytes(new TestMessage
                {
                    MessageNumber = iteration,
                    Redelivered = null,
                    ThreadIdentifier = s,
                }, jsonOptions);

                try
                {
                    millis = stopwatch.ElapsedMilliseconds;

                    string exchange = "test02-fanout"; // Random.Shared.NextDouble() > 0.5 ? "test02-fanout" : "test01";

                    await box.SendAsync(exchange, "", new Dictionary<string, object>(), "application/json", body, true, false);
                    // TestContext.Progress.WriteLine($"Sent message {i}\n");
                    // Console.WriteLine($"Sent message {s} : {iteration}\n");

                    sendAsyncTotalMillis += stopwatch.ElapsedMilliseconds - millis;
                }
                catch (Exception e)
                {
                    // TestContext.Progress.WriteLine($"Failed to send message {i}:\n" + e);
                    Console.WriteLine($"Failed to send message {s} : {iteration}:\n" + e);
                }

                // if (iteration % 50 == 0)
                // {
                //     // Console.WriteLine($"\n--- Finished loop for caller {s} ---" +
                //     //                   $"active callers count: {activeCallersCount}\n");
                //     
                // }

                if (iteration % 50 == 0)
                {
                    lock (activeCallersLock)
                    {
                        activeCallers.Add(s);
                    }
                }

                if (s == "MONITOR" && iteration % 50 == 0)
                {
                    long? activeCallersCount = null;
                    lock (activeCallersLock)
                    {
                        activeCallersCount = activeCallers.Count;
                    }

                    Console.WriteLine($"\nReporting for caller {s}:" +
                                      $"\nIteration: {iteration}" +
                                      $"\nMean SendAsync wait: {(double)sendAsyncTotalMillis / iteration}" +
                                      $"\nActive callers count: {(activeCallersCount.HasValue ? activeCallersCount.Value : "unknown")}" +
                                      $"\nEnd reporting for caller {s}");
                }

                // await Task.Delay(5000, token);
            }
        }, cancellationToken));

        await Task.WhenAll(sendingTasks);
    }

    [Fact(Skip = "So far, RabbitConnectionService is tested manually only.")]
    public async Task QueueSubscription_Works()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        RabbitConnectionService box = new RabbitConnectionService(new RabbitConnectionServiceSettings
        {
            Host = "localhost",
            VirtualHost = "odin-rabbitbox",
            Username = "rabbitbox-test",
            UserPassword = "rabbitbox-test-01",
            Port = 5672,
            ConnectionName = "RabbitBoxIntegTests",
            MaxChannels = 10,
            SendTimeoutMillis = 5000,
        });

        string queueName = "max-length-test-01";

        IRabbitConnectionService.Subscription? subscription = null;
        try
        {
            TestMessage? consumedMessage = null;

            subscription = await box.SubscribeToConsume(queueName, false, true, 10000, TimeSpan.FromSeconds(5));

            subscription.OnConsumed += async message =>
            {
                string str = "no body";
                try
                {
                    str = Encoding.UTF8.GetString(message.Body);
                    TestMessage body = JsonSerializer.Deserialize<TestMessage>(str)!;
                    consumedMessage = body;
                    // var body = JsonSerializer.Deserialize<TestMessage>(message.Body);
                    body.Redelivered = message.IsRedelivered;
                    // Console.WriteLine("OnConsume callback called. Waiting 4 seconds to ack. Message body: " + body);
                    await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
                    // bool nackMessage = Random.Shared.NextDouble() > 0.5;
                    bool nackMessage = false;

                    if (message.AckNackCallbacks is not null)
                    {
                        if (nackMessage)
                        {
                            await message.AckNackCallbacks.Nack(true);
                        }
                        else
                        {
                            await message.AckNackCallbacks.Ack();
                        }
                    }

                    // Console.WriteLine($"{(nackMessage ? "Nacked" : "Acked")} message " + body);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to handle consumed message. Body: " + str + "\nException: " + e);
                    if (message.AckNackCallbacks != null)
                    {
                        await message.AckNackCallbacks.Nack(false);
                    }
                }
            };
            

            subscription.OnFailure += ex =>
            {
                Console.WriteLine("OnFailure callback called. Exception: " + ex);
                return Task.CompletedTask;
            };
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            Console.WriteLine($"Will start consuming queue {queueName} with subscription1.");

            await subscription.StartConsuming();

            Console.WriteLine($"Started consuming queue {queueName} with subscription1.");

            await Task.Delay(TimeSpan.FromSeconds(2000), cancellationToken);

            await subscription.StopConsuming();

            Console.WriteLine($"Stopped consuming queue {queueName}");

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            
            Console.WriteLine("Will close channel");

            await subscription.CloseChannel();

            Console.WriteLine($"Closed channel for queue {queueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something failed testing queue {queueName}. Exception: " + ex);
        }
    }
}

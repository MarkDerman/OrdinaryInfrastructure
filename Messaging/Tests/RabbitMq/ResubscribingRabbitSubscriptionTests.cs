using Odin.Messaging.RabbitMq;
using System.Text;

namespace Tests.Odin.Messaging.RabbitMq;

public class ResubscribingRabbitSubscriptionTests()
{


    public async Task ResubscribingSubscription_Works(CancellationToken cancellationToken)
    {
        try
        {
            RabbitConnectionService connectionService = new RabbitConnectionService(new RabbitConnectionServiceSettings
            {
                Host = "localhost",
                VirtualHost = "odin-rabbitbox",
                Username = "rabbitbox-test",
                UserPassword = "rabbitbox-test-01",
                Port = 5673,
                ConnectionName = "RabbitBoxIntegTests",
                MaxChannels = 10,
                SendTimeoutMillis = 5000,
            });

            string queueName = "max-length-test-01";

            ResubscribingRabbitSubscriptionFactory factory = new ResubscribingRabbitSubscriptionFactory(
                connectionService,
                queueName,
                false,
                true,
                260,
                checkChannelPeriod: TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4)
            );

            IResubscribingRabbitSubscription subscription = factory.Create();
            // RabbitConnectionServiceTests.TestMessage? mostRecent = null;

            subscription.OnFailure += ex =>
            {
                Console.WriteLine("OnFailure fired: " + ex);
                return Task.CompletedTask;
            };

            subscription.OnConsumed += async message =>
            {
                // var body = JsonSerializer.Deserialize<RabbitConnectionServiceTests.TestMessage>(message.Body);

                string body = Encoding.UTF8.GetString(message.Body);

                // Console.WriteLine("Received message " + body);

                // await Task.Delay(TimeSpan.FromSeconds(20));
                //
                // Console.WriteLine("StopConsuming");
                // subscription.StopConsuming();

                // await Task.Delay(TimeSpan.FromSeconds(3));
                //
                // Console.WriteLine("Closing channel");
                // await subscription.DisposeAsync();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                try
                {
                    await message.AckNackCallbacks!.Ack.Invoke();
                    Console.WriteLine("Acked message " + body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to ack message " + body + "\nException: " + ex);
                }

                // await subscription.DisposeAsync();
                //
                // Console.WriteLine("Disposed subscription");


            };

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            await subscription.StartConsuming();

            Console.WriteLine("Started consuming");

            await Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);

            Console.WriteLine("StopConsuming....");
            await subscription.StopConsuming();

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            Console.WriteLine("Closing subscription");
            await subscription.DisposeAsync();

            // _ = Task.Run(async () =>
            // {
            //     while (true)
            //     {
            //         await Task.Delay(5000);
            //         Console.WriteLine("Most recent message: " + mostRecent);
            //     }
            // });

            await Task.Delay(TimeSpan.FromHours(3), cancellationToken);

            // await subscription.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("ResubscribingSubscription_Works failed: " + ex);
        }

    }
}
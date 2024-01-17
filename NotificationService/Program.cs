// See https://aka.ms/new-console-template for more information

using Helpdesk.Api;
using Microsoft.Extensions.Hosting;
using Oakton;
using Wolverine;
using Wolverine.RabbitMQ;

return await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.UseRabbitMq();

        opts.ListenToRabbitQueue("notifications");
    }).RunOaktonCommands(args);


public static class RingAllTheAlarmsHandler
{
    public static void Handle(RingAllTheAlarms message)
    {
        Console.WriteLine("I'm going to scream out an alert about incident " + message.IncidentId);
    }
}
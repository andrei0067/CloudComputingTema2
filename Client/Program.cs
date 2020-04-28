using System;
using System.Globalization;
using Grpc.Core;
using ZodiacFinderGrpc;

namespace ZodiacClient
{
    class Program
    {
        private static bool IsDateValid(string date)
        {
            var dateFormats = new[] {"dd/MM/yyyy","dd/MM/yyy","dd/MM/yy","dd/MM/y"};
            Console.WriteLine(date);
            return DateTime.TryParseExact(
                date,
                dateFormats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None, 
                out _);
        }
        
        private static RequestBirth getBirthday()
        {
            Console.Write("Ziua de nastere : ");
            var dateOfBirthStr = Console.ReadLine();
            if (dateOfBirthStr==null || !IsDateValid(dateOfBirthStr))
            {
                Console.WriteLine("Data invalida!");
                System.Environment.Exit(1);
            }

            var fields = dateOfBirthStr.Split('/');

            var rb=new RequestBirth();

            rb.Day = Int32.Parse(fields[0]);
            rb.Mo = Int32.Parse(fields[1]);
            rb.Year = Int32.Parse(fields[2]);
            
            return rb;
        }
        
        public static void Main(string[] args)
        {
            Channel channel = new Channel("localhost:50002", ChannelCredentials.Insecure);

            var client = new ZodiacService.ZodiacServiceClient(channel);
            var birthRequest = getBirthday();
            
            var reply = client.ZodieFinder(birthRequest);
            Console.WriteLine("Zodie : "+ reply.Sign);

            Console.ReadKey();
            channel.ShutdownAsync().Wait();
        }
    }
}
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using ZodiacFinderGrpc;

namespace ZodiacFinder
{
    class ZodiacFinderImpl : ZodiacService.ZodiacServiceBase
    {
        private static string ZodieFile ="../../../zodiac.txt";
        
        public override Task<Reply> ZodieFinder(RequestBirth request, ServerCallContext context)
        {
            Console.WriteLine("Request: "+request.Day+"/"+request.Mo+"/"+request.Year);

            Task<Reply> reply=null;

            try
            {
                reply = Task.FromResult(FindSignForDate(request));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            return reply;
        }

        public static SignRange[] ReadFile()
        {
            var fileContent = System.IO.File.ReadAllText(ZodieFile);
            var lines = fileContent.Split("\n");
            var signRanges = new SignRange[12];

            for (var i=0; i<lines.Length; i++)
            {
                var fields = lines[i].Split(";");
                var start = fields[0];
                var end = fields[1];

                var signIndex = Int32.Parse(fields[2]);

                var startDay = Int32.Parse(start.Split("/")[0]);
                var startMonth = Int32.Parse(start.Split("/")[1]);
                var endDay = Int32.Parse(end.Split("/")[0]);
                var endMonth = Int32.Parse(end.Split("/")[1]);

                var signRange=new SignRange{StartDay = startDay, StartMonth = startMonth, EndDay = endDay, EndMonth = endMonth, SignIndex = signIndex};
                
                signRanges[i] = signRange;
            }

            return signRanges;
        }

        private Reply FindSignForDate(RequestBirth birthDate)
        {
            var day = birthDate.Day;
            var month = birthDate.Mo;
            var year = birthDate.Year;
            
            var sign = ZodiacSign.Varsator;

            var signRanges = ReadFile();

            int signIndex = -1;
            foreach (var signRange in signRanges)
            {
                if ((month == signRange.StartMonth && day>=signRange.StartDay) || (month == signRange.EndMonth && day<=signRange.EndDay) )
                {
                    signIndex = signRange.SignIndex;
                    break;
                }
            }

            if (signIndex > -1)
            {   
                return new Reply() {Sign = (ZodiacSign)signIndex};
            }
            else
            {
                Console.WriteLine("Nu s-a putut afla zodia");
                return null;
            }
        }
    }

    class Program
    {
        const int Port = 50002;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { ZodiacService.BindService(new ZodiacFinderImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            
            Console.WriteLine("Server functioneaza pe portul : " + Port);
            Console.ReadKey();
            
            server.ShutdownAsync().Wait();
        }
    }
}
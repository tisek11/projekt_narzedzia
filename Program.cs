using System;
using System.Net.Sockets;
using System.Net;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using Nuntius.Notifications;
using Nuntius.Connections;

namespace Nuntius
{
    class Program
    {
        //private Nuntius.localisationPlacer language;

        private static string ActualSum { get; set; }

        private static string GetcurrentSum()
        { return Convert.ToString(DateTime.Now.Year - (DateTime.Today.Year - (DateTime.Today.Year % 100))) + Convert.ToString(DateTime.Now.DayOfYear) + Convert.ToString(DateTime.Now.Hour) + Convert.ToString(DateTime.Now.Minute); }

        private static SqlConnection NuntiusDB = new SqlConnection(@"Data source=147.135.211.194;
                                                                    database=NuntiusDB;
                                                                    Integrated Security=false;
                                                                    UID=sa;
                                                                    PWD=Hassann!");

        

        

        private static SqlCommand insertWarnings = new SqlCommand(@"INSERT INTO [dbo].[Warnings]
                                                                   ([type]
                                                                   ,[callDatetime]
                                                                   ,[heading]
                                                                   ,[descryption]
                                                                   ,[priority])
                                                                VALUES
                                                                   (@type
                                                                   ,@callDatetime
                                                                   ,@heading
                                                                   ,@desc
                                                                   ,@priority)", NuntiusDB);

        private static SqlCommand selectWarnings = new SqlCommand(@"SELECT TOP (1000) [id]
                                                                    ,[type]
                                                                    ,[callDatetime]
                                                                    ,[heading]
                                                                    ,[descryption]
                                                                    ,[priority]
                                                                FROM[NuntiusDB].[dbo].[Warnings] WHERE [priority] <> 0", NuntiusDB);

        private static Queue<EngineConnection> awaitingConnections = new Queue<EngineConnection>();
        private static List<Thread> activeThreads = new List<Thread>();

        private static List<Notification> actualNotification = new List<Notification>();

        private static Thread threadsManagementThread = new Thread(ThreadsManagement);
        private static Thread clientListingThread = new Thread(ClientListing);
        private static Thread readClientThread = new Thread(ReadAdmin);

        private static Object ReturnActualWarnings(bool serialized)
        {
            List<Notification> notifications = new List<Notification>();

            using (SqlDataReader reader = selectWarnings.ExecuteReader())
            {
                while (reader.Read())
                {
                    notifications.Add(new Notification((int)reader["id"], (string)reader["type"], (string)reader["heading"], (string)reader["descryption"], (DateTime)reader["callDatetime"], (byte)reader["priority"]));
                    reader.NextResult();
                }
            }

            if (serialized)
            {
                MemoryStream memoryStream = new MemoryStream();
                (new BinaryFormatter()).Serialize(memoryStream, notifications);

                return memoryStream.ToArray();
            }
            
            return notifications;
        }

        private static void ReadAdmin()
        {
            List<Notification> actualNotification = (List<Notification>)ReturnActualWarnings(false);

            /*Console.WriteLine(actualNotification.Count);

            foreach (Notification x in actualNotification)
            {
                x.WriteLine();
            }
            /*Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1024));
            serverSocket.Listen(10);*/
    }

    private static void ReadClient(object client) 
        {
            Thread.CurrentThread.Name = ((EngineConnection)client).Address;

            try
            {
                if (((EngineConnection)client).socket.Connected == true) Console.WriteLine("Połączenie z {0} otwarte!", ((EngineConnection)client).Address);
                {
                    byte[] x = (byte[])ReturnActualWarnings(true);
                    ((EngineConnection)client).socket.Send(Encoding.ASCII.GetBytes(x.Length.ToString()));

                    ((EngineConnection)client).socket.Send(x, x.Length, SocketFlags.None);
                    ((EngineConnection)client).WipeOut(userNewLog.Clone());
                }
            }

            catch (SocketException) { ((EngineConnection)client).WipeOut(userNewLog.Clone()); };
        }

        private static void ThreadsManagement()
        {
            int activeThreadsLast, awaitingConnectionsLast;
            Console.WriteLine("Aktywne: {0} Oczekujące: {1}", activeThreadsLast = activeThreads.Count, awaitingConnectionsLast = awaitingConnections.Count);

            while (true)
            {
                if (activeThreads.Count > 0)
                    activeThreads.RemoveAll(x => (x.IsAlive != true));

                if (activeThreadsLast != activeThreads.Count || awaitingConnectionsLast != awaitingConnections.Count)
                    Console.WriteLine("Aktywne: {0} Oczekujące: {1}", activeThreadsLast = activeThreads.Count, awaitingConnectionsLast = awaitingConnections.Count);

                while ((activeThreads.Count < 1) && (awaitingConnections.Count > 0))
                {
                    Thread test = new Thread(ReadClient);
                    EngineConnection client = awaitingConnections.Dequeue();

                    test.Start(client);
                    activeThreads.Add(test);
                }
            }
        }

        private static void ClientListing()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1024));
            serverSocket.Listen(10);

            Console.WriteLine("Gotowy do listowania połączeń!");
            while (true)
                awaitingConnections.Enqueue(new EngineConnection(serverSocket.Accept()));
        }

        static void Main(string[] args)
        {
            NuntiusDB.Open();
            ActualSum = GetcurrentSum();
            Console.WriteLine("Stan połączenia z bazą Nuntiusa: {0}, obecna suma kontrolna to: {1}", NuntiusDB.State, ActualSum);

            ReturnActualWarnings(true);

            ReadAdmin();

            threadsManagementThread.Name = "threadsManagement";
            clientListingThread.Name = "clientListing";

            threadsManagementThread.Start();
            clientListingThread.Start();
            Thread.Sleep(1000000);

        }
    }
}

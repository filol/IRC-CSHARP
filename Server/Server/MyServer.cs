using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Server
{
    class Server
    {
        //Recommended port 7581
        private int port;
        private static Dictionary<string, Dictionary<string, TcpClient>> chanels;
        public Server(int port)
        {
            this.port = port;
        }

        public void start()
        {
            //Ajout du channel par défaut
            chanels = new Dictionary<string, Dictionary<string, TcpClient>>();
            chanels.Add("Général",new Dictionary<string, TcpClient>());

            //Connexion
            TcpListener l = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            l.Start();
            Console.WriteLine("Server run on 127.0.0.1:"+port);

            while (true)
            {
                TcpClient comm = l.AcceptTcpClient();
                Console.WriteLine("Connection established @" + comm);
                new Thread(new Receiver(comm).doOperation).Start();
            }
        }

        public static bool nickExist(string nickname)
        {
            foreach (KeyValuePair<string, Dictionary<string, TcpClient>> chanel in chanels)
            {
                if (chanel.Value.ContainsKey(nickname))
                {
                    return true;
                }
            }
            return false;
        }

        class Receiver
        {
            private TcpClient comm;

            public Receiver(TcpClient s)
            {
                comm = s;
            }

            public void doOperation()
            {
                while (true)
                {
                    try
                    {
                        // read expression
                        Message msg = Utils.Utils.rcvMsg(comm.GetStream());

                        Console.WriteLine("code = " + msg.code + " message received by " + msg.author + " say in " + msg.channel + " : " + msg.message);
                        //Here computing the message

                        //Demande de connexion au serveur
                        if (msg.channel == "Général" && msg.code == -1)
                        {
                            if (nickExist(msg.author))
                            {
                                Message newMessage = new Message();
                                newMessage.author = "Server";
                                newMessage.message = "Error pseudo already exist";
                                newMessage.channel = "Général";
                                newMessage.code = 1;
                                Thread thread = new Thread(() => Utils.Utils.sendMsg(comm.GetStream(), newMessage));
                                thread.Start();

                            }
                            else
                            {
                                Message newMessage = new Message();
                                newMessage.author = "Server";
                                newMessage.message = "Error pseudo already exist";
                                newMessage.channel = "Général";
                                newMessage.code = 1;

                                Thread thread = new Thread(() => SendMessageToAll(chanels[msg.channel], newMessage));
                                thread.Start();

                                chanels[msg.channel].Add(msg.author, comm);

                                Message welcomeMessage = new Message();
                                welcomeMessage.author = "Server";
                                welcomeMessage.message = "Welcome !";
                                welcomeMessage.channel = "Général";
                                welcomeMessage.code = 0;
                                Thread thread2 = new Thread(() => Utils.Utils.sendMsg(comm.GetStream(), welcomeMessage));
                                thread2.Start();

                            }
                        }

                        if (msg.code == 0)
                        {
                            try
                            {
                                Console.WriteLine("Message send to all client");
                                Thread thread = new Thread(() => SendMessageToAll(chanels[msg.channel], msg));
                                thread.Start();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Disconnected");
                            }


                        }
                    
                    }
                    catch (System.InvalidOperationException e)
                    {
                        Console.WriteLine("Disconnected");
                    }
                }

            }

            public void SendMessageToAll(Dictionary<string, TcpClient> chanel, Message message)
            {
                foreach (TcpClient comm in chanel.Values)
                {
                    Console.WriteLine("Message send to "+comm);
                    Utils.Utils.sendMsg(comm.GetStream(), message);
                }
            }
        }
    }
}

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
        //Recommended port 7581 // 8976
        private int port;
        private static Dictionary<string, Dictionary<string, TcpClient>> chanels;
        public Server(int port)
        {
            this.port = port;
        }

        private static List<string> _channelList;
        private static List<string> channelList
        {
            get
            {
                _channelList = new List<string>();
                foreach (string chanel in chanels.Keys)
                {
                    Console.WriteLine("SERVERChannels : " +chanel);
                    _channelList.Add(chanel);
                }
                Console.WriteLine(_channelList.ToString());
                return _channelList;
            }
        }

        public void start()
        {
            //Ajout du channel par défaut
            chanels = new Dictionary<string, Dictionary<string, TcpClient>>();
            chanels.Add("Général", new Dictionary<string, TcpClient>());

            //Connexion
            TcpListener l = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            l.Start();
            Console.WriteLine("Server run on 127.0.0.1:" + port);

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
                                Console.WriteLine("SERVER : Erreur pseudo already exist");
                                Message newMessage = new Message();
                                newMessage.author = "Server";
                                newMessage.message = "Error pseudo already exist";
                                newMessage.channel = "Général";
                                //newMessage.ChannelList = channelList;
                                newMessage.code = 1;
                                Utils.Utils.sendMsg(comm.GetStream(), newMessage);
                                //thread.Start();

                            }
                            else
                            {
                                Message welcomeMessage = new Message();
                                welcomeMessage.author = "Server";
                                welcomeMessage.message = "Welcome !";
                                welcomeMessage.channel = "Général";
                                //welcomeMessage.ChannelList = channelList;
                                welcomeMessage.code = 0;
                                Utils.Utils.sendMsg(comm.GetStream(), welcomeMessage);


                                Console.WriteLine("SERVER : welcome");
                                Message newMessage = new Message();
                                newMessage.author = "Server";
                                newMessage.message = msg.author + " vient de se connecter";
                                newMessage.channel = "Général";
                                //newMessage.ChannelList = channelList;
                                newMessage.code = 0;

                                //Console.WriteLine("SERVER : list channel");
                                //foreach (string chanel in newMessage.ChannelList)
                                //{
                                //    Console.WriteLine(chanel);
                                //}

                                SendMessageToAllInChannel(chanels[msg.channel], newMessage);

                                chanels[msg.channel].Add(msg.author, comm);

                                //On envoie tous les channels existant au ptit nouveau
                                if (chanels.Keys.Count>1)
                                {
                                    foreach (string channel in chanels.Keys)
                                    {
                                        Utils.Utils.sendMsg(comm.GetStream(), new Message(channel, 3));
                                    }
                                }
                            }
                        }

                        if (msg.code == 0)
                        {
                            try
                            {
                                Console.WriteLine("Message send to all client");
                                //msg.ChannelList = channelList;
                                SendMessageToAllInChannel(chanels[msg.channel], msg);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Disconnected");
                            }


                        }

                        if (msg.code == 2) //Création/Join d'un nouveau channel
                        {

                            Message message = new Message();
                            if (chanels.ContainsKey(msg.channel)) //le channel existe déjà
                            {
                                if (!chanels[msg.channel].ContainsKey(msg.author))
                                {
                                    chanels[msg.channel].Add(msg.author, comm);
                                    message.channel = msg.channel;
                                    message.message = msg.author + " a rejoint : " + msg.channel;
                                    message.code = 0;
                                    message.author = "Server";
                                    //message.ChannelList = channelList;
                                    try
                                    {
                                        Console.WriteLine("Message send to all client");
                                        SendMessageToAllInChannel(chanels[message.channel], message);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Disconnected");
                                    }
                                }
                            }
                            else //le channel n'existe pas
                            {
                                chanels.Add(msg.channel,new Dictionary<string, TcpClient>());
                                chanels[msg.channel].Add(msg.author, comm);
                                Utils.Utils.sendMsg(comm.GetStream(),new Message(msg.channel, 3));
                                message.channel = msg.channel;
                                message.message = msg.author + " a créé : " + msg.channel;
                                message.code = 0;
                                message.author = "Server";
                                //message.ChannelList = channelList;
                                try
                                {
                                    Console.WriteLine("Message send to all client");
                                    SendMessageToAll(message);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Disconnected");
                                }
                            }
                            
                        }

                    }
                    catch (System.InvalidOperationException e)
                    {
                        Console.WriteLine("Disconnected");
                    }
                }
            }

            public void SendMessageToAllInChannel(Dictionary<string, TcpClient> chanel, Message message)
            {
                foreach (TcpClient comm in chanel.Values)
                {
                    Console.WriteLine("Message send to " + comm);
                    //Console.WriteLine("SERVER : list channel");
                    //foreach (string channel in message.ChannelList)
                    //{
                    //    Console.WriteLine(channel);
                    //}
                    Utils.Utils.sendMsg(comm.GetStream(), message);
                }
            }

            public void SendMessageToAll(Message message)
            {
                List<TcpClient> list = new List<TcpClient>();
                foreach (Dictionary<string, TcpClient> chanel in chanels.Values)
                {
                    foreach (TcpClient comm in chanel.Values)
                    {
                        if (!list.Contains(comm))
                        {
                            list.Add(comm);
                            Console.WriteLine("Message send to " + comm);
                            Utils.Utils.sendMsg(comm.GetStream(), message);
                        }
                    }
                }
                
            }
        }
    }
}

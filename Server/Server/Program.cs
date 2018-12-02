using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(7581);
            server.start();

            //MyServer startIt = new MyServer();
            //startIt.Start();
        }
    }

    //public class MyServer : ForwardToAll
    //{
    //    private ArrayList readList = new ArrayList(); //liste utilisée par socket.select 
    //    private string msgString = null; //contiendra le message envoyé aux autres clients
    //    private string msgDisconnected = null; //Notification connexion/déconnexion
    //    private string messageSerialized;
    //    private Message message;
    //    public bool readLock = false;//Flag aidant à la synchronisation
    //    private string rtfMsgEncStart = "\\pard\\cf1\\b0\\f1 ";//Code RTF
    //    private string rtfMsgContent = "\\cf2 ";//code RTF
    //    private string rtfConnMsgStart = "\\pard\\qc\\b\\f0\\fs20 "; //Code RTF
    //    public void Start()
    //    {
    //        //réception de l'adresse IP locale
    //        IPHostEntry ipHostEntry = Dns.Resolve(Dns.GetHostName());
    //        IPAddress ipAddress = ipHostEntry.AddressList[0];
    //        Console.WriteLine("IP du serveur = " + ipAddress.ToString());
    //        Socket CurrentClient = null;
    //        //Création de la socket
    //        Socket ServerSocket = new Socket(AddressFamily.InterNetwork,
    //          SocketType.Stream,
    //          ProtocolType.Tcp);
    //        try
    //        {
    //            //On lie la socket au point de communication
    //            ServerSocket.Bind(new IPEndPoint(ipAddress, 8000));
    //            //On la positionne en mode "écoute"
    //            ServerSocket.Listen(10);
    //            //Démarrage du thread avant la première connexion client
    //            Thread getReadClients = new Thread(new ThreadStart(getRead));
    //            getReadClients.Start();
    //            //Démarrage du thread vérifiant l'état des connexions clientes
    //            Thread pingPongThread = new Thread(new ThreadStart(CheckIfStillConnected));
    //            pingPongThread.Start();
    //            //Boucle infinie
    //            while (true)
    //            {
    //                Console.WriteLine("Attente d'une nouvelle connexion...");
    //                //L'exécution du thread courant est bloquée jusqu'à ce qu'un
    //                //nouveau client se connecte
    //                CurrentClient = ServerSocket.Accept();
    //                Console.WriteLine("Nouveau client:" + CurrentClient.GetHashCode());
    //                //Stockage de la ressource dans l'arraylist acceptlist
    //                acceptList.Add(CurrentClient);
    //            }
    //        }
    //        catch (SocketException E)
    //        {
    //            Console.WriteLine(E.Message);
    //        }

    //    }

    //    private void getConnected()
    //    {

    //        foreach (object item in MatchList.Values)
    //        {
    //            Console.WriteLine(item);
    //        }
    //    }

    //    //Méthode démarrant l'écriture du message reçu par un client
    //    //vers tous les autres clients
    //    private void writeToAll()
    //    {
    //        base.SendMsg(message);
    //    }
    //    private void infoToAll()
    //    {
    //        base.SendMsg(message);
    //    }

    //    private void CheckIfStillConnected()
    //    {
    //        /* Etant donné que la propriété .Connected d'une socket n'est pas
    //         * mise à jour lors de la déconnexion d'un client sans que l'on ait
    //         * prélablement essayé de lire ou d'écrire sur cette socket, cette méthode
    //         * parvient à déterminer si une socket cliente s'est déconnectée grce à la méthode
    //         * poll. On effectue un poll en lecture sur la socket, si le poll retourne vrai et que
    //         * le nombre de bytes disponible est 0, il s'agit d'une connexion terminée*/
    //        while (true)
    //        {
    //            for (int i = 0; i < acceptList.Count; i++)
    //            {
    //                if (((Socket)acceptList[i]).Poll(10, SelectMode.SelectRead) && ((Socket)acceptList[i]).Available == 0)
    //                {
    //                    if (!readLock)
    //                    {
    //                        Console.WriteLine("Client " + ((Socket)acceptList[i]).GetHashCode() + " déconnecté");
    //                        removeNick(((Socket)acceptList[i]));
    //                        ((Socket)acceptList[i]).Close();
    //                        acceptList.Remove(((Socket)acceptList[i]));
    //                        i--;
    //                    }
    //                }
    //            }
    //            Thread.Sleep(5);
    //        }
    //    }
    //    //Vérifie que le pseudo n'est pas déjà attribué à un autre utilisateur
    //    //La Hashtable matchlist ne sert qu'à ça. Pour des développements ultérieurs, elle
    //    //pourrait aussi servir à envoyer la liste de tous les connectés aux utilisateurs
    //    private bool checkNick(string nick, Socket Resource)
    //    {
    //        if (MatchList.ContainsValue(nick))
    //        {
    //            //Le pseudo est déjà pris, on refuse la connexion.
    //            ((Socket)acceptList[acceptList.IndexOf(Resource)]).Shutdown(SocketShutdown.Both);
    //            ((Socket)acceptList[acceptList.IndexOf(Resource)]).Close();
    //            acceptList.Remove(Resource);
    //            Console.WriteLine("Pseudo déjà pris");
    //            return false;
    //        }
    //        else
    //        {
    //            MatchList.Add(Resource, nick);
    //            getConnected();
    //        }
    //        return true;
    //    }

    //    //Lorsqu'un client se déconnecte, il faut supprimer le pseudo associé à cette connexion
    //    private void removeNick(Socket Resource)
    //    {
    //        Console.Write("DECONNEXION DE:" + MatchList[Resource]);
    //        msgDisconnected = rtfConnMsgStart + ((string)MatchList[Resource]).Trim() + " vient de se déconnecter!\\par";
    //        Thread DiscInfoToAll = new Thread(new ThreadStart(infoToAll));
    //        DiscInfoToAll.Start();
    //        DiscInfoToAll.Join();
    //        MatchList.Remove(Resource);
    //    }

    //    //Cette méthode est exécutée dans un thread à part
    //    //Elle lit en permanence l'état des sockets connectées et
    //    //vérifie si celles-ci tentent d'envoyer quelque chose
    //    //au serveur. Si tel est le cas, elle réceptionne les paquets
    //    //et appelle forwardToAll pour renvoyer ces paquets vers
    //    //les autres clients.
    //    private void getRead()
    //    {
    //        while (true)
    //        {
    //            readList.Clear();
    //            for (int i = 0; i < acceptList.Count; i++)
    //            {
    //                readList.Add((Socket)acceptList[i]);
    //            }
    //            if (readList.Count > 0)
    //            {
    //                Socket.Select(readList, null, null, 1000);
    //                for (int i = 0; i < readList.Count; i++)
    //                {
    //                    if (((Socket)readList[i]).Available > 0)
    //                    {
    //                        readLock = true;
    //                        int paquetsReceived = 0;
    //                        long sequence = 0;
    //                        string Nick = null;
    //                        string formattedMsg = null;
    //                        byte[] buffer;
    //                        while (((Socket)readList[i]).Available > 0)
    //                        {
    //                            BinaryFormatter bf = new BinaryFormatter();
    //                            message = (Message)bf.Deserialize(new NetworkStream(((Socket)readList[i])));

    //                            if (paquetsReceived == 0)
    //                            {
    //                                string seq = msgString.Substring(0, 6);
    //                                sequence = Convert.ToInt64(seq);
    //                                Nick = msgString.Substring(6, 15);
    //                                formattedMsg = rtfMsgEncStart + Nick.Trim() + " a écrit:" + rtfMsgContent +
    //                                msgString.Substring(20, (msgString.Length - 20)) + "\\par";
    //                            }
    //                            else
    //                            {
    //                                formattedMsg = rtfMsgContent + msgString + "\\par";
    //                            }
    //                            msg = System.Text.Encoding.UTF8.GetBytes(formattedMsg);
    //                            if (sequence == 1)
    //                            {
    //                                if (!checkNick(Nick, ((Socket)readList[i])))
    //                                {
    //                                    break;
    //                                }
    //                                else
    //                                {
    //                                    string rtfMessage = rtfConnMsgStart + Nick.Trim() + " vient de se connecter\\par";
    //                                    msg = System.Text.Encoding.UTF8.GetBytes(rtfMessage);
    //                                }
    //                            }
    //                            //Démarrage du thread renvoyant le message à tous les clients
    //                            Thread forwardingThread = new Thread(new ThreadStart(writeToAll));
    //                            forwardingThread.Start();
    //                            forwardingThread.Join();
    //                            paquetsReceived++;
    //                        }
    //                        readLock = false;
    //                    }
    //                }
    //            }
    //            Thread.Sleep(10);
    //        }
    //    }
    //}


    //public class ForwardToAll
    //{
    //    public ArrayList acceptList = new ArrayList();
    //    public Hashtable MatchList = new Hashtable();

    //    public void SendMsg(Message message)
    //    {
    //        for (int i = 0; i < acceptList.Count; i++)
    //        {
    //            if (((Socket)acceptList[i]).Connected)
    //            {
    //                try
    //                {
    //                    Utils.sendMsg(new NetworkStream(((Socket)acceptList[i])),message);
    //                    Console.WriteLine("Writing to:" + acceptList.Count.ToString());
    //                }
    //                catch
    //                {
    //                    Console.Write(((Socket)acceptList[i]).GetHashCode() + " déconnecté");
    //                }
    //            }
    //            else
    //            {
    //                acceptList.Remove((Socket)acceptList[i]);
    //                i--;
    //            }
    //        }
    //    }
    //}
}

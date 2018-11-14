using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server startIt = new Server();
            startIt.Start();
        }
    }

    public class Server : forwardToAll
    {
        ArrayList readList = new ArrayList(); //liste utilisée par socket.select 
        string msgString = null; //contiendra le message envoyé aux autres clients
        string msgDisconnected = null; //Notification connexion/déconnexion
        byte[] msg;//Message sous forme de bytes pour socket.send et socket.receive
        public bool readLock = false;//Flag aidant à la synchronisation
        private string rtfMsgEncStart = "\\pard\\cf1\\b0\\f1 ";//Code RTF
        private string rtfMsgContent = "\\cf2 ";//code RTF
        private string rtfConnMsgStart = "\\pard\\qc\\b\\f0\\fs20 "; //Code RTF
        public void Start()
        {
            //réception de l'adresse IP locale
            IPHostEntry ipHostEntry = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostEntry.AddressList[0];
            Console.WriteLine("IP=" + ipAddress.ToString());
            Socket CurrentClient = null;
            //Création de la socket
            Socket ServerSocket = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream,
              ProtocolType.Tcp);
            try
            {
                //On lie la socket au point de communication
                ServerSocket.Bind(new IPEndPoint(ipAddress, 8000));
                //On la positionne en mode "écoute"
                ServerSocket.Listen(10);
                //Démarrage du thread avant la première connexion client
                Thread getReadClients = new Thread(new ThreadStart(getRead));
                getReadClients.Start();
                //Démarrage du thread vérifiant l'état des connexions clientes
                Thread pingPongThread = new Thread(new ThreadStart(CheckIfStillConnected));
                pingPongThread.Start();
                //Boucle infinie
                while (true)
                {
                    Console.WriteLine("Attente d'une nouvelle connexion...");
                    //L'exécution du thread courant est bloquée jusqu'à ce qu'un
                    //nouveau client se connecte
                    CurrentClient = ServerSocket.Accept();
                    Console.WriteLine("Nouveau client:" + CurrentClient.GetHashCode());
                    //Stockage de la ressource dans l'arraylist acceptlist
                    acceptList.Add(CurrentClient);
                }
            }
            catch (SocketException E)
            {
                Console.WriteLine(E.Message);
            }

        }

        private void getConnected()
        {

            foreach (object item in MatchList.Values)
            {
                Console.WriteLine(item);
            }
        }

        //Méthode démarrant l'écriture du message reu par un client
        //vers tous les autres clients
        private void writeToAll()
        {
            base.sendMsg(msg);
        }
        private void infoToAll()
        {
            base.sendMsg(msgDisconnected);
        }

        private void CheckIfStillConnected()
        {
            /* Etant donné que la propriété .Connected d'une socket n'est pas
             * mise à jour lors de la déconnexion d'un client sans que l'on ait
             * prélablement essayé de lire ou d'écrire sur cette socket, cette méthode
             * parvient à déterminer si une socket cliente s'est déconnectée grce à la méthode
             * poll. On effectue un poll en lecture sur la socket, si le poll retourne vrai et que
             * le nombre de bytes disponible est 0, il s'agit d'une connexion terminée*/
            while (true)
            {
                for (int i = 0; i < acceptList.Count; i++)
                {
                    if (((Socket)acceptList[i]).Poll(10, SelectMode.SelectRead) && ((Socket)acceptList[i]).Available == 0)
                    {
                        if (!readLock)
                        {
                            Console.WriteLine("Client " + ((Socket)acceptList[i]).GetHashCode() + " déconnecté");
                            removeNick(((Socket)acceptList[i]));
                            ((Socket)acceptList[i]).Close();
                            acceptList.Remove(((Socket)acceptList[i]));
                            i--;
                        }
                    }
                }
                Thread.Sleep(5);
            }
        }
        //Vérifie que le pseudo n'est pas déjà attribué à un autre utilisateur
        //La Hashtable matchlist ne sert qu'à ça. Pour des développements ultérieurs, elle
        //pourrait aussi servir à envoyer la liste de tous les connectés aux utilisateurs
        private bool checkNick(string nick, Socket Resource)
        {
            if (MatchList.ContainsValue(nick))
            {
                //Le pseudo est déjà pris, on refuse la connexion.
                ((Socket)acceptList[acceptList.IndexOf(Resource)]).Shutdown(SocketShutdown.Both);
                ((Socket)acceptList[acceptList.IndexOf(Resource)]).Close();
                acceptList.Remove(Resource);
                Console.WriteLine("Pseudo déjà pris");
                return false;
            }
            else
            {
                MatchList.Add(Resource, nick);
                getConnected();
            }
            return true;
        }

        //Lorsqu'un client se déconnecte, il faut supprimer le pseudo associé à cette connexion
        private void removeNick(Socket Resource)
        {
            Console.Write("DECONNEXION DE:" + MatchList[Resource]);
            msgDisconnected = rtfConnMsgStart + ((string)MatchList[Resource]).Trim() + " vient de se déconnecter!\\par";
            Thread DiscInfoToAll = new Thread(new ThreadStart(infoToAll));
            DiscInfoToAll.Start();
            DiscInfoToAll.Join();
            MatchList.Remove(Resource);
        }

        //Cette méthode est exécutée dans un thread à part
        //Elle lit en permanence l'état des sockets connectées et
        //vérifie si celles-ci tentent d'envoyer quelque chose
        //au serveur. Si tel est le cas, elle réceptionne les paquets
        //et appelle forwardToAll pour renvoyer ces paquets vers
        //les autres clients.
        private void getRead()
        {
            while (true)
            {
                readList.Clear();
                for (int i = 0; i < acceptList.Count; i++)
                {
                    readList.Add((Socket)acceptList[i]);
                }
                if (readList.Count > 0)
                {
                    Socket.Select(readList, null, null, 1000);
                    for (int i = 0; i < readList.Count; i++)
                    {
                        if (((Socket)readList[i]).Available > 0)
                        {
                            readLock = true;
                            int paquetsReceived = 0;
                            long sequence = 0;
                            string Nick = null;
                            string formattedMsg = null;
                            while (((Socket)readList[i]).Available > 0)
                            {
                                msg = new byte[((Socket)readList[i]).Available];
                                ((Socket)readList[i]).Receive(msg, msg.Length, SocketFlags.None);
                                msgString = System.Text.Encoding.UTF8.GetString(msg);
                                if (paquetsReceived == 0)
                                {
                                    string seq = msgString.Substring(0, 6);
                                    try
                                    {
                                        sequence = Convert.ToInt64(seq);
                                        Nick = msgString.Substring(6, 15);
                                        formattedMsg = rtfMsgEncStart + Nick.Trim() + " a écrit:" + rtfMsgContent +
                                        msgString.Substring(20, (msgString.Length - 20)) + "\\par";
                                    }
                                    catch
                                    {
                                        //Ce cas de figure ne devrait normalement
                                        //jamais se produire. Il peut se produire uniquement
                                        //si un client développé par quelqu'un d'autre
                                        //tente de se connecter sur le serveur.
                                        Console.Write("Message non conforme");
                                        acceptList.Remove(((Socket)readList[i]));
                                        break;
                                    }
                                }
                                else
                                {
                                    formattedMsg = rtfMsgContent + msgString + "\\par";
                                }
                                msg = System.Text.Encoding.UTF8.GetBytes(formattedMsg);
                                if (sequence == 1)
                                {
                                    if (!checkNick(Nick, ((Socket)readList[i])))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        string rtfMessage = rtfConnMsgStart + Nick.Trim() + " vient de se connecter\\par";
                                        msg = System.Text.Encoding.UTF8.GetBytes(rtfMessage);
                                    }
                                }
                                //Démarrage du thread renvoyant le message à tous les clients
                                Thread forwardingThread = new Thread(new ThreadStart(writeToAll));
                                forwardingThread.Start();
                                forwardingThread.Join();
                                paquetsReceived++;
                            }
                            readLock = false;
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
    }


    public class forwardToAll
    {
        public ArrayList acceptList = new ArrayList();
        public Hashtable MatchList = new Hashtable();
        public forwardToAll() { }
        public void sendMsg(byte[] msg)
        {
            for (int i = 0; i < acceptList.Count; i++)
            {
                if (((Socket)acceptList[i]).Connected)
                {
                    try
                    {
                        int bytesSent = ((Socket)acceptList[i]).Send(msg, msg.Length, SocketFlags.None);
                    }
                    catch
                    {
                        Console.Write(((Socket)acceptList[i]).GetHashCode() + " déconnecté");
                    }
                }
                else
                {
                    acceptList.Remove((Socket)acceptList[i]);
                    i--;
                }
            }
        }

        public void sendMsg(string message)
        {
            for (int i = 0; i < acceptList.Count; i++)
            {
                if (((Socket)acceptList[i]).Connected)
                {
                    try
                    {
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(message);
                        int bytesSent = ((Socket)acceptList[i]).Send(msg, msg.Length, SocketFlags.None);
                        Console.WriteLine("Writing to:" + acceptList.Count.ToString());
                    }
                    catch
                    {
                        Console.Write(((Socket)acceptList[i]).GetHashCode() + " déconnecté");
                    }
                }
                else
                {
                    acceptList.Remove((Socket)acceptList[i]);
                    i--;
                }
            }
        }
    }
}

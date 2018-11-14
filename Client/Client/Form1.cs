using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Socket ClientSocket = null;
        public Thread DataReceived = null;
        private Thread Flasht = null;
        public string NickName = null;
        public long sequence = 0;
        public int numberMsg = 0;
        private bool allowBlink = false;
        //Je mets cette déclaration sur 3 lignes pour ne pas avoir de scroll horizontale :)
        //C'est en fait la déclaration de début d'un document RTF (merci wordpad) 
        private const string rtfStart = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fcharset0 Arial;}{\\f1\\fswiss\\fprq2\\fcharset0 Arial;}}{\\colortbl ;\\red0\\green0\\blue128;\\red0\\green128\\blue0;}\\viewkind4\\uc1";
        private string rtfContent = null;

public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Si les paramètres de connexion ont été enregistrés, on les récupère
        //via cette méthode
        void getParams()
        {
            if (File.Exists("params.ini"))
            {
                using (StreamReader SR = new StreamReader("params.ini"))
                {
                    ServerHost.Text = SR.ReadLine();
                    Nick.Text = SR.ReadLine();
                    SR.Close();
                }
            }
        }
        //Cette méthode envoie le message sur le serveur
        void SendMsg(string message)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(message);
            int DtSent = ClientSocket.Send(msg, msg.Length, SocketFlags.None);
            if (DtSent == 0)
            {
                MessageBox.Show("Aucune donnée n'a été envoyée");
            }
        }

        //Cette méthode permet de récupérer l'adresse ip du serveur sur lequel
        //on désire se connecter
        private String GetAdr()
        {
            try
            {
                IPHostEntry iphostentry = Dns.GetHostByName(ServerHost.Text);
                String IPStr = "";
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    IPStr = ipaddress.ToString();
                    return IPStr;
                }
            }
            catch (SocketException E)
            {
                MessageBox.Show(E.Message);
            }

            return "";
        }

        //Cette méthode est appelée par un thread à part qui lit constament la socket
        //pour voir si le serveur essaye d'écrire dessus
        private void CheckData()
        {
            try
            {
                while (true)
                {
                    if (ClientSocket.Connected)
                    {
                        if (ClientSocket.Poll(10, SelectMode.SelectRead) && ClientSocket.Available == 0)
                        {
                            //La connexion a été clturée par le serveur ou bien un problème
                            //réseau est apparu
                            MessageBox.Show("La connexion au serveur est interrompue. Essayez avec un autre pseudo");
                            Connect.Enabled = true;
                            Thread.CurrentThread.Abort();
                        }
                        //Si la socket a des données à lire
                        if (ClientSocket.Available > 0)
                        {
                            string messageReceived = null;
                            while (ClientSocket.Available > 0)
                            {
                                try
                                {
                                    byte[] msg = new Byte[ClientSocket.Available];
                                    //Réception des données
                                    ClientSocket.Receive(msg, 0, ClientSocket.Available, SocketFlags.None);
                                    messageReceived = System.Text.Encoding.UTF8.GetString(msg).Trim();
                                    //On concatène les données reues(max 4ko) dans
                                    //une variable de la classe
                                    rtfContent += messageReceived;
                                }
                                catch (SocketException E)
                                {
                                    MessageBox.Show("CheckData read" + E.Message);
                                }
                            }
                            try
                            {
                                //On remplit le richtextbox avec les données reues 
                                //lorsqu'on a tout réceptionné
                                chatBody.Rtf = rtfStart + rtfContent;
                                this.BringToFront();
                            }
                            catch (Exception E)
                            {
                                MessageBox.Show(E.Message);
                            }

                        }
                    }
                    //On temporise pendant 10 millisecondes, ceci pour éviter
                    //que le micro processeur s'emballe
                    Thread.Sleep(10);
                }
            }
            catch
            {
                //Ce thread étant susceptible d'tre arrté à tout moment
                //on catch l'exception afin de ne pas afficher un message à l'utilisateur
                Thread.ResetAbort();
            }
        }
        
        //Cette méthode génère le numéro de séquence collé en
        //entte du message envoyé au serveur
        string GetSequence()
        {
            sequence++;
            string msgSeq = Convert.ToString(sequence);
            char pad = Convert.ToChar("0");
            msgSeq = msgSeq.PadLeft(6, pad);
            return msgSeq;
        }

        //Cette méthode provoque l'auto-scroll du richtextbox dès

        void HandleAutoScroll(object sender, System.EventArgs e)
        {
            chatBody.SelectionStart = chatBody.Rtf.Length;
            chatBody.Focus();
            msgArea.Focus();
        }

        void MainFormLoad(object sender, System.EventArgs e)
        {
            getParams();
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (Nick.Text == "")
            {
                MessageBox.Show("Le pseudo ne peut tre null");
                return;
            }
            if (ServerHost.Text == "")
            {
                MessageBox.Show("Le nom du serveur ne peut tre null");
                return;
            }
            //On formatte le pseudo sur une longueur de 15 caractères
            NickName = Nick.Text.Trim();
            if (NickName.Length < 15)
            {
                char pad = Convert.ToChar(" ");
                NickName = NickName.PadRight(15, pad);
            }
            else if (NickName.Length > 15)
            {
                MessageBox.Show("Le pseudo doit tre de 15 caractères maximum");
                return;
            }
            //Chaque message sera précédé d'un numéro de sequence
            //Le numéro de séquence 1 servira à identifier le pseudo
            //cté serveur.
            sequence = 0;
            IPAddress ip = IPAddress.Parse(GetAdr());
            IPEndPoint ipEnd = new IPEndPoint(ip, 8000);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ClientSocket.Connect(ipEnd);
                if (ClientSocket.Connected)
                {
                    SendMsg(GetSequence() + NickName);
                    Connect.Enabled = false;
                }


            }
            catch (SocketException E)
            {
                MessageBox.Show("Connection" + E.Message);
            }
            try
            {
                DataReceived = new Thread(new ThreadStart(CheckData));
                DataReceived.Start();
            }
            catch (Exception E)
            {
                MessageBox.Show("Démarrage Thread" + E.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Si le thread recevant les données a été démarré, on l'arrte
            if (DataReceived != null)
            {
                try
                {
                    DataReceived.Abort();
                    DataReceived.Join();
                }
                catch (Exception E)
                {
                    MessageBox.Show("Arrt Thread" + E.Message);
                }
            }
            if (ClientSocket != null && ClientSocket.Connected)
            {
                try
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();
                    if (ClientSocket.Connected)
                    {
                        MessageBox.Show("Erreur: " + Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                    }

                }
                catch (SocketException SE)
                {
                    MessageBox.Show("SE" + SE.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //On vérifie que le client est bien connecté
            if (ClientSocket == null || !ClientSocket.Connected)
            {
                MessageBox.Show("Vous n'tes pas connecté");
                return;
            }
            try
            {
                if (msgArea.Text != "")
                {
                    //Etant donné qu'on travaille en RTF, on échappe les caractères
                    //spéciaux avant de les envoyer sur le serveur qui nous renverra
                    //le message ainsi qu'aux autres clients connectés
                    //Si vous travaillez en mode texte, vous n'aurez pas à vous soucier
                    //de tout cela
                    string reformattedBuffer = msgArea.Text.Replace("}", "\\}");
                    reformattedBuffer = reformattedBuffer.Replace("\n", "\\par\r\n");
                    //Chaque message est précédé d'un numéro de séquence, ce qui permet
                    //de vérifier si le client vient de se connecter ou non
                    SendMsg(GetSequence() + NickName + reformattedBuffer.Replace("{", "\\{"));
                    msgArea.Clear();
                }

            }
            catch (Exception E)
            {
                MessageBox.Show("SendMessage:" + E.Message);
            }
        }
    }
}

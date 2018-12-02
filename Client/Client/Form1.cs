using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Utils;

namespace Client
{
    public partial class Form1 : Form
    {
        private string hostname;
        private int port;
        private TcpClient comm;
        private Thread ReceiveMessageThread = null;
        private string Nickname = null;

       
        //Je mets cette déclaration sur 3 lignes pour ne pas avoir de scroll horizontale :)
        //C'est en fait la déclaration de début d'un document RTF (merci wordpad) 
        private const string rtfStart = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fcharset0 Arial;}{\\f1\\fswiss\\fprq2\\fcharset0 Arial;}}{\\colortbl ;\\red0\\green0\\blue128;\\red0\\green128\\blue0;}\\viewkind4\\uc1";
        private string rtfContent = null;

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Program launch");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ReceiveMessages()
        {
            while (true)
            {
                if (comm.Connected)
                {
                    Utils.Message message = Utils.Utils.rcvMsg(comm.GetStream());
                    UpdateChatBody(message);
                }
                else //On est plus connecté donc on arrête tout
                {
                    MessageBox.Show("Disconnected");
                    Thread.CurrentThread.Abort();
                }
            }
        }


        private delegate void UpdateChatBodyDelegate(Utils.Message msg);

        private void UpdateChatBody(Utils.Message msg)
        {
            Console.WriteLine("Message recu : "+msg.message);

                //On remplit le richtextbox avec les données reues 
                //lorsqu'on a tout réceptionné
                if (chatBody.InvokeRequired)
                {
                    chatBody.Invoke(new UpdateChatBodyDelegate(UpdateChatBody), msg);
                }
                else
                {
                    chatBody.Text += msg.author + "say : " + msg.message+"\n";
                }



        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (Nick.Text == "")
            {
                MessageBox.Show("Le pseudo ne peut être null");
                return;
            }
            if (ServerHost.Text == "")
            {
                MessageBox.Show("Le nom du serveur ne peut être null");
                return;
            }

            comm = new TcpClient(ServerHost.Text, 7581);
            Nickname = Nick.Text;
            Utils.Message message = new Utils.Message();
            message.author = Nickname;
            message.channel = "Général";
            message.code = -1;
            Utils.Utils.sendMsg(comm.GetStream(),message);
            Utils.Message response = Utils.Utils.rcvMsg(comm.GetStream());
            if (response.code!=0) //Erreur, something wrong
            {
                MessageBox.Show(response.message);
            }

                //Si on était précedement connecté on stop le thread
                if (ReceiveMessageThread != null)
                {
                    ReceiveMessageThread.Abort();
                }

                UpdateChatBody(response);
                ReceiveMessageThread = new Thread(ReceiveMessages);
                ReceiveMessageThread.Start();
                
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Si le thread recevant les données a été démarré, on l'arrête
            if (ReceiveMessageThread != null)
            {
                try
                {
                    ReceiveMessageThread.Abort();
                    ReceiveMessageThread.Join();
                }
                catch (Exception E)
                {
                    MessageBox.Show("Arrêt Thread" + E.Message);
                }
            }
            if (comm != null && comm.Connected)
            {
                    comm.Close();
            }
        }

        //Clic sur le bouton envoyer
        private void button3_Click(object sender, EventArgs e)
        {
            //On vérifie que le client est bien connecté
            if (comm == null || !comm.Connected)
            {
                MessageBox.Show("Vous n'êtes pas connecté");
                return;
            }
            try
            {
                if (msgArea.Text != "")
                {
                    Utils.Message message = new Utils.Message();
                    message.author = Nickname;
                    message.message = msgArea.Text;
                    message.channel = "Général";
                    message.code = 0;
                    Utils.Utils.sendMsg(comm.GetStream(),message);
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

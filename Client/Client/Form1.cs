using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Utils;
using Message = Utils.Message;

namespace Client
{
    public partial class Form1 : Form
    {
        private string hostname;
        private int port;
        private TcpClient comm;
        private Thread ReceiveMessageThread = null;
        private string Nickname = null;
        private Dictionary<string, List<Message>> channels;
        private string currentChannel;

       
        //Je mets cette déclaration sur 3 lignes pour ne pas avoir de scroll horizontale :)
        //C'est en fait la déclaration de début d'un document RTF (merci wordpad) 
        private const string rtfStart = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fcharset0 Arial;}{\\f1\\fswiss\\fprq2\\fcharset0 Arial;}}{\\colortbl ;\\red0\\green0\\blue128;\\red0\\green128\\blue0;}\\viewkind4\\uc1";
        private string rtfContent = null;

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Program launch");
            currentChannel = "Général";
            channels = new Dictionary<string, List<Message>>();
            channels.Add("Général",new List<Message>());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ReceiveMessages(TcpClient comm)
        {
            while (true)
            {
                if (comm.Connected)
                {
                    Utils.Message message = Utils.Utils.rcvMsg(comm.GetStream());
                    UpdateChatBody(message);
                    UpdateListChannel(message.ChannelList);
                    channels[currentChannel].Add(message);
                }
                else //On est plus connecté donc on arrête tout
                {
                    MessageBox.Show("Disconnected");
                    Thread.CurrentThread.Abort();
                }
            }
        }

        private delegate void UpdateListChannelDelegate(List<string> chanelsList);
        private void UpdateListChannel(List<string> chanelsList)
        {
            if (chanelList.InvokeRequired)
            {
                chanelList.Invoke(new UpdateListChannelDelegate(UpdateListChannel), chanelsList);
            }
            else
            {
                chanelList.Items.Clear();
                foreach (string channel in chanelsList)
                {
                    chanelList.Items.Add(channel);
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

        private delegate void UpdateChatBodyDelegateList(List<Message> msg);

        private void UpdateChatBody(List<Message> msg)
        {
            if (chatBody.InvokeRequired)
            {
                chatBody.Invoke(new UpdateChatBodyDelegateList(UpdateChatBody), msg);
            }
            else
            {
                foreach (Message message in msg)
                {
                    chatBody.Text += message.author + "say : " + message.message + "\n";
                }
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

            comm = new TcpClient(ServerHost.Text, 8976);
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
                ReceiveMessageThread = new Thread(() => ReceiveMessages(comm));
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

        private void join_Click(object sender, EventArgs e)
        {
            currentChannel = chanelList.SelectedItem.ToString();
            UpdateChatBody(channels[currentChannel]);
        }

        private void create_Click(object sender, EventArgs e)
        {
            string value = "Document 1";
            if (InputBox("New document", "New document name:", ref value) == DialogResult.OK)
            {
                currentChannel = value;
                UpdateChatBody(new Message());
            }
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}

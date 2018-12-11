using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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


        private string rtfContent = null;

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Program launch");
            currentChannel = "Général";
            channels = new Dictionary<string, List<Message>>();
            channels.Add("Général", new List<Message>());
            List<string> list  = new List<string>();
            list.Add("Général");
            UpdateListChannel(list);
            ServerHost.Text = "127.0.0.1";
            Nick.Text = "Francois";
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
                    Message message = Utils.Utils.rcvMsg(comm.GetStream());
                    Console.WriteLine(message);
                    if (!channels.ContainsKey(message.channel))
                    {
                        channels.Add(message.channel,new List<Message>());
                    }
                    channels[message.channel].Add(message);
                    UpdateChatBody(message);
                    if (message.code==3)
                    {
                        Console.WriteLine("Chat body clear");
                        clearChatBody();
                    }
                    UpdateListChannel(new List<string>(channels.Keys));
                }
                else //On est plus connecté donc on arrête tout
                {
                    MessageBox.Show("Disconnected");
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

        private delegate void clearChatBodyDelegate();
        private void clearChatBody()
        {
            if (chatBody.InvokeRequired)
            {
                chatBody.Invoke(new clearChatBodyDelegate(clearChatBody));
            }
            else
            {
                chatBody.Clear();
            }
        }

        private delegate void UpdateChatBodyDelegate(Message msg);

        private void UpdateChatBody(Message msg)
        {
            Console.WriteLine("Message recu : " + msg);

            //On remplit le richtextbox avec les données reues 
            //lorsqu'on a tout réceptionné
            if (chatBody.InvokeRequired)
            {
                chatBody.Invoke(new UpdateChatBodyDelegate(UpdateChatBody), msg);
            }
            else
            {
                if (msg.channel==currentChannel)
                {
                    chatBody.Text += msg.author + " say : " + msg.message + "\n";
                }
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
                    chatBody.Text += message.author + " say : " + message.message + "\n";
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

            //if (!checkUserAccount(Nick.Text,Password.Text))
            //{
            //    Console.WriteLine("Failed to connect");
            //    return;
            //}

            comm = new TcpClient(ServerHost.Text, 8976);
            Nickname = Nick.Text;
            Utils.Message message = new Utils.Message();
            message.author = Nickname;
            message.channel = "Général";
            message.code = -1;
            string msg2 = "Hello";
            Utils.Utils.sendMsg(comm.GetStream(), message);
            Message response = Utils.Utils.rcvMsg(comm.GetStream());
            if (response.code != 0) //Erreur, something wrong
            {
                MessageBox.Show(response.message);
            }


            //Si on était précedement connecté on stop le thread
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
                    message.channel = currentChannel;
                    message.code = 0;
                    Utils.Utils.sendMsg(comm.GetStream(), message);
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
            if (chanelList.SelectedItem == null)
            {
                MessageBox.Show("Vous devez selectionner un topics");
                return;
            }
            currentChannel = chanelList.SelectedItem.ToString();
            if (!channels.ContainsKey(chanelList.SelectedItem.ToString())) //Si le topic n'existe pas on l'ajoute dans notre dictionnaire
            {
                channels.Add(currentChannel, new List<Message>());
            }
            chatBody.Clear();
            UpdateChatBody(channels[currentChannel]);

            //Message pour être inscrit dans la liste des utilisateurs dans le channel
            Message message = new Message();
            message.author = Nickname;
            message.channel = currentChannel;
            message.code = 2;
            Utils.Utils.sendMsg(comm.GetStream(), message);
        }

        private void create_Click(object sender, EventArgs e)
        {
            string value = "new channel";
            if (InputBox("Créé un channel", "New channel name:", ref value, true) == DialogResult.OK)
            {
                currentChannel = value;
                Message message = new Message();
                message.author = Nickname;
                message.channel = currentChannel;
                message.code = 2;
                Utils.Utils.sendMsg(comm.GetStream(),message);
            }
        }

        public static DialogResult InputBox(string title, string promptText, ref string value, bool input)
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

            if (!input)
            {
                label.Visible = false;
                textBox.Visible = false;
            }

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private bool checkUserAccount(string username, string password)
        {
            SqlConnection myConnection = new SqlConnection("Data Source=151.80.145.4;" + 
                                                           "User Id=francois;" +
                                                           "password=francois%123%;" +
                                                           "database=francois; ");


            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT * FROM `projetcsharp` WHERE `username` LIKE '" + username + "'", myConnection);
                myReader = myCommand.ExecuteReader();
                if (!myReader.Read()) //Pas d'utilisateur enregistré
                {
                    //TODO
                    string value = "new channel";
                    if (InputBox("Créé un channel", "New channel name:", ref value, true) == DialogResult.OK)
                    {
                        // Insert inside db
                        SqlCommand myCommand2 = new SqlCommand("INSERT INTO `projetcsharp`(`username`, `password`) VALUES('francois2', '123')", myConnection);
                        myCommand2.ExecuteNonQuery();
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    if (myReader.GetString(1) == password) //Connexion ok
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return false;
        }
    }
}

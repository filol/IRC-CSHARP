using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    [Serializable]
    public class Message
    {
        public Message() { }

        public Message(string channel,int code)
        {
            this.channel = channel;
            this.code = code;
        }

        //indique l'id du channel dans lequel le message à été envoyé. Channel 0  = serveur, demande de connexion
        public string channel { get; set; }
        public string message { get; set; }
        public string author { get; set; }
        /*
         * -1 = Ask for connection
         * 0 = No error
         * 1 = Pseudo already exist
         * 2 = create new channel
         * 3 = need to clean history
         * 4 = list users
         * 5 = Private message
         */
        public int code { get; set; }

        //public List<string> ChannelList { get; set; }

        public override string ToString()
        {
            return "Author=" + author + ", Channel=" + channel + ", Code=" + code + ", Message=" + message;
        }
    }
}

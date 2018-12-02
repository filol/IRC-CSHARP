using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    [Serializable]
    public class Message
    {
        //indique l'id du channel dans lequel le message à été envoyé. Channel 0  = serveur, demande de connexion
        public string channel { get; set; }
        public string message { get; set; }
        public string author { get; set; }
        /*
         * -1 = Ask for connection
         * 0 = No error
         * 1 = Pseudo already exist
         */
        public int code { get; set; }

    }
}

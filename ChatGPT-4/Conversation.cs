using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace ChatGPT_4
{
    public static class Conversation
    {
        public static List<JSONList> JsoList = new List<JSONList>();
        public static List<Message> Messages = new List<Message>();

        public static void AddMessage(Message message)
        {
            Messages.Add(message);
            JsoList.Add(new JSONList(message.Role,message.Content));
        }
        public static void ClearMessages()
        {
            Messages.Clear();
            JsoList.Clear();
        }

        public static List<Message> GetList()
        {
            return Messages;
        }

        public static bool IsEmpty()
        {
            return Messages.Count == 0;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI;

namespace ChatGPT_4
{
    public class JSONList
    {
        public Role Role { get; set; }
        public string Content { get; set; }
        public JSONList(Role role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}

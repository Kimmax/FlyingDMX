using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuernberger.FlyingDMX
{
    public class Command
    {
        public enum CommandType
        {
            SetBrightness,
            SetColor,
            SetS2L,
            Exit
        }

        public CommandType Type { get; private set; }
        public string[] Args { get; private set; }

        public Command(CommandType type, string[] args)
        {
            this.Type = type;
            this.Args = args;
        }

        public static Command TryParse(string text)
        {
            return new Command((CommandType)Enum.Parse(typeof(CommandType), text.Split(':')[0]), text.Split(':')[1].Split(';'));
        }

        public override string ToString()
        {
            return this.Type.ToString() + ":" + String.Join(";", this.Args);
        }
    }
}

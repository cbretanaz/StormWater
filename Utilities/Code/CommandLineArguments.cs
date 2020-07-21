using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CoP.Enterprise;

namespace CoP
{
    /// <summary>
    /// Class for managing command line arguments (string[] args).  The following sytax is
    /// supported: each parameter is preceeded by a slash (/) and followed by a colon (:).
    /// 
    /// Example:
    ///     /Action:TestAction /Param1:Value
    ///     The TestAction is called with one parameter (Param1=Value)
    /// </summary>
    public class CommandLineArguments : Dictionary<string, string>
    {
        public string Action { get; private set; }

        public CommandLineArguments(string[] args)
        {
            if (args == null || args.Length == 0 || (args.Length == 1 && string.IsNullOrEmpty(args[0])))
                return;
            var parameters = args.Combine(" ").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                // get key and add colons back into value
                var items = parameter.Split(':').ToList();
                var key = items[0];
                items.RemoveAt(0);
                var value = items.Combine(":");
                if (string.Equals(key, "Action", StringComparison.CurrentCultureIgnoreCase))
                    Action = string.IsNullOrEmpty(value) ? null : value.Trim();
                else
                    this[key] = string.IsNullOrEmpty(value) ? null : value.Trim();
            }
        }
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Action: {0}", Action);
            var index = 0;
            foreach (var item in this)
            {
                if (index++ > 0)
                    builder.AppendFormat("\r\n");
                builder.AppendFormat("{0}: {1}", item.Key, item.Value);
            }
            return builder.ToString();
        } 
    }
}

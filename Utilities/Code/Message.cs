using System.Windows.Forms;

namespace CoP.Enterprise
{
    public static class Message
    {
        /// <summary>
        /// Name of Application, to be used 
        /// </summary>
        public static string ApplicationName { get; set; }
        /// <summary>
        /// Caption to be used in title bar of message box
        /// </summary>
        public static string Caption { get; set; }
        /// <summary>
        /// Display Informational message box.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        public static void Info(string message,
            string caption = null, params object[] args)
        {
            var msg = args != null && args.Length > 0? 
                string.Format(message, args): message;
            MessageBox.Show(msg, caption ?? Caption,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #region notifications
        /// <summary>
        /// Display warning message box.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        public static void Warn(string message, 
            string caption = null, params object[] args)
        {
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            MessageBox.Show(msg, caption?? Caption,
                MessageBoxButtons.OK, MessageBoxIcon.Warning); }

        /// <summary>
        /// Display Exclamation point message box.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        public static void Bang(string message, 
            string caption = null, params object[] args)
        { 
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            MessageBox.Show(msg, caption ?? Caption,
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

        /// <summary>
        /// Display error message box.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        public static void Error(string message, 
            string caption = null, params object[] args)
        { 
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            MessageBox.Show(msg, caption ?? Caption,
                MessageBoxButtons.OK, MessageBoxIcon.Error); }

        /// <summary>
        /// Display Yes/No confirmation dialog message box.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        /// <returns>true(Yes) or false(Cancel)</returns>
        public static bool YN(string message, 
            string caption = null, params object[] args)
        { 
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            return MessageBox.Show(msg, caption ?? Caption,
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                   MessageBoxDefaultButton.Button1)
                   == DialogResult.Yes; }
        #endregion notifications

        #region queries, confirmations, etc.
        /// <summary>
        /// Display Ok/Cancel confirmation dialog message box to gather user response.
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        /// <returns>true(OK) or false(Cancel)</returns>
        public static bool OkCnx(string message, 
            string caption = null, params object[] args)
        { 
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            return MessageBox.Show(msg, caption ?? Caption,
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                   MessageBoxDefaultButton.Button1)
                   == DialogResult.OK; }

        /// <summary>
        /// Display Yes/No/Cancel dialog result to gather user response
        /// </summary>
        /// <param name="message">Message to be displayed in message box</param>
        /// <param name="caption">Caption to be used in title bar of message box</param>
        /// <param name="args">list of arguments to be used in formatting message</param>
        /// <returns>DialogResult Yes, No, or Cancel</returns>
        public static DialogResult YNCnx(string message,
            string caption = null, params object[] args)
        { 
            var msg = args != null && args.Length > 0 ? 
                string.Format(message, args) : message;
            return MessageBox.Show(msg, caption ?? Caption,
                   MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                   MessageBoxDefaultButton.Button1); }
        #endregion queries, confirmations, etc.
    }
}
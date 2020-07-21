using System;
using System.DirectoryServices.AccountManagement;


namespace CoP.Enterprise
{
    [Serializable]
    public class UserInfo
    {
        #region properties
        public string CoPDisplayName
        { get { return string.Format("{0} {1}", FirstName, LastName); } }
        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }
        public string EmailAddress { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Username { get; private set; }
        public Guid? Guid { get; private set; }
        public string DisplayName { get; private set; }
        public string UPN { get; private set; }
        #endregion properties

        private UserInfo() { }

        public static UserInfo From(UserPrincipal up)
        {
            return new UserInfo
            {
                FirstName = up.GivenName,
                EmailAddress = up.EmailAddress,
                Guid = up.Guid,
                LastName = up.Surname,
                MiddleName = up.MiddleName,
                PhoneNumber = up.VoiceTelephoneNumber,
                Username = up.Name,
                UPN = up.UserPrincipalName
            };
        }
    }
}
using IsmStateServer.Types;

namespace MonsoonAPI.models
{
    public class PersonNameDataM
    {
        // ReSharper disable once InconsistentNaming
        public string m_strFirstName;
        // ReSharper disable once InconsistentNaming
        public string m_strLastName;
        // ReSharper disable once InconsistentNaming
        public string m_strLogin;

        public static PersonNameDataM FromPersonNameData(PersonNameData person)
        {
            return new PersonNameDataM
            {
                m_strLastName = person.m_strLastName,
                m_strFirstName = person.m_strFirstName,
                m_strLogin = person.m_strLogin ?? string.Empty
            };
        }

        public PersonNameData ToPersonNameData()
        {
            return new PersonNameData()
            {
                m_strLogin = m_strLogin,
                m_strFirstName = m_strFirstName,
                m_strLastName = m_strLastName
            };
        }
    }
}

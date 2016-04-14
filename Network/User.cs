using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Network
{
    public class User
    {
        public User()
        {
            Image = new byte[0];
        }

        public IPublicKey PublicKey
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public byte[] Image
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string ToXml()
        {
            var xml = new XElement("User",
                new XElement("Name", Name),
                new XElement("Image", Convert.ToBase64String(Image ?? new byte[0])),
                new XElement("Key", this.PublicKey.ToXml()));
            return xml.ToString();
        }

        public static User FromXml(string xmlString)
        {
            var xml = XElement.Parse(xmlString);
            var name = xml.Nodes().OfType<XElement>().First(x => x.Name == "Name").Value;
            var image = xml.Nodes().OfType<XElement>().First(x => x.Name == "Image").Value;
            var certificate = xml.Nodes().OfType<XElement>().First(x => x.Name == "Key").Value;

            var u = new User()
            {
                Name = name,
                Image = Convert.FromBase64String(image),
                PublicKey = Security.SecurityFactory.CreatePublicKey().LoadXml(certificate)
            };
            return u;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                result = result * prime + PublicKey.GetHashCode();
                result = result * prime + Name.GetHashCode();
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is User))
                return false;
            var other = (User)obj;
            if (!PublicKey.Equals(other.PublicKey))
                return false;
            if (!Name.Equals(other.Name))
                return false;
            return true;
        }
    }
}
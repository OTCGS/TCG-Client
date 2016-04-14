using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Security.Interfaces;

namespace Client.Game.Data
{
    /// <summary>
    /// Das Übertragungsformat des Public Keys
    /// </summary>
    public partial class PublicKey : Security.Interfaces.IPublicKeyData
    {


        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var p = obj as PublicKey;

            if (p == null)
                return false;

            return p.Exponent.SequenceEqual(this.Exponent) && p.Modulus.SequenceEqual(this.Modulus);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                foreach (byte b in Exponent.Concat(Modulus))
                    result = (result * 31) ^ b;
                return result;
            }
        }



        public IEnumerable<byte> Bytes()
        {
            return Modulus.Concat(Exponent);

        }

        void IPublicKeyData.SetKey(byte[] modulus, byte[] exponent)
        {
            this.Modulus = modulus;
            this.Exponent = exponent;
        }
    }


    /// <summary>
    /// Eine Eindeutige Karte. Wird genutzt um den besitz einer Karte Darzustellen.
    /// Jede ID existeiret nur einmal auf jedem Server.
    /// Wenn Die ID und der Server identisch ist, ist es die selbe Karte.
    /// </summary>
    public partial class CardInstance
    {

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var c = obj as CardInstance;
            if (c == null)
                return false;

            return c.Id == this.Id && c.CardDataId.Equals(this.CardDataId) && c.Creator.Equals(Creator);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 31) ^ this.Id.GetHashCode();
            result = (result * 31) ^ this.CardDataId.GetHashCode();
            result = (result * 31) ^ this.Creator.GetHashCode();
            return result;
        }

        public IEnumerable<byte> Bytes()
        {
            return Id.ToBigEndianBytes().Concat(CardDataId.ToBigEndianBytes()).Concat(Creator.Bytes());
        }

    }


    public partial class Ruleset
    {
        public byte[] Bytes()
        {
            return Id.ToBigEndianBytes().Concat(
                Creator.Bytes().Concat(
                Encoding.UTF8.GetBytes(Name)).Concat(
                Misc.BitConverter.GetBytes(Revision)).Concat(
                Encoding.UTF8.GetBytes(Script)).Concat(
                (MandatoryKeys?.OrderBy(x => x.Name)?.Select(
                    x => x.Bytes()
                )?.SelectMany(x => x))) ?? Enumerable.Empty<byte>()).ToArray();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var c = obj as Ruleset;
            if (c == null)
                return false;

            return c.Id == this.Id && c.Creator.Equals(this.Creator) && c.Revision == this.Revision;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 31) ^ this.Id.GetHashCode();
            result = (result * 31) ^ this.Creator.GetHashCode();
            result = (result * 31) ^ this.Revision.GetHashCode();
            return result;
        }




    }

    public partial class Keys
    {
        public byte[] Bytes()
        {
            return Encoding.UTF8.GetBytes(this.Name + this.Type);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var c = obj as Keys;
            if (c == null)
                return false;

            return c.Name == this.Name && c.Type == this.Type;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 31) ^ this.Name.GetHashCode();
            result = (result * 31) ^ this.Type.GetHashCode();
            return result;
        }

    }


    /// <summary>
    /// Die Daten der einzelenen Karten. Wie Name, Bild und Eigenschaften.
    /// </summary>
    /// <remarks>
    /// Dieses Objekt repräsentiert NICHT eine Instanz einer Karte die jemand besitzt. 
    /// Sondern die Daten des Types einer Karte.
    /// Es können viele gleiche Karten existieren, die verschiedenen Spielern gehören.
    /// </remarks>
    public partial class CardData
    {

        public UuidServer UuidServer { get { return new UuidServer() { Uuid = this.Id, Server = this.Creator }; } }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var other = obj as CardData;
            if (other == null)
                return false;


            return this.Id.Equals(other.Id) && this.Creator.Equals(Creator);

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = result * prime + this.Id.GetHashCode();
            result = result * prime + this.Creator.GetHashCode();
            return result;
        }

        public IEnumerable<byte> Bytes()
        {
            return Id.ToBigEndianBytes()
                .Concat(Creator.Bytes())
                .Concat(Encoding.UTF8.GetBytes(Edition))
                .Concat(Misc.BitConverter.GetBytes(CardRevision))
                .Concat(ImageId.ToBigEndianBytes())
                .Concat(Encoding.UTF8.GetBytes(Name))
                .Concat(
                    Values.OrderBy(x => x.Key).Select(x => Encoding.UTF8.GetBytes(x.Key)
                          .Concat(Encoding.UTF8.GetBytes(x.Value))).SelectMany(x => x)
                );
        }

    }
    public partial class ServerId
    {


        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var other = obj as ServerId;
            if (other == null)
                return false;


            return this.Key.Equals(other.Key) && this.Revision == other.Revision;

        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = result * prime + this.Key.GetHashCode();
            result = result * prime + this.Revision.GetHashCode();
            return result;
        }

        public IEnumerable<byte> Bytes()
        {
            return Encoding.UTF8.GetBytes(Name)
                .Concat(Key.Bytes())
                .Concat(Icon == Guid.Empty ? new byte[] { } : Icon.ToBigEndianBytes())
                .Concat(Encoding.UTF8.GetBytes(Uri))
                .Concat(Misc.BitConverter.GetBytes(Revision));
        }
    }

    /// <summary>
    /// Verweist auf ein Bild.
    /// </summary>
    public partial class ImageData
    {

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var other = obj as ImageData;

            if (other == null)
                return false;

            return other.Id == this.Id;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public IEnumerable<byte> Bytes()
        {
            return Id.ToBigEndianBytes().Concat(Image).Concat(Creator.Bytes());
        }


    }



    /// <summary>
    /// Die Daten der einzelenen Karten. Wie Name, Bild und Eigenschaften.
    /// </summary>
    /// <remarks>
    /// Dieses Objekt repräsentiert NICHT eine Instanz einer Karte die jemand besitzt. 
    /// Sondern die Daten des Types einer Karte.
    /// Es können viele gleiche Karten existieren, die verschiedenen Spielern gehören.
    /// </remarks>
    public partial class UuidServer
    {

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            var other = obj as UuidServer;
            if (other == null)
                return false;


            return this.Uuid.Equals(other.Uuid) && this.Server.Equals(Server);

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = result * prime + this.Uuid.GetHashCode();
            result = result * prime + this.Server.GetHashCode();
            return result;
        }
    }




}

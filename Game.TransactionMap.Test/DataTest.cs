using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Game.TransactionMap.Test
{
    /// <summary>
    /// Summary description for DataTest
    /// </summary>
    [TestClass]
    public class DataTest
    {
        public DataTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestBinaryReperesntationUUID()
        {
            var g = new Guid("A1100CBF-A073-456E-8735-7585BB6B4691");
            var bytes = Convert.ToBase64String(g.ToBigEndianBytes());
            var expected = "oRAMv6BzRW6HNXWFu2tGkQ==";
            Assert.AreEqual(expected, bytes);
        }

        [TestMethod]
        public void CHeckPublickKeyVeryfi()
        {


            //Modulus: 6odr0EmivbYLpKk38kSdFC0KJs3JkwnK1XXSd4pDLerp+S6lTH/XDq3Tb9XC2oyKA0I/LBbCIRUxOiWhf5W8BPfqwf4ljziVR6e3ak7fkWCQaROVbJXPNyDzDPd7NZYiL8fXXP4ejjWr/gNb0v0bRAfGctjvRa0lrEwfQ0bz1z8=
            //Exponent: AQAB
            //1dZTLTiB+KaGtgHhx85LgccdBXdxUmoL8DSUF38dcZ3ZDNqnTGO74y14eNpR3Lvh4MiNMce2LhFIVc4oodEOkAtxM/xpOgiuc8erOMj1lxZriDUrk6gKrb20wczHwUL5gkXt3H95V75yZmqMvgo4F6ZPmoWDeLqsH+Jp2o9Ixjg=

            //< RSAKeyValue >
            //<RSAKeyValue>
            //<Modulus>6odr0EmivbYLpKk38kSdFC0KJs3JkwnK1XXSd4pDLerp+S6lTH/XDq3Tb9XC2oyKA0I/LBbCIRUxOiWhf5W8BPfqwf4ljziVR6e3ak7fkWCQaROVbJXPNyDzDPd7NZYiL8fXXP4ejjWr/gNb0v0bRAfGctjvRa0lrEwfQ0bz1z8=
            //</Modulus>
            //<Exponent>AQAB</Exponent></RSAKeyValue>



            var data = Convert.FromBase64String("oRAMv6BzRW6HNXWFu2tGkQ==");
            var key = Security.SecurityFactory.CreatePublicKey();
            var exponent = Convert.FromBase64String("AQAB");

            // Variante mit Führender 0
            var modulus = Convert.FromBase64String("AOqHa9BJor22C6SpN/JEnRQtCibNyZMJytV10neKQy3q6fkupUx/1w6t02/VwtqMigNCPywWwiEVMToloX+VvAT36sH+JY84lUent2pO35FgkGkTlWyVzzcg8wz3ezWWIi/H11z+Ho41q/4DW9L9G0QHxnLY70WtJaxMH0NG89c/");
            key.SetKey(modulus, exponent);

            var signature = Convert.FromBase64String("1dZTLTiB+KaGtgHhx85LgccdBXdxUmoL8DSUF38dcZ3ZDNqnTGO74y14eNpR3Lvh4MiNMce2LhFIVc4oodEOkAtxM/xpOgiuc8erOMj1lxZriDUrk6gKrb20wczHwUL5gkXt3H95V75yZmqMvgo4F6ZPmoWDeLqsH+Jp2o9Ixjg=");
            Console.WriteLine(key.ToString());
            var erg = key.Veryfiy(data, signature);
            Assert.IsTrue(erg);

        }
        [TestMethod]
        public void CheckDataStrucktureVonTransfare()
        {

            var expected = "AOqHa9BJor22C6SpN/JEnRQtCibNyZMJytV10neKQy3q6fkupUx/1w6t02/VwtqMigNCPywWwiEVMToloX+VvAT36sH+JY84lUent2pO35FgkGkTlWyVzzcg8wz3ezWWIi/H11z+Ho41q/4DW9L9G0QHxnLY70WtJaxMH0NG89c/AQABAOqHa9BJor22C6SpN/JEnRQtCibNyZMJytV10neKQy3q6fkupUx/1w6t02/VwtqMigNCPywWwiEVMToloX+VvAT36sH+JY84lUent2pO35FgkGkTlWyVzzcg8wz3ezWWIi/H11z+Ho41q/4DW9L9G0QHxnLY70WtJaxMH0NG89c/AQABoRAMv6BzRW6HNXWFu2tGkQDqh2vQSaK9tgukqTfyRJ0ULQomzcmTCcrVddJ3ikMt6un5LqVMf9cOrdNv1cLajIoDQj8sFsIhFTE6JaF/lbwE9+rB/iWPOJVHp7dqTt+RYJBpE5Vslc83IPMM93s1liIvx9dc/h6ONav+A1vS/RtEB8Zy2O9FrSWsTB9DRvPXPwEAAQAAAAAA6odr0EmivbYLpKk38kSdFC0KJs3JkwnK1XXSd4pDLerp+S6lTH/XDq3Tb9XC2oyKA0I/LBbCIRUxOiWhf5W8BPfqwf4ljziVR6e3ak7fkWCQaROVbJXPNyDzDPd7NZYiL8fXXP4ejjWr/gNb0v0bRAfGctjvRa0lrEwfQ0bz1z8BAAEA6odr0EmivbYLpKk38kSdFC0KJs3JkwnK1XXSd4pDLerp+S6lTH/XDq3Tb9XC2oyKA0I/LBbCIRUxOiWhf5W8BPfqwf4ljziVR6e3ak7fkWCQaROVbJXPNyDzDPd7NZYiL8fXXP4ejjWr/gNb0v0bRAfGctjvRa0lrEwfQ0bz1z8BAAE=";

            var keyA = new Client.Game.Data.PublicKey()
            {
                Modulus = Convert.FromBase64String("AOqHa9BJor22C6SpN/JEnRQtCibNyZMJytV10neKQy3q6fkupUx/1w6t02/VwtqMigNCPywWwiEVMToloX+VvAT36sH+JY84lUent2pO35FgkGkTlWyVzzcg8wz3ezWWIi/H11z+Ho41q/4DW9L9G0QHxnLY70WtJaxMH0NG89c/"),
                Exponent = Convert.FromBase64String("AQAB")
            };
            var keyB = new Client.Game.Data.PublicKey()
            {
                Modulus = Convert.FromBase64String("AOqHa9BJor22C6SpN/JEnRQtCibNyZMJytV10neKQy3q6fkupUx/1w6t02/VwtqMigNCPywWwiEVMToloX+VvAT36sH+JY84lUent2pO35FgkGkTlWyVzzcg8wz3ezWWIi/H11z+Ho41q/4DW9L9G0QHxnLY70WtJaxMH0NG89c/"),
                Exponent = Convert.FromBase64String("AQAB")
            };


            var transfare = new TransactionMap.ServiceMerger.TransferImplimentation()
            {
                CardId = new Guid("A1100CBF-A073-456E-8735-7585BB6B4691"),
                CardTransferIndex = 0,
                Creator = keyA,
                Giver = keyA,
                Recipient = keyB,
                PreviousTransactionHash = null

            };

            var transaction = new TransactionMap.ServiceMerger.TransactionImplimentation()
            {
                A = keyA,
                B = keyB,
                Transfers = new TransactionMap.ServiceMerger.ITransfer[] { transfare },
            };

            var bytes = GenereateBytesToSign(transaction);
            var actual = Convert.ToBase64String(bytes);
            Assert.AreEqual(expected, actual);


        }

        internal static byte[] GenereateBytesToSign(TransactionMap.ServiceMerger.TransactionImplimentation t)
        {


            var buffer = new List<byte>();

            buffer.AddRange(t.A.Modulus);
            buffer.AddRange(t.A.Exponent);

            buffer.AddRange(t.B.Modulus);
            buffer.AddRange(t.B.Exponent);

            foreach (var transfare in t.Transfers)
            {
                // Generate Hash
                buffer.AddRange(transfare.CardId.ToBigEndianBytes());

                var cardCreator = transfare.Creator;
                buffer.AddRange(cardCreator.Modulus);
                buffer.AddRange(cardCreator.Exponent);


                buffer.AddRange(Misc.BitConverter.GetBytes(transfare.CardTransferIndex));

                buffer.AddRange(transfare.Giver.Modulus);
                buffer.AddRange(transfare.Giver.Exponent);

                buffer.AddRange(transfare.Recipient.Modulus);
                buffer.AddRange(transfare.Recipient.Exponent);

                buffer.AddRange(transfare.PreviousTransactionHash ?? new byte[0]);

            }

            return buffer.ToArray();
        }

    }
}

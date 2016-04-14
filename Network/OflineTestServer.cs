using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class OfflineTestServer
    {
        public static async Task<Tuple<MultiConnection, MultiConnection>> GetConnection(User user, ConnectionReason reason, Guid dataId, Security.IPublicKey dataKey, string ip)
        {
            var testUser = new User();
            testUser.Name = "OfflineTestUser";
            testUser.PublicKey = Security.SecurityFactory.CreatePublicKey();
            var connection = new MultiConnection(user, reason,dataId,dataKey);
            var testConnection = new MultiConnection(testUser, reason,dataId,dataKey);

            var socket = Socket.SocketFactory.CreateReliableDatagramsocket();
            await socket.Bind();
            var direcConnection = new RawConnections.DirectConnection(socket);

            var otherSocket = Socket.SocketFactory.CreateReliableDatagramsocket();
            await otherSocket.Bind();
            var otherDirecConnection = new RawConnections.DirectConnection(otherSocket);

            direcConnection.SetOtherPort(ip, otherSocket.Port);
            otherDirecConnection.SetOtherPort(ip, socket.Port);

            await connection.Init(direcConnection);
            await testConnection.Init(otherDirecConnection);

            return Tuple.Create(connection, testConnection);
        }
    }
}
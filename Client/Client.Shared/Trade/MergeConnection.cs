using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;
using System.Linq;

namespace Client.Trade
{
    class MergeConnectivity : Network.AbstractVerifiableConnectivity<Client.Game.Data.Protokoll>
    {

        public MergeConnectivity(Network.IUserConnection connection) : base(connection, Viewmodel.UserDataViewmodel.Instance.LoggedInUser, new byte[] { 98, 49, 43 })
        {

        }


        public async Task Merge()
        {
            using (var question = new MergeConnectivity.Question())
            {
                var mergeTask = global::Game.TransactionMap.Graph.Merge(question);

                var incomming = this.Recive();
                var outgoing = question.DeQueue();

                UInt32 index = 0;
                Dictionary<UInt32, TaskCompletionSource<byte[]>> dataLookup = new Dictionary<uint, TaskCompletionSource<byte[]>>();

                bool imReady = false;
                bool otherReady = false;

                while (!(imReady && otherReady))
                {
                    Logger.Assert(incomming != null || mergeTask != null || outgoing != null, "Wenn wir die Schleife betreten, muss midestens eins von den 3en nicht null sein.");

                    var finishedTask = await Task.WhenAny(new[] { incomming, outgoing, mergeTask }.Where(x => x != null));
                    if (finishedTask == incomming)
                    {
                        var data = await incomming;
                        if (data is MergeDataCall)
                        {
                            var mData = data as MergeDataCall;
                            Logger.TransactionInfo(String.Format("MergeConnection Recived Call Index:{0}", mData.Index));
                            var answer = await question.ReciveBytes(mData.Data);
                            Logger.TransactionInfo(String.Format("MergeConnection Send Return Index:{0}", mData.Index));
                            await this.SendMessage(new MergeDataReturn() { Data = answer, Index = mData.Index });

                        }
                        else if (data is MergeDataReturn)
                        {
                            var mData = data as MergeDataReturn;
                            Logger.TransactionInfo(String.Format("MergeConnection Recived Return Index:{0}", mData.Index));
                            Logger.Assert(dataLookup.ContainsKey(mData.Index), "Index ist nicht enthalten?? MergeConnection.Merge()");
                            dataLookup[mData.Index].SetResult(mData.Data);
                            dataLookup.Remove(mData.Index);
                        }
                        else if (data is MergeDataOtherFinished)
                            otherReady = true;
                        else
                            throw new NotSupportedException();

                        incomming = this.Recive();

                    }
                    else if (finishedTask == outgoing)
                    {
                        var data = await outgoing;
                        index++;
                        var toSend = new MergeDataCall() { Data = data.Item1, Index = index };
                        dataLookup.Add(toSend.Index, data.Item2);

                        Logger.TransactionInfo(String.Format("MergeConnection Send Call Index:{0}", index));
                        await this.SendMessage(toSend);

                        outgoing = question.DeQueue();
                    }
                    else if (finishedTask == mergeTask)
                    {
                        imReady = true;
                        outgoing = null;
                        mergeTask = null;
                        await SendMessage(new MergeDataOtherFinished());
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                }


            }
        }


        protected override Task<Protokoll> ConvertFromByte(byte[] data)
        {
            var xml = UTF8Encoding.UTF8.GetString(data, 0, data.Length);
            return Task.FromResult((Protokoll)Game.Data.Protokoll.Convert(xml));
        }

        protected override Task<byte[]> ConvertToByte(Protokoll data)
        {
            var str = Game.Data.Protokoll.Convert(data);
            return Task.FromResult(Encoding.UTF8.GetBytes(str));
        }

        private class Question : global::Game.TransactionMap.AbstractQuestchener, IDisposable
        {

            private readonly System.Collections.Concurrent.ConcurrentChannel<Tuple<byte[], TaskCompletionSource<byte[]>>> messeageQue = new System.Collections.Concurrent.ConcurrentChannel<Tuple<byte[], TaskCompletionSource<byte[]>>>();

            public Question()
            {

            }

            protected async override Task<byte[]> SendBytes(byte[] bytes)
            {
                var t = new TaskCompletionSource<byte[]>();
                await messeageQue.Send(Tuple.Create(bytes, t));
                return await t.Task;
            }

            internal Task<Tuple<byte[], TaskCompletionSource<byte[]>>> DeQueue()
            {
                return messeageQue.Recive();
            }

            public new Task<byte[]> ReciveBytes(byte[] bytes)
            {
                return base.ReciveBytes(bytes);
            }


            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        messeageQue.Dispose();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
            // ~Question() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}

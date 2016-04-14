using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Data
{
    static class CardDataStorage
    {
        private static readonly Dictionary<string, Uri> imageDic = new Dictionary<string, Uri>();
        private static readonly Dictionary<string, ImageData> imageDataDic = new Dictionary<string, ImageData>();
        private static readonly Dictionary<CardDataId, CardData> cardDic = new Dictionary<CardDataId, CardData>();
        public static async Task<CardData> GetCardData(CardDataId id)
        {

            if (cardDic.ContainsKey(id))
                return cardDic[id];
            // TODO: Finde die Daten durch nachfragen bei anderen.
            throw new NotImplementedException();

        }

        public static async Task<Uri> GetCardDataImageUri(CardDataId id)
        {
            var data = await GetCardData(id);
            var imageid = data.ImageId;
            if (imageDic.ContainsKey(imageid))
                return imageDic[imageid];

            if (imageDataDic.ContainsKey(imageid))
            {
                imageDic[imageid] = await GenerateUri(imageDataDic[imageid]);
                return imageDic[imageid];
            }

            // TODO: Finde das Imege durch nachfragen bei anderen.
            throw new NotImplementedException();
        }

        public static void InitStandardGameCards(GameData data)
        {
            foreach (var c in data.StandardCards)
                cardDic[c.Id] = c;

            foreach (var i in data.StandardCardsImages)
                imageDataDic[i.Id] =i;
        }

        private static async Task<Uri> GenerateUri(ImageData i)
        {
            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync("bla", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await            Windows.Storage.FileIO.WriteBytesAsync(file, i.Image);
            
            return new Uri("ms-appdata:///temp/" + file.Name, UriKind.Absolute);
        }
    }
}

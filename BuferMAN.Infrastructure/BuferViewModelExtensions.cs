using BuferMAN.Models;
using BuferMAN.View;
using System.Linq;

namespace BuferMAN.Infrastructure
{
    public static class BuferViewModelExtensions
    {
        public static BuferItem ToModel(this BuferViewModel buferViewModel)
        {
            var buferItem = new BuferItem()
            {
                Pinned = buferViewModel.Pinned,
                Alias = buferViewModel.Alias,
                Formats = buferViewModel.Clip
                                        .GetFormats()
                                        .ToDictionary(
                                                 f => f,
                                                 f => buferViewModel.Clip.GetData(f))
            };
            return buferItem;
        }
    }
}

using System.Linq;
using BuferMAN.Models;
using BuferMAN.View;

namespace BuferMAN.Infrastructure
{
    public class Mapper : IMapper // TODO (s) into mapper
    {
        public BuferItem Map(BuferViewModel buferViewModel)
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

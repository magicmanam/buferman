using BuferMAN.Infrastructure.Settings;
using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Settings
{
    internal class ProgramSettings : IProgramSettings
    {
        public IEnumerable<BufersStorageModel> StoragesToLoadOnStart =>
            new List<BufersStorageModel> {
                new BufersStorageModel
                {
                    StorageType = BufersStorageType.JsonFile,
                    StorageAddress = this.DefaultBufersFileName
                }
            };

        public string DefaultBufersFileName => "bufers.json";

        public int MaxBufersCount => 30;

        public int ExtraBufersCount => 25;

        public int MaxBuferPresentationLength => 2300;//Limits: low 2000, high 5000

        public int BuferTooltipDuration => 2500;

        public Color BuferDefaultBackColor
        {
            get
            {
                return Color.Silver;
            }
        }

        public Color CurrentBuferBackColor
        {
            get
            {
                return this.BuferDefaultBackColor;// Color.LightGreen;
            }
        }

        public Color FocusedBuferBackColor
        {
            get
            {
                return Color.LightSteelBlue;
            }
        }

        public Color PinnedBuferBackColor
        {
            get
            {
                return Color.LightSlateGray;
            }
        }

        public Color PinnedCurrentBuferBackColor
        {
            get
            {
                return this.PinnedBuferBackColor;// Color.LawnGreen;
            }
        }

        public bool ShowUserModeNotification { get; } = true;
    }
}

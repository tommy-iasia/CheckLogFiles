using System;
using System.IO;

namespace CheckLogWorker
{
    public static class HarddiskSpaceRecordExtensions
    {
        public static HarddiskSpaceRecord ToRecord(this DriveInfo drive, DateTime time)
        {
            return new HarddiskSpaceRecord
            {
                Name = drive.Name,

                AvailableFreeSpace = drive.AvailableFreeSpace,
                TotalFreeSpace = drive.TotalFreeSpace,
                TotalSize = drive.TotalSize,

                Time = time
            };
        }
    }
}

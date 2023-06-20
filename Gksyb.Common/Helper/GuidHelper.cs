using System.Threading;

namespace Gksyb.Common
{
    /// <summary>
    /// 唯一ID生成帮助类
    /// </summary>
    public class GuidHelper
    {
        /// <summary>
        /// 生成类似Mongodb的ObjectId有序、不重复Guid
        /// </summary>
        /// <returns></returns>
        public static string NewMongodbId()
        {
            var now = DateTime.Now;
            var uninxtime = (int)now.Subtract(DateTime.UnixEpoch).TotalSeconds;
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
            var rand = rnd.Value.Next(0, int.MaxValue);
            var guid = string.Format("{0}{1}{2}{3}{4}",
                uninxtime.ToString("x8").PadLeft(8, '0')
                , __staticMachine.ToString("x8").PadLeft(8, '0').Substring(2, 6)
                , __staticPid.ToString("x8").PadLeft(8, '0').Substring(6, 2)
                , increment.ToString("x8").PadLeft(8, '0')
                , rand.ToString("x8").PadLeft(8, '0'));
            return Guid.Parse(guid).ToString("N");
        }

        /// <summary>
        /// 用雪花算法生成ID
        /// </summary>
        /// <returns></returns>
        public static long NewSnowflakeId() => IdWorker.NextId();

        /// <summary>
        /// 11位短ID生成
        /// </summary>
        /// <returns></returns>
        public static string NewShortId() => IdWorker.NextId().ToBase62();

        private static readonly ThreadLocal<Random> rnd = new(() => new Random());
        private static readonly int __staticMachine = ((0x00ffffff & Environment.MachineName.GetHashCode()) + AppDomain.CurrentDomain.Id) & 0x00ffffff;
        private static readonly int __staticPid = Environment.ProcessId;
        private static int __staticIncrement = rnd.Value.Next();

        private class IdWorker
        {
            public const long Twepoch = 1288834974657L;

            private const int WorkerIdBits = 5;
            private const int DatacenterIdBits = 5;
            private const int SequenceBits = 12;
            private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);
            private const int MaxDatacenterId = -1 ^ (-1 << DatacenterIdBits);

            private const int WorkerIdShift = SequenceBits;
            private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
            public const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
            private const int SequenceMask = -1 ^ (-1 << SequenceBits);

            private static readonly long WorkerId = rnd.Value.Next(0, MaxWorkerId);
            public static readonly long DatacenterId = rnd.Value.Next(0, MaxDatacenterId);

            private static long _sequence = 0L;
            private static long _lastTimestamp = -1L;
            private static readonly object _lock = new();

            public static long NextId()
            {
                lock (_lock)
                {
                    var timestamp = TimeGen();
                    if (_lastTimestamp == timestamp)
                    {
                        _sequence = (_sequence + 1) & SequenceMask;
                        if (_sequence == 0)
                        {
                            Thread.Sleep(1);
                            timestamp = TimeGen();
                        }
                    }
                    else
                    {
                        _sequence = 0;
                    }
                    _lastTimestamp = timestamp;
                    var id = ((timestamp - Twepoch) << TimestampLeftShift) |
                             (DatacenterId << DatacenterIdShift) |
                             (WorkerId << WorkerIdShift) | _sequence;
                    return id;
                }
            }

            protected static long TimeGen()
            {
                var timestamp = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
                if (timestamp < _lastTimestamp)
                {
                    throw new MessageException($"时钟被调整，两次时间差为负数{_lastTimestamp - timestamp}");
                }
                return timestamp;
            }
        }
    }
}
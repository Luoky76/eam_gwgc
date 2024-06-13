namespace Gksyb.Common
{
    /// <summary>
    /// 坐标互转
    /// </summary>
    public static class GpsCoordinateHelper
    {
        private const double _pi = 3.14159265358979324;
        private const double _a = 6378245.0;
        private const double _ee = 0.00669342162296594323;
        private const double _xpi = 3.14159265358979324 * 3000.0 / 180.0;

        /// <summary>
        /// wgs坐标转百度坐标
        /// </summary>
        /// <returns></returns>
        public static double[] GPSToBaidu(double lng, double lat)
        {
            double[] gcj = WGSToGCJ(lat, lng);
            double[] bd = GCJToBaidu(gcj[0], gcj[1]);
            return bd;
        }

        /// <summary>
        /// gcj02坐标转百度
        /// </summary>
        /// <returns></returns>
        private static double[] GCJToBaidu(double lat, double lon)
        {
            double x = lon, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * _xpi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * _xpi);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            return new double[] { bd_lon, bd_lat };
        }

        /// <summary>
        /// wgs坐标转gcj02
        /// </summary>
        /// <returns></returns>
        private static double[] WGSToGCJ(double lat, double lon)
        {
            double dLat = TransformLat(lon - 105.0, lat - 35.0);
            double dLon = TransformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * _pi;
            double magic = Math.Sin(radLat);
            magic = 1 - _ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((_a * (1 - _ee)) / (magic * sqrtMagic) * _pi);
            dLon = (dLon * 180.0) / (_a / sqrtMagic * Math.Cos(radLat) * _pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            double[] loc = { mgLat, mgLon };
            return loc;
        }

        /// <summary>
        /// 纬度转换
        /// </summary>
        /// <returns></returns>
        private static double TransformLat(double lat, double lon)
        {
            double ret = -100.0 + 2.0 * lat + 3.0 * lon + 0.2 * lon * lon + 0.1 * lat * lon + 0.2 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * _pi) + 20.0 * Math.Sin(2.0 * lat * _pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lon * _pi) + 40.0 * Math.Sin(lon / 3.0 * _pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lon / 12.0 * _pi) + 320 * Math.Sin(lon * _pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 经度转换
        /// </summary>
        /// <returns></returns>
        private static double TransformLon(double lat, double lon)
        {
            double ret = 300.0 + lat + 2.0 * lon + 0.1 * lat * lat + 0.1 * lat * lon + 0.1 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * _pi) + 20.0 * Math.Sin(2.0 * lat * _pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * _pi) + 40.0 * Math.Sin(lat / 3.0 * _pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lat / 12.0 * _pi) + 300.0 * Math.Sin(lat / 30.0 * _pi)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 百度转GPS
        /// </summary>
        /// <returns></returns>
        public static double[] BaiduToGPS(double lng, double lat)
        {
            var x_pi = 3.14159265358979324 * 3000.0 / 180.0;
            var x = lng - 0.0065;
            var y = lat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            var gg_lng = z * Math.Cos(theta);
            var gg_lat = z * Math.Sin(theta);
            double[] bdpoint = GoogleToGps(gg_lng, gg_lat);
            return bdpoint;
        }

        /// <summary>
        /// 谷歌转GPS
        /// </summary>
        /// <returns></returns>
        public static double[] GoogleToGps(double lng, double lat)
        {
            if (IsOutOfChina(lng, lat))
            {
                return new double[] { lng, lat };
            }
            double dlat = TransformLat2(lng - 105.0, lat - 35.0);
            double dlng = TransformLon2(lng - 105.0, lat - 35.0);
            double radlat = lat / 180.0 * _pi;
            double magic = Math.Sin(radlat);
            magic = 1 - _ee * magic * magic;
            double sqrtmagic = Math.Sqrt(magic);
            dlat = (dlat * 180.0) / ((_a * (1 - _ee)) / (magic * sqrtmagic) * _pi);
            dlng = (dlng * 180.0) / (_a / sqrtmagic * Math.Cos(radlat) * _pi);
            double mglat = lat + dlat;
            double mglng = lng + dlng;
            return new double[] { Math.Round(lng * 2 - mglng, 4), Math.Round(lat * 2 - mglat, 4) };
        }

        /// <summary>
        /// 纬度转换
        /// </summary>
        /// <returns></returns>
        private static double TransformLat2(double lat, double lon)
        {
            double ret = -100.0 + 2.0 * lat + 3.0 * lon + 0.2 * lon * lon + 0.1 * lat * lon + 0.2 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * _pi) + 20.0 * Math.Sin(2.0 * lat * _pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lon * _pi) + 40.0 * Math.Sin(lon / 3.0 * _pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lon / 12.0 * _pi) + 320 * Math.Sin(lon * _pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 经度转换
        /// </summary>
        /// <returns></returns>
        private static double TransformLon2(double lat, double lon)
        {
            double ret = 300.0 + lat + 2.0 * lon + 0.1 * lat * lat + 0.1 * lat * lon + 0.1 * Math.Sqrt(Math.Abs(lat));
            ret += (20.0 * Math.Sin(6.0 * lat * _pi) + 20.0 * Math.Sin(2.0 * lat * _pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * _pi) + 40.0 * Math.Sin(lat / 3.0 * _pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lat / 12.0 * _pi) + 300.0 * Math.Sin(lat / 30.0 * _pi)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 判断是否在国内 不在国内则不做偏移
        /// </summary>
        /// <returns></returns>
        private static bool IsOutOfChina(double lng, double lat)
        {
            return (lng < 72.004 || lng > 137.8347) || ((lat < 0.8293 || lat > 55.8271) || false);
        }
    }
}
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TicketBooking.Infrastructure.Payments
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            StringBuilder query = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // Dùng EscapeDataString để xử lý ký tự đặc biệt chuẩn xác nhất
                    string encodedKey = WebUtility.UrlEncode(kv.Key);
                    string encodedValue = WebUtility.UrlEncode(kv.Value);

                    data.Append(encodedKey + "=" + encodedValue + "&");
                    query.Append(encodedKey + "=" + encodedValue + "&");
                }
            }

            string queryString = query.ToString();
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1, 1);
            }

            string rawData = data.ToString();
            if (rawData.Length > 0)
            {
                rawData = rawData.Remove(rawData.Length - 1, 1);
            }

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, rawData);
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        // ... (Giữ nguyên các hàm ValidateSignature và HmacSHA512 bên dưới)

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    string encodedKey = WebUtility.UrlEncode(kv.Key);
                    string encodedValue = WebUtility.UrlEncode(kv.Value);
                    data.Append(encodedKey + "=" + encodedValue + "&");
                }
            }
            string rawData = data.ToString();
            if (rawData.Length > 0)
            {
                rawData = rawData.Remove(rawData.Length - 1, 1);
            }
            string myChecksum = HmacSHA512(secretKey, rawData);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
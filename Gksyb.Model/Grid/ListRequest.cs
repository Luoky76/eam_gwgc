using Gksyb.Common;
using Gksyb.Model.Filter;

namespace Gksyb.Model.Grid
{
    public class ListRequest : PageRequest
    {
        /// <summary>
        /// 查询条件
        /// </summary>
        public List<FilterRule> Rules { get; set; }

        /// <summary>
        /// 排序条件
        /// </summary>
        public List<OrderRequest> Orders { get; set; }

        /// <summary>
        /// 是否获取总数
        /// </summary>
        public bool TakeTotal { get; set; }

        public GridRequest ToGridRequest(bool encrypt = false)
        {
            FilterGroup group = null;
            string json = "";
            if (Rules?.Count > 0)
            {
                group = new FilterGroup() { Rules = Rules };
                json = group.ToMiniJson();
            }
            var request = new GridRequest
            {
                Page = Page,
                PageSize = PageSize,
                ChangePage = TakeTotal ? "" : "1",
                Where = encrypt ? CryptographyHelper.EncryptFront(json) : json
            };
            var last = Orders?.LastOrDefault();
            if (last != null)
            {
                request.SortOrder = encrypt ? CryptographyHelper.EncryptFront(last.OrderType) : last.OrderType;
                request.SortName = Orders.SkipLast(1).Select(c => c.GetOrderBy()).ToStr(",");
                request.SortName = $"{request.SortName},{last.Field}".TrimStart(',');
                request.SortName = encrypt ? CryptographyHelper.EncryptFront(request.SortName) : request.SortName;
            }
            return request;
        }

    }
}

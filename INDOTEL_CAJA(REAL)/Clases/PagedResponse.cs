using System.Collections.Generic;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class PagedResponse<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new List<T>();
    }
}

using System;

namespace Nesoft.SearchEngine
{
    public class PagingInfo
    {
        private int m_PageNumber;
        private int m_PageSize;

        public PagingInfo()
            : this(1, 10)
        {
        }

        public PagingInfo(int pageNumber, int pageSize)
        {
            m_PageNumber = pageNumber;
            m_PageSize = pageSize;
        }

        public int PageIndex
        {
            get
            {
                return (PageNumber - 1) > 0 ? PageNumber - 1 : 0;
            }
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
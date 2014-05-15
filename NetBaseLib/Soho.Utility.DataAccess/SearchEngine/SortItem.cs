using System;

namespace Soho.Utility.DataAccess.SearchEngine
{
    public class SortItem
    {
        private string sortKey;
        private SortOrderType type;

        public string SortKey
        {
            get { return this.sortKey; }
            set { this.sortKey = value; }
        }

        public SortOrderType SortType
        {
            get { return this.type; }
            set { this.type = value; }
        }
    }
}
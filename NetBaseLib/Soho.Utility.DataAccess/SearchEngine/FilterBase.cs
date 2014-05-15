namespace Soho.Utility.DataAccess.SearchEngine
{
    public class FilterBase
    {
        #region Field

        protected string field;

        #endregion Field

        #region Property

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Field
        {
            get { return field; }
            set { field = value; }
        }

        #endregion Property
    }
}
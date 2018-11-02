namespace DtpCore.Commands
{
    public class QueryCommand
    {
        //public virtual string SortOrder { get; set; }
        //public virtual string SortField { get; set; }
        //public virtual string CurrentFilter { get; set; }
        //public virtual string SearchString { get; set; }
        public virtual int? PageIndex { get; set; }
        public virtual int? PageSize { get; private set; }

    }
}

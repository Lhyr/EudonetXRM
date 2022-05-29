namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public interface ICatalogValue
    {
        string Code { get; set; }
        string DbValue { get; set; }
        string DisplayLabel { get; set; }
        bool Hidden { get; set; }
        int ParentId { get; set; }
        string ToolTipText { get; set; }
    }
}
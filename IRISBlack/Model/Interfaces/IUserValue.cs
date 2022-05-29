using System.Collections.Generic;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public interface IUserValue
    {
        List<IUserValue> ChildrensUserListItem { get; set; }
        bool Disabled { get; set; }
        string GroupId { get; set; }
        string GroupLevel { get; set; }
        bool Hidden { get; set; }
        bool IsChild { get; set; }
        string ItemCode { get; set; }
        string Label { get; set; }
        int Level { get; }
        bool Selected { get; set; }
        int Type { get; set; }
    }
}
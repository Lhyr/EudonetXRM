using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class FieldsMenuShortcutModel
    {
        
        public PhoneShortcutModel PhoneShortcut { get; set; }
        public EmailShortcutModel EmailShortcut { get; set; }
        public HyperLinkShortcutModel HyperLinkShortcut { get; set; }
        public IEnumerable<SocialNetworkShortcutModel> SocialNetworkShortcut { get; set; }
        public GeoLocationShortcutModel GeoLocationShortcut { get; set; }
        public VCardShortcutModel VCardShortcut { get; set; }
    }
}
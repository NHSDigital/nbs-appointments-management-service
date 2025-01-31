using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Okta;

public class OktaConfiguration
{
    public string Domain { get; set; }
    public string ManagementId { get; set; }
    public string ManagementSecret { get; set; }
    public string PrivateKeyP { get; set; }
    public string PrivateKeyQ { get; set; }
    public string PrivateKeyD { get; set; }
    public string PrivateKeyE { get; set; }
    public string PrivateKeyKid { get; set; }
    public string PrivateKeyQi { get; set; }
    public string PrivateKeyDp { get; set; }
    public string PrivateKeyDq { get; set; }
}


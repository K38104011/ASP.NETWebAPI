//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PingYourPackage.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserInRole
    {
        public System.Guid Key { get; set; }
        public System.Guid UserKey { get; set; }
        public System.Guid RoleKey { get; set; }
    
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}

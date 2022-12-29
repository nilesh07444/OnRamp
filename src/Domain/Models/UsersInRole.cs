using System;

namespace Domain.Models
{
    public class UsersInRole : DomainObject
    {
        //public int UserId { get; set; }        
        public virtual User User { get; set; }
        public Guid RoleID { get; set; }
    }
}
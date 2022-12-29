using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;

namespace Domain.Models
{
    public class StandardUserGroup : DomainObject
    {
        private ISet<User> _users;
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public bool IsforSelfSignUpGroup { get; set; }
        public virtual ISet<User> UserList
        {
            get { return _users ?? (_users = new LinkedHashSet<User>()); }
            set { _users = value; }
        }
    }
}
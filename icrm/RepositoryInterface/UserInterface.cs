using System;
using System.Collections.Generic;
using icrm.Models;
namespace icrm.RepositoryInterface
{
    public interface UserInterface : IDisposable
    {
        IEnumerable<ApplicationUser> GetAllAvailableUsers(String role);
        ApplicationUser findUserOnId(string id);
        bool Update(ApplicationUser user);
    }
}
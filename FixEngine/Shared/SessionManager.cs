using FixEngine.Entity;
using FixEngine.Models;
using FixEngine.Resources;
using FixEngine.Services;
using System.Collections.Concurrent;

namespace FixEngine.Shared
{
    public class SessionManager
    {
        private ConcurrentDictionary<string,Session> _session = new();
        public void AddSession(string token, UserResource user)
        {
            if(!IsExist(token)) { 
                _session.TryAdd(token, new Session() { 
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                });
            }

        }
        public void RemoveSession(string token) {
            if (_session.ContainsKey(token))
            {
                _session.TryRemove(token, out Session data);
            }
        }
        public Session GetSession(string token)
        {
            if(IsExist(token))
                return _session[token];
            
            return null;
        }
        public bool IsExist(string token)
        {
            return _session.ContainsKey(token);
        }
    }
}

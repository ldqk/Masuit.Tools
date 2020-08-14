using System.Collections.Generic;
using System.Web;

namespace Masuit.Tools.Test.Mvc.Mocks
{
    public class MockHttpSession : HttpSessionStateBase
    {
        private readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
        private string _sessionId = "0000-0000";

        public override object this[string name]
        {
            get
            {
                _sessionStorage.TryGetValue(name, out var val);
                return val;
            }
            set => _sessionStorage[name] = value;
        }

        public void SetSessionId(string sessionId)
        {
            _sessionId = sessionId;
        }

        public override string SessionID => _sessionId;

        public override void Remove(string name)
        {
            if (_sessionStorage.ContainsKey(name))
            {
                _sessionStorage.Remove(name);
            }
        }
    }
}
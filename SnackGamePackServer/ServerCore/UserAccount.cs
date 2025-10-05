using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnackGamePackServer.ServerCore
{
    public class UserAccount
    {
        private static Int32 _dummyClientIndex = 1;

        public static String IssueDummyClientId() => $"<DummyClient->{_dummyClientIndex++}";
    }
}

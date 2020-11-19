using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleApi.Helpers
{
    public static class ApiRoutes
    {
        public const string Root = "https://localhost:5001";
        public const string Base = Root + "/api";

        public static class Identity
        {
            public const string Token = Base + "/token";
        }

        public static class Numbers
        {
            public const string GetAllBasic = Base + "/basic/number";

            public const string GetAllJwt = Base + "/jwt/numberList";
        }
    }
}

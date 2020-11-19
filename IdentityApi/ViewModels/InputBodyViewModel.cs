using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.ViewModels
{
    public class InputBodyViewModel
    {
        public string Method { get; set; }
        public string Channel { get; set; }
        public string Path { get; set; }
    }
}

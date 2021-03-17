using Microsoft.AspNetCore.Mvc;

namespace CheckLogServer.Filter
{
    public class LoginRequiredAttribute : TypeFilterAttribute
    {
        public LoginRequiredAttribute() : base(typeof(LoginFilter)) { }
    }
}

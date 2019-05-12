using System;
using Microsoft.Owin;
using Owin;



[assembly: OwinStartup(typeof(GraphTutorial.Startup))]

namespace GraphTutorial
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          ConfigureAuth(app);
        }
        
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(GraphTutorial.Startup))]

namespace GraphTutorial
{
    partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }           
    }
}


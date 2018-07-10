using System;
using System.Net;
using System.Net.Http;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DalSoft.RestClient.Examples
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRestClient("https://api.github.com", new Headers(new { UserAgent = "DalSoft.RestClient" }))
                .HttpClientBuilder.ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                });

            services.AddRestClient("NamedGitHubClient", "https://api.github.com/orgs/", new Headers(new { UserAgent = "DalSoft.RestClient" }));
            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

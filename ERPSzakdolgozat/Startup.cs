﻿using ERPSzakdolgozat.Helpers;
using ERPSzakdolgozat.Models;
using ERPSzakdolgozat.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;

namespace ERPSzakdolgozat
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			// To use in-memory Database for better demoing
			services.AddDbContext<ERPDBContext>(opt => opt.UseInMemoryDatabase("ERPDB"));

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			// TODO dont know how this works... need to change options to something
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/auth/login";
					options.AccessDeniedPath = "/auth/accessdenied";
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
				options.AddPolicy("HR", policy => policy.RequireClaim(ClaimTypes.Role, "HR"));
			});

			services.AddSingleton<IClaimsTransformation, ClaimsTransformer>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			// For seeding the database
			var context = serviceProvider.GetService<ERPDBContext>();
			context.Database.EnsureCreated();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}